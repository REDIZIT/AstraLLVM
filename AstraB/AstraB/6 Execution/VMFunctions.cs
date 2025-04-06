using System.Reflection;

public class VMFunctions
{
    public VM vm;

    private List<MethodInfo> methods = new();
    
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
    public void Print(int heapAddress)
    {
        int number = vm.heap.ReadInt(heapAddress);
        
        Console.WriteLine(number);
    }

    [Export]
    public void B(int pointerAddress, int valueAddress)
    {
        int value = vm.heap.ReadInt(valueAddress);
        int targetAddress = vm.heap.ReadInt(pointerAddress);
        
        vm.heap.WriteInt(targetAddress, value);
            
        Console.WriteLine($"set_int at {targetAddress}, value at {valueAddress} = {value}");
    }

    [Export]
    public void C(int pointer, int returnPointer)
    {
        int value = vm.heap.ReadInt(pointer);
            
        Console.WriteLine($"Get value at {pointer})");
    }

    [Export]
    public void Print_Ptr(HeapAddress pointerHeapAddress)
    {
        int pointer = vm.heap.ReadInt(pointerHeapAddress);
        int value = vm.heap.ReadInt(pointer);
        
        Console.WriteLine($"<0x{pointer:x8}> = {value}");
    }
}

public class ExportAttribute : Attribute
{
}