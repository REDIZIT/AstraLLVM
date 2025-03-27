public static class Program
{
    public static void Main()
    {
        string exampleCode = """

MyType { int a; long b }

MyFunction()
{
    print()
}

""";
        
        var tokens = Tokenizer.Tokenize(exampleCode);

        var ast = Parser.Parse(tokens);

        Module module = Resolver.Resolve(ast);

        CompiledModule compiled = Generator.Generate(module);

        VM vm = new();
        vm.Run(compiled);
    }
}