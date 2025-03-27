public static class Generator
{
    private static CompiledModule compiled;
    private static Module module;
    
    public static CompiledModule Generate(Module module)
    {
        compiled = new();
        Generator.module = module;

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
    private static void Add(short value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        compiled.code.AddRange(bytes);
    }
    private static void Add(int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        compiled.code.AddRange(bytes);
    }
    private static void Add(long value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        compiled.code.AddRange(bytes);
    }
}

public enum OpCode : byte
{
    Invalid = 0,
    Nop,
    Print,
    InternalCall,
    ExternalCall,
    Allocate_Variable
}