﻿public class VM
{
    private CompiledModule module;

    private int current;

    private byte[] stack, heap;
    private int stackPointer, basePointer;
    private int heapPointer;
    
    public void Run(CompiledModule module)
    {
        this.module = module;
        current = module.functionPointerByID[0];

        stack = new byte[1024];
        heap = new byte[1024];
        stackPointer = 0;
        basePointer = 0;
        

        int opsDone = 0;
        int opsLimit = 1000;

        while (current < module.code.Count)
        {
            if (opsDone >= opsLimit)
            {
                throw new Exception("Operations limit exceeded");
            }

            OpCode opCode = (OpCode)module.code[current];
            current++;
            
            Execute(opCode);
            
            opsDone++;
        }
    }

    private void Execute(OpCode opCode)
    {
        switch (opCode)
        {
            case OpCode.Nop: return;
            case OpCode.Print: Console.WriteLine("Printed!"); break;
            case OpCode.InternalCall: InternalCall(); break;
            case OpCode.ExternalCall: ExternalCall(); break;
            case OpCode.Allocate_Variable: AllocateVariable(); break;
            default: throw new NotImplementedException($"There is no implementation for {opCode} opcode");
        }
    }

    private void AllocateVariable()
    {
        int sizeInBytes = NextInt();

        int heapAddress = heapPointer;
        heapPointer += sizeInBytes;

        Write(stack, stackPointer, BitConverter.GetBytes(heapAddress));
        stackPointer += sizeInBytes;
    }
    
    private void InternalCall()
    {
        int inModuleIndex = NextInt();

        int pointer = module.functionPointerByID[inModuleIndex];
        current = pointer;
    }

    private void ExternalCall()
    {
        int inModuleIndex = NextInt();

        if (inModuleIndex == 0)
        {
            Console.WriteLine("Printed as external call!");
        }
        else
        {
            throw new Exception($"Unknown external function with inModuleIndex = {inModuleIndex}");
        }
    }

    private int NextInt()
    {
        return BitConverter.ToInt32(NextRange(4));
    }

    private byte[] NextRange(int count)
    {
        byte[] bytes = module.code.Slice(current, count).ToArray();
        current += count;
        return bytes;
    }

    private void Write(byte[] arr, int address, byte[] value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            arr[address + i] = value[i];
        }
    }
}