public static class Generator
{
    private static CompiledModule compiled;
    private static Module module;

    private static Stack<StaticVariable> staticVariables = new();
    private static Dictionary<string, StaticVariable> staticVariableByName = new();
    private static int staticRbpOffset;
    
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
        if (node is Node_FunctionDeclaration functionDeclaration) FunctionDeclaration(functionDeclaration);
        else if (node is Node_Print print) Print(print);
        else if (node is Node_FunctionCall call) FunctionCall(call);
        else if (node is Node_VariableDeclaration variableDeclaration) VariableDeclaration(variableDeclaration);
        else if (node is Node_VariableAssign variableAssignment) VariableAssignment(variableAssignment);
        else throw new Exception($"Failed to generate due to unexpected node '{node}'");
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
        Add(OpCode.Allocate_Variable);

        TypeInfo type = module.GetType(node.typeName);
        Add(type.sizeInBytes);

        StaticVariable variable = new StaticVariable()
        {
            name = node.variableName,
            rbpOffset = staticRbpOffset,
            sizeInBytes = type.sizeInBytes
        };

        staticRbpOffset += type.sizeInBytes;
        
        staticVariables.Push(variable);
        staticVariableByName.Add(variable.name, variable);
    }

    private static void VariableAssignment(Node_VariableAssign node)
    {
        Add(OpCode.Variable_SetValue);

        if (node.left is Node_Identifier ident)
        {
            StaticVariable variable = staticVariableByName[ident.name];
            
            if (node.value is Node_ConstantNumber constant)
            {
                Add((int)1);
                Add(variable.sizeInBytes);
                Add(variable.rbpOffset);
                AddRange(constant.value, variable.sizeInBytes);
            }
        }
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
        }
        else
        {
            throw new Exception($"Failed to generate {nameof(Node_FunctionCall)} due to unknown functionNode ({node.functionNode})");
        }
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
    Variable_GetValue
}

public class StaticVariable
{
    public string name;
    public int rbpOffset, sizeInBytes;
}