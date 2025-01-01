public static class Compiler
{
    public static string Compile_Astra_to_LLVM(string astraCode)
    {
        Access.Set(Stage.Tokenize);
        List<Token> tokens = Tokenizer.Tokenize(astraCode);

        Access.Set(Stage.AST_parse);
        List<Node> ast = AbstractSyntaxTreeBuilder.Parse(tokens);

        ResolvedModule module = Resolver.DiscoverModule(ast);

        string llvm = Generator.Generate(ast, module);

        return llvm;
    }
}
