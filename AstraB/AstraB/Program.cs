public static class Program
{
    public static void Main()
    {
        string exampleCode = """

MyType { int a; long b }

Array<T> { }

MyFunction()
{
    ptr pointer = alloc(16)
    Array<int> arr = pointer to Array<int>
    
    pointer ~= 77
    get(pointer)
    set(pointer, 123)
    get(pointer)
}

set(ptr pointer, int value)
{
    print_ptr(pointer)
    print(value)
    pointer ~= value
}

get(ptr pointer)
{
    print_ptr(pointer)
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