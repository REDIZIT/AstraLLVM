public static class Program
{
    public static void Main()
    {
        string exampleCode = """

MyType { int a; long b }

Array<T> { }

MyFunction()
{
    int a = 10 / 2 - 1 * 2

    print(a)
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