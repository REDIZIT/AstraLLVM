public class VM
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
            case OpCode.Variable_SetValue: VariableSetValue(); break;
            case OpCode.Math: Math(); break;
            default: throw new NotImplementedException($"There is no implementation for {opCode} opcode");
        }
    }

    private void Math()
    {
        int mode = NextInt();
        int resultAddress = NextAddress();
        int aAddress = NextAddress();
        int bAddress = NextAddress();

        int aValue = ReadInt(heap, aAddress);
        int bValue = ReadInt(heap, bAddress);
        WriteInt(heap, resultAddress, aValue + bValue);
    }

    private void VariableSetValue()
    {
        int mode = NextInt();
        int sizeInBytes = NextInt();
        int destVariable = NextAddress();
        

        if (mode == 0)
        {
            int valueVariable = NextAddress();
            Copy(heap, heap, valueVariable, destVariable, sizeInBytes);
        }
        else
        {
            byte[] value = NextRange(sizeInBytes);
            Write(heap, destVariable, value);
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
            int argumentRbpOffset = stackPointer - sizeof(int);
            int pointer = ReadInt(stack, argumentRbpOffset);
            int value = ReadInt(heap, pointer);
            
            Print(value);
        }
        else
        {
            throw new Exception($"Unknown external function with inModuleIndex = {inModuleIndex}");
        }
    }


    private void Print(int number)
    {
        Console.WriteLine(number);
    }
    

    private int NextInt()
    {
        return BitConverter.ToInt32(NextRange(4));
    }

    private int NextAddress()
    {
        int rbp = NextInt();
        return basePointer + rbp;
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

    private void WriteInt(byte[] arr, int address, int value)
    {
        Write(arr, address, BitConverter.GetBytes(value));
    }

    private int ReadInt(byte[] arr, int address)
    {
        return BitConverter.ToInt32(arr, address);
    }

    private void Copy(byte[] source, byte[] destination, int sourceAddress, int destinationAddress, int sizeInBytes)
    {
        for (int i = 0; i < sizeInBytes; i++)
        {
            destination[destinationAddress + i] = source[sourceAddress + i];
        }
    }
}