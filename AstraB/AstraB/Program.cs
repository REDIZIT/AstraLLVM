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
}

test()
{
    int another = 33
    
    ptr pointer = another to ptr
    pointer ~= 77
    
    print_ptr(pointer)
    
    print(another)
}

onemore()
{
    print(77)
}

""";
        
        var tokens = Tokenizer.Tokenize(exampleCode);

        var ast = Parser.Parse(tokens);

        VM vm = new();
        Module module = Resolver.Resolve(ast, vm);
        
        AstChecker.CheckAndModify(ast);

        CompiledModule compiled = Generator.Generate(module);

        
        vm.Run(compiled);
    }
}