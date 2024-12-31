public static class Compiler
{
    public static string Compile_Astra_to_LLVM(string astraCode)
    {
        List<Token> tokens = Tokenizer.Tokenize(astraCode);

        List<Node> ast = AbstractSyntaxTreeBuilder.Parse(tokens);

        Module module = DiscoverModule(ast);

        string llvm = Generator.Generate(ast, module);

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
            PrimitiveTypeInfo type = new()
            {
                name = "i" + i,
                asmName = "i" + i,
            };
            module.typeInfoByName[type.name] = type;
        }

        PrimitiveTypeInfo.BOOL = (PrimitiveTypeInfo)module.GetType("i1");
        PrimitiveTypeInfo.BYTE = (PrimitiveTypeInfo)module.GetType("i8");
        PrimitiveTypeInfo.SHORT = (PrimitiveTypeInfo)module.GetType("i16");
        PrimitiveTypeInfo.INT = (PrimitiveTypeInfo)module.GetType("i32");
        PrimitiveTypeInfo.LONG = (PrimitiveTypeInfo)module.GetType("i64");


        PrimitiveTypeInfo ptrType = new()
        {
            name = "ptr",
            asmName = "ptr",
        };
        module.typeInfoByName[ptrType.name] = ptrType;
        PrimitiveTypeInfo.PTR = ptrType;
    }
}
