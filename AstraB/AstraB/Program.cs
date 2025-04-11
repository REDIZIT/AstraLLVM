public static class Program
{
    public static void Main()
    {
        string exampleCode = """

MyType { int a; long b }

Array<T> { }

MyFunction()
{
    ptr pointer = alloc_arr(3)
    
    Array<int> arr = pointer to Array<int>
    
    print(get_len(arr))
    
    set(pointer, 0, 11)
    set(pointer, 1, 22)
    set(pointer, 2, 33)
    --set(pointer, 3, 44)
    
    get(pointer, 0)
    get(pointer, 1)
    get(pointer, 2)
    get(pointer, 3)
    get(pointer, 4)
    get(pointer, 5)
    get(pointer, 6)
    get(pointer, 7)
}

alloc_arr(int length) -> ptr
{
    ptr pointer = alloc(4 + length * 4)
    pointer ~= length
    return pointer
}

set(ptr pointer, int index, int value)
{
    pointer + 4 + index * 4 ~= value
}

get(ptr pointer, int index)
{
    ptr p = pointer + 4 + index * 4
    print_ptr(p)
}

get_len(Array self) -> int
{
    int len = self to ptr to int
    return len
}

""";


        string anotherExample = """

main()
{
    int value = 234
    print(value)
    
    if value > 1
    {
        int a
        int b
        print(11)
        print(22)
    }
    
    int anotherValue = 77
    print(value)
    print(anotherValue)
}

""";

        Lexer lexer = new();
        var tokens = lexer.Tokenize(exampleCode, includeSpacesAndEOF: false);

        var ast = Parser.Parse(tokens);

        VM vm = new();
        Module module = Resolver.Resolve(ast, vm);
        
        AstChecker.CheckAndModify(ast);

        CompiledModule compiled = Generator.Generate(module);
        
        vm.Run(compiled);
    }
}