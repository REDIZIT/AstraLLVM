using System.Reflection;
using AVM;

public class VM
{
    private CompiledModule module;

    private int current;

    public Memory stack, heap;
    private int stackPointer, basePointer;
    private int heapPointer;

    private VMFunctions functions;

    public VM()
    {
        functions = new()
        {
            vm = this
        };
        functions.BakeMethods();
    }
    
    public void Run(CompiledModule module)
    {
        this.module = module;
        current = module.functionPointerByID[0];

        stack = new();
        heap = new();
        stackPointer = 0;
        basePointer = 0;
        

        int opsDone = 0;
        int opsLimit = 1000;

        List<OpCode> ops = new();

        while (current < module.code.Count)
        {
            if (opsDone >= opsLimit)
            {
                throw new Exception("Operations limit exceeded");
            }

            OpCode opCode = (OpCode)module.code[current];
            ops.Add(opCode);
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
            case OpCode.InternalCall: InternalCall(); break;
            case OpCode.ExternalCall: ExternalCall(); break;
            case OpCode.Allocate_Variable: AllocateVariable(); break;
            case OpCode.Variable_SetValue: VariableSetValue(); break;
            case OpCode.Math: Math(); break;
            case OpCode.BeginScope: BeginScope(); break;
            case OpCode.DropScope: DropScope(); break;
            default: throw new NotImplementedException($"There is no implementation for {opCode} opcode");
        }
    }

    private void BeginScope()
    {
        PushInt(basePointer);
        basePointer = stackPointer;
    }
    
    private void DropScope()
    {
        stackPointer = basePointer;
        basePointer = PopInt();
    }

    private void Math()
    {
        int mode = NextInt();
        int resultAddress = NextAddress();
        int aAddress = NextAddress();
        int bAddress = NextAddress();

        int aValue = heap.ReadInt(aAddress);
        int bValue = heap.ReadInt(bAddress);
        heap.WriteInt(resultAddress, aValue + bValue);
    }

    private void VariableSetValue()
    {
        int mode = NextInt();
        int sizeInBytes = NextInt();

        ScopeRelativeRbpOffset destRbpOffset = NextRBP();
        StackAddress destStackAddress = ToAbs(destRbpOffset);
        HeapAddress destHeapAddress = new HeapAddress(stack.ReadInt(destStackAddress));
        

        if (mode == 0)
        {
            // Value
            ScopeRelativeRbpOffset valueRbpOffset = NextRBP();
            int valueStackAddress = ToAbs(valueRbpOffset);
            int valueHeapAddress = stack.ReadInt(valueStackAddress);
            
            heap.Copy(valueHeapAddress, destHeapAddress, (byte)sizeInBytes);
        }
        else if (mode == 1)
        {
            // Const
            byte[] value = NextRange(sizeInBytes);
            heap.Write(destHeapAddress, value);
        }
        else if (mode == 2)
        {
            // Pointer
            // SetValue_Var_Ptr
            int valueVariable = NextAddress();
            heap.WriteInt(destHeapAddress, valueVariable);
        }
        else if (mode == 3)
        {
            // SetValue_Var_To_Ptr
            StackAddress valueVariable = NextAddress();
            int value = stack.ReadInt(valueVariable);
            
            int pointer = heap.ReadInt(destHeapAddress);
            
            heap.Copy(value, pointer, (byte)sizeInBytes);
        }
        else
        {
            throw new Exception($"Invalid variable set value mode ({mode})");
        }
    }
    
    private void AllocateVariable()
    {
        int sizeInBytes = NextInt();

        int heapAddress = heapPointer;
        stack.Write(stackPointer, BitConverter.GetBytes(heapAddress), noLogs: true);

        stack.logger.Log_Allocate(stackPointer, sizeInBytes);
        heap.logger.Log_Allocate(heapAddress, sizeInBytes);
        
        heapPointer += sizeInBytes;
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

        int fakeBasePointer = stackPointer;
        
        MethodInfo methodInfo = functions.GetMethod(inModuleIndex);
        ParameterInfo[] methodParams = methodInfo.GetParameters();
            
        object[] arguments = new object[methodParams.Length];
        int totalArgumentsSize = 0;
        
        for (int i = arguments.Length - 1; i >= 0; i--)
        {
            totalArgumentsSize -= 4;
            int rbpOffset = fakeBasePointer + totalArgumentsSize;
            int value = stack.ReadInt(rbpOffset);
            arguments[i] = value;
        }

        methodInfo.Invoke(functions, arguments);
    }

    
    

    private int NextInt()
    {
        return BitConverter.ToInt32(NextRange(4));
    }

    private ScopeRelativeRbpOffset NextRBP()
    {
        return new ScopeRelativeRbpOffset(NextInt());
    }

    private StackAddress NextAddress()
    {
        ScopeRelativeRbpOffset rbp = NextRBP();
        return ToAbs(rbp);
    }

    private StackAddress ToAbs(ScopeRelativeRbpOffset rbpOffset)
    {
        return new StackAddress(basePointer + rbpOffset);
    }

    private byte[] NextRange(int count)
    {
        byte[] bytes = module.code.Slice(current, count).ToArray();
        current += count;
        return bytes;
    }
    
    public int ReadValueInt(int stackAddress)
    {
        int pointer = stack.ReadInt(stackAddress);
        return heap.ReadInt(pointer);
    }
    
    private void PushInt(int value)
    {
        stack.WriteInt(stackPointer, value);
        stackPointer += 4;
    }

    private int PopInt()
    {
        stackPointer -= 4;
        return stack.ReadInt(stackPointer);
    }
}