public static class Generator
{
    private static CompiledModule compiled;
    private static Module module;

    private static Stack<StaticVariable> staticVariables = new();
    private static Dictionary<string, StaticVariable> staticVariableByName = new();
    private static int staticRbpOffset;

    private static int tempNameIndex;
    
    public static CompiledModule Generate(Module module)
    {
        compiled = new();
        Generator.module = module;
        staticVariables.Clear();
        staticRbpOffset = 0;

        foreach (FunctionInfo function in module.functions)
        {
            int pointer = compiled.code.Count;
            int id = compiled.functionPointerByID.Count;
            compiled.functionPointerByID.Add(id, pointer);
            
            Generate(function.node);
        }

        return compiled;
    }

    private static void Generate(Node node)
    {
        switch (node)
        {
            case Node_FunctionDeclaration functionDeclaration:
                FunctionDeclaration(functionDeclaration);
                break;
            case Node_Print print:
                Print(print);
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
        
        Add(OpCode.Math);
        Add((int)0);
        Add(node.result.rbpOffset);
        Add(node.left.result.rbpOffset);
        Add(node.right.result.rbpOffset);
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
        node.result = AllocateVariable(node.typeName, node.variableName);
    }

    private static StaticVariable AllocateVariable(string typeName, string variableName)
    {
        TypeInfo type = module.GetType(typeName);
        return AllocateVariable(type, variableName);
    }
    private static StaticVariable AllocateVariable(TypeInfo type, string variableName)
    {
        Add(OpCode.Allocate_Variable);
        Add(type.sizeInBytes);

        StaticVariable variable = new StaticVariable()
        {
            name = variableName,
            rbpOffset = staticRbpOffset,
            sizeInBytes = type.sizeInBytes,
            type = type,
        };

        staticRbpOffset += type.sizeInBytes;
        
        staticVariables.Push(variable);
        staticVariableByName.Add(variable.name, variable);

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
        Add(OpCode.Variable_SetValue);
        Add((int)0);
        Add(dest.sizeInBytes);
        Add(dest.rbpOffset);
        Add(value.rbpOffset);
    }

    private static void SetValue_Var_Const(StaticVariable dest, byte[] value)
    {
        Add(OpCode.Variable_SetValue);
        Add((int)1);
        Add(dest.sizeInBytes);
        Add(dest.rbpOffset);
        AddRange(value, dest.sizeInBytes);
    }

    private static void Print(Node_Print node)
    {
        Add(OpCode.Print);
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

            // Generate arguments nodes
            for (int i = 0; i < info.parameters.Count; i++)
            {
                FieldInfo paramInfo = info.parameters[i];
                Node argumentNode = node.passedArguments[i];

                Generate(argumentNode);
                if (argumentNode.result.type != paramInfo.type)
                {
                    throw new Exception($"Failed to generate function '{info.name}' due to invalid passed argument type. Expected '{paramInfo.type.name}' at argument with index {i}, but got '{argumentNode.result.type.name}'");
                }
            }
            
            // Allocate (duplicate) arguments
            for (int i = 0; i < info.parameters.Count; i++)
            {
                Node argumentNode = node.passedArguments[i];

                StaticVariable argumentVariable = AllocateVariable(argumentNode.result.type, NextTempName());
                SetValue_Var_Var(argumentVariable, argumentNode.result);
            }

            if (info.module == module)
            {
                Add(OpCode.InternalCall);
                Add(info.inModuleIndex);
            }
            else
            {
                Add(OpCode.ExternalCall);
                Add(info.inModuleIndex);
            }
            
            // Deallocate arguments
            // TODO: 
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

    private static void Add(OpCode code)
    {
        Add((byte)code);
    }
    private static void Add(byte value)
    {
        compiled.code.Add(value);
    }
    private static void AddRange(byte[] value, int padSize = 0)
    {
        compiled.code.AddRange(value);

        for (int i = 0; i < padSize - value.Length; i++)
        {
            compiled.code.Add((byte)0);
        }
    }
    private static void Add(short value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        AddRange(bytes);
    }
    private static void Add(int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        AddRange(bytes);
    }
    private static void Add(long value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        AddRange(bytes);
    }
}

public enum OpCode : byte
{
    Invalid = 0,
    Nop,
    Print,
    InternalCall,
    ExternalCall,
    Allocate_Variable,
    Variable_SetValue,
    Math
}

public class StaticVariable
{
    public string name;
    public int rbpOffset, sizeInBytes;
    public TypeInfo type;
}