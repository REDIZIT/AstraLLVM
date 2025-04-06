public static class Program
{
    public static void Main()
    {
        string exampleCode = """

MyType { int a; long b }

Array<T> { }

MyFunction()
{
    int number = 22
    print(number)
    test()
    return
}

test()
{
    int another = 33
    print(another)
    return
}

onemore()
{
    print(77)
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