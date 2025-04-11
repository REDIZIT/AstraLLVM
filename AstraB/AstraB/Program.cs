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
    set(pointer, 4, 123)
    
    get(pointer, 0)
    get(pointer, 4)
}

set(ptr pointer, int index, int value)
{
    ptr p = pointer + index
    print_ptr(p)
    print(value)
    p ~= value
}

get(ptr pointer, int index)
{
    ptr p = pointer + index
    print_ptr(p)
    
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