public static class Program
{
    public static void Main()
    {
        string exampleCode = """

MyType { int a; long b }

Array<T> { }

MyFunction()
{
    int a = 1
    int b = 2
    int c = 3
    int d = 4
    
    ptr pointer = a to ptr
    pointer ~= 11
    
    pointer = b to ptr
    pointer ~= 22
    
    pointer = pointer + 8
    pointer ~= 33

    print(a)
    print(b)
    print(c)
    print(d)
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