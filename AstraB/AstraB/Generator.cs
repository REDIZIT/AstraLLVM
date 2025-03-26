public static class Generator
{
    private static CompiledModule compiled;
    
    public static CompiledModule Generate(Module module)
    {
        compiled = new();

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

    private static void Print(Node_Print node)
    {
        Add(OpCode.Print);
    }

    private static void Add(OpCode code)
    {
        compiled.code.Add((byte)code);
    }
}

public enum OpCode : byte
{
    Invalid = 0,
    Nop,
    Print
}