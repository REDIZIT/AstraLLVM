public static class Program
{
    public static void Main()
    {
        string exampleCode = """

MyType { int a; long b }

Array<T> { }

MyFunction()
{
    int number = 66
    print(number)
    
    number = 77
    
    ptr pointer = number to ptr
    
    set_int(pointer, 11)

    print_ptr(pointer)
    print(number)
    
    
    int anotherNumber = 47
    print(anotherNumber)
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