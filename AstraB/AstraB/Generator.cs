public static class Generator
{
    private static CompiledModule compiled;
    private static Module module;
    private static List<Instruction> instructions;
    
    private static Scope_GenerationPhase currentScope;
    private static int tempNameIndex;
    
    public static CompiledModule Generate(Module module)
    {
        compiled = new();
        instructions = new();
        Generator.module = module;
        currentScope = new();

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
        ident.result = currentScope.GetVariable(ident.name);
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
        StaticVariable variable = currentScope.RegisterLocalVariable(type, variableName);
        Add(new AllocateVariable_Instruction(type.SizeInBytes).WithDebug(variable));

        return variable;
    }

    private static void VariableAssignment(Node_VariableAssign node)
    {
        Generate(node.left);
        StaticVariable variable = node.left.result;
        
        Generate(node.value);
            
        SetValue_Var_Var(variable, node.value.result);
    }

    private static void SetValue_Var_Var(StaticVariable dest, StaticVariable value)
    {
        ScopeRelativeRbpOffset destOffset = currentScope.GetRelativeRBP(dest);
        ScopeRelativeRbpOffset valueOffset = currentScope.GetRelativeRBP(value);
        Add(SetValue_Instruction.Variable_to_Variable(destOffset, valueOffset, dest.sizeInBytes));
    }

    private static void SetValue_Var_Const(StaticVariable dest, byte[] value)
    {
        ScopeRelativeRbpOffset destOffset = currentScope.GetRelativeRBP(dest);
        Add(SetValue_Instruction.Const_to_Variable(destOffset, value));
    }
    
    private static void SetValue_Var_Ptr(StaticVariable dest, StaticVariable value)
    {
        ScopeRelativeRbpOffset destOffset = currentScope.GetRelativeRBP(dest);
        ScopeRelativeRbpOffset valueOffset = currentScope.GetRelativeRBP(value);
        Add(SetValue_Instruction.Pointer_to_Variable(destOffset, valueOffset, dest.sizeInBytes));
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
            
            
            Add(new Debug_Instruction($"Allocate arguments to pass"));
            
            // Add(new Scope_Instruction(true));
            // currentScope = currentScope.CreateSubScope();
            
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
                
                Add(new Debug_Instruction($"Sus: copy from {currentScope.GetRelativeRBP(argumentNode.result)} to {currentScope.GetRelativeRBP(argumentVariable)}"));
                
                SetValue_Var_Var(argumentVariable, argumentNode.result);
            }
            
            Add(new FunctionCall_Instruction(info.inModuleIndex, info.module != module));
            
            
            Add(new Debug_Instruction($"Deallocate"));
            
            // currentScope = currentScope.parent;
            // Add(new Scope_Instruction(false));
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
    public Scope_GenerationPhase scope;
}