public static class Program
{
    public static void Main()
    {
        string exampleCode = """

MyType { int a; long b }

Array<T> { }

MyFunction()
{
    ptr a = alloc(16)
    print_ptr(a)
    
    ptr b = alloc(16)
    print_ptr(b)

    
    int number = 123
    print(number)
    
    ptr c = number to ptr
    print_ptr(c)
    
    print(number)
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