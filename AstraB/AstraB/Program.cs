public static class Program
{
    public static void Main()
    {
        string exampleCode = """

MyType { int a; long b }

Array<T> { }

MyFunction()
{
    ptr pointer = alloc_arr(4)
    
    Array<int> arr = pointer

    arr.set(0, 11)
    arr.set(1, 22)
    arr.set(2, 33)
    
    arr.get(0)
    arr.get(1)
    arr.get(2)
    arr.get(3)
    arr.get(4)
    arr.get(5)
    arr.get(6)
    arr.get(7)
}

alloc_arr(int length) -> ptr
{
    ptr pointer = alloc(4 + length * 4)
    pointer ~= length
    return pointer
}

set(Array self, int index, int value)
{
    ptr pointer = self
    pointer + 4 + index * 4 ~= value
}

get(Array self, int index)
{
    if index >= self.get_len() panic()
    ptr p = self + 4 + index * 4
    print_ptr(p)
}

get_len(Array self) -> int
{
    ptr pointer = self
    int len = pointer to int
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