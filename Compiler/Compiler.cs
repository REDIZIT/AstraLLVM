public static class Compiler
{
    public static string Compile_Astra_to_LLVM(string astraCode)
    {
        List<Token> tokens = Tokenizer.Tokenize(astraCode);

        List<Node> ast = AbstractSyntaxTreeBuilder.Parse(tokens);

        Module module = DiscoverModule(ast);

        string llvm = Generator.Generate(ast);

        return llvm;
    }

    private static Module DiscoverModule(List<Node> ast)
    {
        Module module = new();

        RegisterLLVMTypes(module);
        foreach (Node node in ast)
        {
            node.RegisterRefs(module);
        }

        foreach (Node node in ast)
        {
            node.ResolveRefs(module);
        }

        return module;
    }
    private static void RegisterLLVMTypes(Module module)
    {
        for (int i = 1; i <= 64; i++)
        {
            TypeInfo type = new()
            {
                asmName = "i" + i,
                astraName = "i" + i
            };
            module.typeInfoByName[type.astraName] = type;
        }
    }
}
