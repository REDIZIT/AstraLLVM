public static class Compiler
{
    public static string Compile_Astra_to_LLVM(string astraCode)
    {
        List<Token> tokens = Tokenizer.Tokenize(astraCode);

        var root = AbstractSyntaxTreeBuilder.Parse(tokens);

        string llvm = Generator.Generate(root);

        return llvm;
    }
}
