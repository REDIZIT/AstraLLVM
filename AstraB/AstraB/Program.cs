public static class Program
{
    public static void Main()
    {
        string exampleCode = """

MyType { int a; long b }

MyFunction()
{
    int a
    int b
    long c
    
    a = 42
    b = 777
    c = 1827455253
    
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