﻿using System.Diagnostics.CodeAnalysis;
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
    public void print_ptr(IntPtr pointerHeapAddress)
    {
        int pointer = vm.heap.ReadInt(pointerHeapAddress.ToInt32());
        int value = vm.heap.ReadInt(pointer);
        
        Console.WriteLine($"<0x{pointer:x8}> = {value}");
    }
}

public class ExportAttribute : Attribute
{
}