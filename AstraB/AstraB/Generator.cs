public static class Generator
{
    private static CompiledModule compiled;
    private static Module module;
    private static List<Instruction> instructions;

    private static Stack<StaticVariable> staticVariables = new();
    private static Dictionary<string, StaticVariable> staticVariableByName = new();
    private static int staticRbpOffset;

    private static int tempNameIndex;
    
    public static CompiledModule Generate(Module module)
    {
        compiled = new();
        instructions = new();
        Generator.module = module;
        staticVariables.Clear();
        staticRbpOffset = 0;

        //
        // Generate instructions
        //
        foreach (FunctionInfo function in module.functions)
        {
            int pointer = compiled.code.Count;
            int id = compiled.functionPointerByID.Count;
            compiled.functionPointerByID.Add(id, pointer);
            
            Generate(function.node);
        }
        
        //
        // Encode instruction into byte-code
        //
        InstructionEncoder encoder = new();
        foreach (Instruction instruction in instructions)
        {
            instruction.Encode(encoder);
        }

        compiled.code = encoder.code;

        return compiled;
    }

    private static void Generate(Node node)
    {
        switch (node)
        {
            case Node_FunctionDeclaration functionDeclaration:
                FunctionDeclaration(functionDeclaration);
                break;
            case Node_FunctionCall call:
                FunctionCall(call);
                break;
            case Node_VariableDeclaration variableDeclaration:
                VariableDeclaration(variableDeclaration);
                break;
            case Node_VariableAssign variableAssignment:
                VariableAssignment(variableAssignment);
                break;
            case Node_Binary binary:
                Binary(binary);
                break;
            case Node_ConstantNumber constantNumber:
                Constant(constantNumber);
                break;
            case Node_Identifier ident:
                LoadVariable(ident);
                break;
            case Node_CastTo cast:
                Cast(cast);
                break;
            default:
                throw new Exception($"Failed to generate due to unexpected node '{node}'");
        }
    }

    private static void LoadVariable(Node_Identifier ident)
    {
        ident.result = staticVariableByName[ident.name];
    }

    private static void Constant(Node_ConstantNumber constant)
    {
        constant.result = AllocateVariable(constant.typeName, NextTempName());
        SetValue_Var_Const(constant.result, constant.value);
    }

    private static void Binary(Node_Binary node)
    {
        Generate(node.left);
        Generate(node.right);
        
        TypeInfo resultType = module.GetType(node.tokenOperator.ResultType);
        node.result = AllocateVariable(resultType.name, NextTempName());
        
        Add(new Math_Instruction(node.left.result.rbpOffset, node.right.result.rbpOffset, node.result.rbpOffset));
    }

    private static void Cast(Node_CastTo node)
    {
        Generate(node.valueToCast);

        TypeInfo targetType = module.GetType(node.typeName);

        if (targetType.name == "ptr")
        {
            node.result = AllocateVariable(targetType, NextTempName());
            SetValue_Var_Ptr(node.result, node.valueToCast.result);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private static void FunctionDeclaration(Node_FunctionDeclaration node)
    {
        Block(node.block);
    }

    private static void Block(Node_Block node)
    {
        foreach (Node children in node.children)
        {
            Generate(children);
        }
    }

    private static void VariableDeclaration(Node_VariableDeclaration node)
    {
        node.result = AllocateVariable(node.typeName, node.concreteGenericTypes, node.variableName);
    }

    private static StaticVariable AllocateVariable(string typeName, string variableName)
    {
        return AllocateVariable(typeName, null, variableName);
    }
    private static StaticVariable AllocateVariable(string typeName, List<Token_Identifier> concreteGenericTypes, string variableName)
    {
        TypeInfo type = module.GetType(typeName);

        if (type.IsGeneric)
        {
            GenericImplementationInfo genericType = module.GetGeneric(type, concreteGenericTypes.Select(t => module.GetType(t.name)));
            return AllocateVariable(genericType, variableName);
        }
        else
        {
            return AllocateVariable(type, variableName);
        }
    }
    private static StaticVariable AllocateVariable(ITypeInfo type, string variableName)
    {
        StaticVariable variable = new StaticVariable()
        {
            name = variableName,
            rbpOffset = staticRbpOffset,
            sizeInBytes = type.SizeInBytes,
            type = type,
        };

        staticRbpOffset += type.SizeInBytes;
        
        staticVariables.Push(variable);
        staticVariableByName.Add(variable.name, variable);
        
        Add(new AllocateVariable_Instruction(type.SizeInBytes));

        return variable;
    }

    private static void VariableAssignment(Node_VariableAssign node)
    {
        if (node.left is Node_Identifier ident)
        {
            StaticVariable variable = staticVariableByName[ident.name];

            Generate(node.value);
            
            SetValue_Var_Var(variable, node.value.result);
            
            // if (node.value is Node_ConstantNumber constant)
            // {
            //     SetValue_Var_Const(variable, constant.value);
            // }
            // else if (node.value is Node_Identifier valueVariableIdent)
            // {
            //     StaticVariable valueVariable = staticVariableByName[valueVariableIdent.name];
            //     SetValue_Var_Var(variable, valueVariable);
            // }
            // else if (node.value is Node_Binary binary)
            // {
            //     Binary();
            // }
            // else
            // {
            //     throw new Exception($"Unknown value node type {node} inside {nameof(VariableAssignment)}");
            // }
        }
    }

    private static void SetValue_Var_Var(StaticVariable dest, StaticVariable value)
    {
        Add(SetValue_Instruction.Variable_to_Variable(dest.rbpOffset, value.rbpOffset, dest.sizeInBytes));
    }

    private static void SetValue_Var_Const(StaticVariable dest, byte[] value)
    {
        Add(SetValue_Instruction.Const_to_Variable(dest.rbpOffset, value));
    }
    
    private static void SetValue_Var_Ptr(StaticVariable dest, StaticVariable value)
    {
        Add(SetValue_Instruction.Pointer_to_Variable(dest.rbpOffset, value.rbpOffset, dest.sizeInBytes));
    }

    private static void FunctionCall(Node_FunctionCall node)
    {
        if (node.functionNode is Node_Identifier ident)
        {
            FunctionInfo info = module.GetFunction(ident.name);

            
            if (info.parameters.Count != node.passedArguments.Count)
            {
                throw new Exception($"Failed to generate function '{info.name}' due to different count of passed ({node.passedArguments.Count}) and required ({info.parameters.Count}) arguments");
            }

            // Generate placeholders for returns
            if (info.returns != null)
            {
                if (info.returns.Count > 1)
                {
                    throw new NotSupportedException("Only 1 function return variable supported yet.");
                }
                else if (info.returns.Count == 1)
                {
                    FieldInfo retInfo = info.returns[0];
                    node.result = AllocateVariable(retInfo.type, NextTempName());
                }
            }
            
            int staticRbpSaver = staticRbpOffset;

            // Generate arguments nodes
            for (int i = 0; i < info.parameters.Count; i++)
            {
                FieldInfo paramInfo = info.parameters[i];
                Node argumentNode = node.passedArguments[i];

                Generate(argumentNode);
                if (argumentNode.result.type != paramInfo.type)
                {
                    throw new Exception($"Failed to generate function '{info.name}' due to invalid passed argument type. Expected '{paramInfo.type.name}' at argument with index {i}, but got '{argumentNode.result.type.Name}'");
                }
            }
            
            // Allocate (duplicate) arguments
            for (int i = 0; i < info.parameters.Count; i++)
            {
                Node argumentNode = node.passedArguments[i];

                StaticVariable argumentVariable = AllocateVariable(argumentNode.result.type, NextTempName());
                SetValue_Var_Var(argumentVariable, argumentNode.result);
            }
            staticRbpOffset = staticRbpSaver;
            
            Add(new FunctionCall_Instruction(info.inModuleIndex, info.module != module));
        }
        else
        {
            throw new Exception($"Failed to generate {nameof(Node_FunctionCall)} due to unknown functionNode ({node.functionNode})");
        }
    }

    private static string NextTempName()
    {
        tempNameIndex++;
        return "temp_" + tempNameIndex;
    }

    private static void Add(Instruction instruction)
    {
        instructions.Add(instruction);
    }
}

public enum OpCode : byte
{
    Invalid = 0,
    Nop,
    InternalCall,
    ExternalCall,
    Allocate_Variable,
    Variable_SetValue,
    Math,
    BeginScope,
    DropScope,
}

public class StaticVariable
{
    public string name;
    public int rbpOffset, sizeInBytes;
    public ITypeInfo type;
}