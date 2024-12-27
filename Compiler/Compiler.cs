public static class Compiler
{
    public static string Compile_Astra_to_LLVM(string astraCode)
    {
        List<Token> tokens = Tokenizer.Tokenize(astraCode);

        string llvm = Generator.Generate(tokens);

        return llvm;
    }
}
