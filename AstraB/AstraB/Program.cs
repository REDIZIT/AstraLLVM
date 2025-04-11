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
    pointer ~= 3
    
    Array<int> arr = pointer to Array<int>
    get_len(pointer)
    
    set(pointer, 0, 11)
    set(pointer, 1, 22)
    
    get(pointer, 0)
    get(pointer, 1)
    get(pointer, 2)
    get(pointer, 3)
    get(pointer, 4)
    get(pointer, 5)
    get(pointer, 6)
    get(pointer, 7)
}

set(ptr pointer, int index, int value)
{
    pointer + 4 + index * 4 ~= value
}

get(ptr pointer, int index)
{
    int len = get_len(pointer)
    
    if index >= len
    {
        panic()
    }
    
    ptr p = pointer + 4 + index * 4
    print_ptr(p)
}

get_len(ptr pointer) -> int
{
    int len = pointer to int
    return len
}

""";


        string anotherExample = """

main()
{
    int value = test()
    print(value)
}

test() -> int
{
    return 234
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