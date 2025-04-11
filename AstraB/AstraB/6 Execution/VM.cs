using System.Reflection;
using System.Runtime.InteropServices;
using AVM;

public partial class VM
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
        current = 0;

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

    public void StopExecution(int exitCode)
    {
        if (exitCode != 0)
        {
            var prevColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"AVM panicked at {current} byte-code and exited with {exitCode} code");
            Console.ForegroundColor = prevColor;
        }
        current = module.code.Count;
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
            case OpCode.Return: Return(); break;
            case OpCode.DeallocateStackBytes: DeallocateStackBytes(); break;
            case OpCode.Quit: StopExecution(0); break;
            case OpCode.If: If(); break;
            case OpCode.Jump: Jump(); break;
            default: throw new NotImplementedException($"There is no implementation for {opCode} opcode");
        }
    }

    private void Jump()
    {
        AbsInstructionIndex index = new(NextInt());
        if (index <= 0) throw new Exception($"Invalid jump instruction index = {index.index}");
        current = index;
    }

    private void If()
    {
        StackAddress conditionStackAddress = NextAddress();
        AbsInstructionIndex elseJump = new(NextInt());
        
        HeapAddress conditionHeapAddress = stack.ReadInt(conditionStackAddress);
        int conditionValue = heap.ReadInt(conditionHeapAddress);

        if (conditionValue == 0)
        {
            current = elseJump.index;
        }
    }

    private void Return()
    {
        int prevCurrent = PopInt();
        current = prevCurrent;
    }

    private void BeginScope()
    {
        // Console.WriteLine($"Begin: RBP = {basePointer}, RSP = {stackPointer}");
        PushInt(basePointer);
        basePointer = stackPointer;
    }

    private void DropScope()
    {
        // int freed = stackPointer;
        stackPointer = basePointer;
        basePointer = PopInt();
        // Console.WriteLine($"End: RBP = {basePointer}, RSP = {stackPointer} (freed: {freed})");
    }

    private void Math()
    {
        MathOperator op = (MathOperator)NextByte();
        StackAddress resultAddress = NextAddress();
        StackAddress aAddress = NextAddress();
        StackAddress bAddress = NextAddress();

        HeapAddress resultHeapAddress = stack.ReadInt(resultAddress);
        HeapAddress aHeapAddress = stack.ReadInt(aAddress);
        HeapAddress bHeapAddress = stack.ReadInt(bAddress);

        int aValue = heap.ReadInt(aHeapAddress);
        int bValue = heap.ReadInt(bHeapAddress);

        int resultValue = Math_Int(op, aValue, bValue);
        
        heap.WriteInt(resultHeapAddress, resultValue);
    }

    private int Math_Int(MathOperator op, int a, int b)
    {
        switch (op)
        {
            case MathOperator.Add: return a + b;
            case MathOperator.Sub: return a - b;
            case MathOperator.Mul: return a * b;
            case MathOperator.Div: return a / b;
            case MathOperator.Less: return a < b ? 1 : 0;
            case MathOperator.LessOrEqual: return a <= b ? 1 : 0;
            case MathOperator.Equal: return a == b ? 1 : 0;
            case MathOperator.Greater: return a > b ? 1 : 0;
            case MathOperator.GreaterOrEqual: return a >= b ? 1 : 0;
            default: throw new NotImplementedException();
        }
    }

    private void VariableSetValue()
    {
        int mode = NextInt();
        int sizeInBytes = NextInt();

        ScopeRelativeRbpOffset destRbpOffset = NextRBP();
        StackAddress destStackAddress = ToAbs(destRbpOffset);
        HeapAddress destHeapAddress = stack.ReadInt(destStackAddress);


        if (mode == 0)
        {
            // Value
            StackAddress valueStackAddress = NextAddress();
            HeapAddress valueHeapAddress = stack.ReadInt(valueStackAddress);

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
            StackAddress valueStackAddress = NextAddress();
            HeapAddress valueHeapAddress = stack.ReadInt(valueStackAddress);
            
            heap.WriteInt(destHeapAddress, valueHeapAddress);
        }
        else if (mode == 3)
        {
            // SetValue_Var_To_Ptr
            StackAddress valueVariable = NextAddress();
            int value = stack.ReadInt(valueVariable);

            int pointer = heap.ReadInt(destHeapAddress);

            heap.Copy(value, pointer, (byte)sizeInBytes);
        }
        else if (mode == 4)
        {
            // GetValue_ByPointer
            StackAddress valueVariable = NextAddress();
            HeapAddress pointerHeapAddress = new(stack.ReadInt(valueVariable));
            int pointer = heap.ReadInt(pointerHeapAddress);

            int value = heap.ReadInt(pointer);

            heap.WriteInt(destHeapAddress, value);
        }
        else
        {
            throw new Exception($"Invalid variable set value mode ({mode})");
        }
    }

    private void AllocateVariable()
    {
        int sizeInBytes = NextInt();

        int stackAddress = stackPointer; 
        int heapAddress = Allocate(sizeInBytes);
        
        stack.Write(stackAddress, BitConverter.GetBytes(heapAddress), noLogs: true);
        stack.logger.Log_Allocate(stackAddress, sizeInBytes);
        stackPointer += sizeInBytes;
    }

    public int Allocate(int sizeInBytes)
    {
        int heapAddress = heapPointer;
        
        heap.logger.Log_Allocate(heapAddress, sizeInBytes);
        heapPointer += sizeInBytes;

        return heapAddress;
    }

    private void InternalCall()
    {
        int inModuleIndex = NextInt();

        int pointer = module.functionPointerByID[inModuleIndex];

        PushInt(current);

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
            totalArgumentsSize += 4; // i's argument type size
            StackAddress rbpOffset = new(fakeBasePointer - totalArgumentsSize);

            Type parameterType = methodParams[i].ParameterType;

            unsafe
            {
                fixed (byte* result = &stack.GetArray(rbpOffset)[rbpOffset])
                {
                    arguments[i] = Marshal.PtrToStructure(new IntPtr(result), parameterType);
                }
            }
        }

        object returnValue = methodInfo.Invoke(functions, arguments);

        if (methodInfo.ReturnParameter.ParameterType != typeof(void))
        {
            totalArgumentsSize += 4; // ret type size
            StackAddress retRbpOffset = new(fakeBasePointer - totalArgumentsSize);
            HeapAddress retHeapAddress = new(stack.ReadInt(retRbpOffset));
            
            // https://stackoverflow.com/questions/4865104/convert-any-object-to-a-byte
            var size = 4;
            // Both managed and unmanaged buffers required.
            var bytes = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            // Copy object byte-to-byte to unmanaged memory.
            Marshal.StructureToPtr(returnValue, ptr, false);
            // Copy data from unmanaged memory to managed buffer.
            Marshal.Copy(ptr, bytes, 0, size);
            // Release unmanaged memory.
            Marshal.FreeHGlobal(ptr);
            
            heap.Write(retHeapAddress, bytes);
        }
    }

    private void DeallocateStackBytes()
    {
        int bytesToDeallocate = NextInt();

        stackPointer -= bytesToDeallocate;
    }


    private int NextInt()
    {
        return BitConverter.ToInt32(NextRange(4));
    }

    private byte NextByte()
    {
        byte value = module.code[current];
        current++;
        return value;
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