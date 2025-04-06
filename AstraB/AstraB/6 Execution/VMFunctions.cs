using System.Diagnostics.CodeAnalysis;
using System.Reflection;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class VMFunctions
{
    public VM vm;
    public List<MethodInfo> methods = new();
    
    public void BakeMethods()
    {
        methods.Clear();
        foreach (MethodInfo methodInfo in GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            if (methodInfo.GetCustomAttribute<ExportAttribute>() == null) continue;
            
            methods.Add(methodInfo);
        }
    }
    
    public MethodInfo GetMethod(int inModuleIndex)
    {
        return methods[inModuleIndex];
    }
    
    [Export]
    public void print(int heapAddress)
    {
        int number = vm.heap.ReadInt(heapAddress);
        
        Console.WriteLine(number);
    }
    
    [Export]
    public void print_ptr(Ptr pointerHeapAddress)
    {
        int pointer = vm.heap.ReadInt(pointerHeapAddress);
        int value = vm.heap.ReadInt(pointer);

        string hex = pointer.ToString("x8");

        ConsoleColor prevColor = Console.ForegroundColor;
        
        Console.Write("<");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("0x");
        
        for (int i = 0; i < hex.Length; i++)
        {
            char digit = hex[i];
            if (digit != '0') Console.ForegroundColor = prevColor;
            
            Console.Write(digit);
        }
        Console.ForegroundColor = prevColor;
        Console.WriteLine($"> = {value}");
    }

    [Export]
    public Ptr alloc(HeapAddress sizeInBytesHeapAddress)
    {
        int sizeInBytes = vm.heap.ReadInt(sizeInBytesHeapAddress);
        int allocatedHeapAddress = vm.Allocate(sizeInBytes);

        return new(allocatedHeapAddress);
    }
}

public class ExportAttribute : Attribute
{
}