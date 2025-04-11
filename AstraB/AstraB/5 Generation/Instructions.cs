public class InstructionEncoder
{
    public List<byte> code = new();
    
    public void Add(OpCode code)
    {
        Add((byte)code);
    }
    public void Add(byte value)
    {
        code.Add(value);
    }
    public void AddRange(byte[] value, int padSize = 0)
    {
        code.AddRange(value);

        for (int i = 0; i < padSize - value.Length; i++)
        {
            code.Add((byte)0);
        }
    }
    public void Add(short value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        AddRange(bytes);
    }
    public void Add(int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        AddRange(bytes);
    }
    public void Add(long value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        AddRange(bytes);
    }

    public void Set(int index, byte[] value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            code[index + i] = value[i];
        }
    }
}

public abstract class Instruction
{
    public Inverval bytecodeRange;
    
    public abstract void Encode(InstructionEncoder encoder);
}

public class AllocateVariable_Instruction : Instruction
{
    public int sizeInBytes;

    public StaticVariable staticVariable;
    
    public AllocateVariable_Instruction(int sizeInBytes)
    {
        this.sizeInBytes = sizeInBytes;
    }

    public override void Encode(InstructionEncoder encoder)
    {
        encoder.Add(OpCode.Allocate_Variable);
        encoder.Add(sizeInBytes);
    }

    public AllocateVariable_Instruction WithDebug(StaticVariable staticVariable)
    {
        this.staticVariable = staticVariable;
        return this;
    }
}

public class DeallocateStackBytes_Instruction(int bytesToDeallocate) : Instruction
{
    public int bytesToDeallocate = bytesToDeallocate;

    public override void Encode(InstructionEncoder encoder)
    {
        encoder.Add(OpCode.DeallocateStackBytes);
        encoder.Add((int)bytesToDeallocate);
    }
}

public class Math_Instruction : Instruction
{
    public ScopeRelativeRbpOffset leftRbpOffset, rightRbpOffset;
    public ScopeRelativeRbpOffset resultRbpOffset;
    public MathOperator op;
    
    public Math_Instruction(ScopeRelativeRbpOffset leftRbpOffset, ScopeRelativeRbpOffset rightRbpOffset, ScopeRelativeRbpOffset resultRbpOffset, MathOperator op)
    {
        this.leftRbpOffset = leftRbpOffset;
        this.rightRbpOffset = rightRbpOffset;
        this.resultRbpOffset = resultRbpOffset;
        this.op = op;
    }

    public override void Encode(InstructionEncoder encoder)
    {
        encoder.Add(OpCode.Math);
        encoder.Add((byte)op);
        encoder.Add(resultRbpOffset);
        encoder.Add(leftRbpOffset);
        encoder.Add(rightRbpOffset);
    }
}

public class SetValue_Instruction : Instruction
{
    public int mode;
    public ScopeRelativeRbpOffset targetRbpOffset, valueRbpOffset;
    public int sizeInBytes;
    public byte[] constant;

    private SetValue_Instruction()
    {
    }

    /// <summary>
    /// Copy value from value variable to target variable
    /// </summary>
    public static SetValue_Instruction Variable_to_Variable(ScopeRelativeRbpOffset targetRbpOffset, ScopeRelativeRbpOffset valueRbpOffset, int sizeInBytes)
    {
        return new SetValue_Instruction()
        {
            mode = 0,
            targetRbpOffset = targetRbpOffset,
            valueRbpOffset = valueRbpOffset,
            sizeInBytes = sizeInBytes
        };
    }

    /// <summary>
    /// Copy value from constant value to target variable
    /// </summary>
    public static SetValue_Instruction Const_to_Variable(ScopeRelativeRbpOffset targetRbpOffset, byte[] constant)
    {
        return new SetValue_Instruction()
        {
            mode = 1,
            targetRbpOffset = targetRbpOffset,
            sizeInBytes = constant.Length,
            constant = constant
        };
    }
    
    /// <summary>
    /// Get pointer (ptr type) of value variable and put it into target variable
    /// </summary>
    public static SetValue_Instruction GetPointer_To_Variable(ScopeRelativeRbpOffset targetRbpOffset, ScopeRelativeRbpOffset valueRbpOffset, int sizeInBytes)
    {
        return new SetValue_Instruction()
        {
            mode = 2,
            targetRbpOffset = targetRbpOffset,
            valueRbpOffset = valueRbpOffset,
            sizeInBytes = sizeInBytes
        };
    }
    
    /// <summary>
    /// Set value of target variable (ptr type) from value variable
    /// </summary>
    public static SetValue_Instruction SetValue_ByPointer(ScopeRelativeRbpOffset targetRbpOffset, ScopeRelativeRbpOffset valueRbpOffset, int sizeInBytes)
    {
        return new SetValue_Instruction()
        {
            mode = 3,
            targetRbpOffset = targetRbpOffset,
            valueRbpOffset = valueRbpOffset,
            sizeInBytes = sizeInBytes
        };
    }

    /// <summary>
    /// Get value of value variable variable (ptr type) and put it into target variable 
    /// </summary>
    public static SetValue_Instruction GetValue_ByPointer(ScopeRelativeRbpOffset resultRbpOffset, ScopeRelativeRbpOffset pointerRbpOffset, int sizeInBytes)
    {
        return new SetValue_Instruction()
        {
            mode = 4,
            targetRbpOffset = resultRbpOffset,
            valueRbpOffset = pointerRbpOffset,
            sizeInBytes = sizeInBytes
        };
    }
    
    public override void Encode(InstructionEncoder encoder)
    {
        encoder.Add(OpCode.Variable_SetValue);
        encoder.Add(mode);
        encoder.Add(sizeInBytes);
        encoder.Add(targetRbpOffset);
        
        if (mode == 1)
        {
            encoder.AddRange(constant, sizeInBytes);
        }
        else
        {
            encoder.Add(valueRbpOffset);
        }
    }
}

public class FunctionCall_Instruction : Instruction
{
    public int functionInModuleIndex;
    public bool isExternal;
    
    public FunctionCall_Instruction(int functionInModuleIndex, bool isExternal)
    {
        this.functionInModuleIndex = functionInModuleIndex;
        this.isExternal = isExternal;
    }

    public override void Encode(InstructionEncoder encoder)
    {
        encoder.Add(isExternal ? OpCode.ExternalCall : OpCode.InternalCall);
        encoder.Add(functionInModuleIndex);
    }
}

public class FunctionDeclaration_Instruction(FunctionInfo functionInfo) : Empty_Instruction
{
    public FunctionInfo functionInfo = functionInfo;
}

public class Scope_Instruction : Instruction
{
    public bool isBeginning;

    public Scope_Instruction(bool isBeginning)
    {
        this.isBeginning = isBeginning;
    }

    public override void Encode(InstructionEncoder encoder)
    {
        encoder.Add(isBeginning ? OpCode.BeginScope : OpCode.DropScope);
    }
}

public class Empty_Instruction : Instruction
{
    public Empty_Instruction()
    {
    }

    public override void Encode(InstructionEncoder encoder)
    {
    }
}

public class Debug_Instruction : Empty_Instruction
{
    public string message;
    
    public Debug_Instruction(string message)
    {
        this.message = message;
    }

    public override string ToString()
    {
        return message;
    }
}

public class Return_Instruction : Instruction
{
    public override void Encode(InstructionEncoder encoder)
    {
        encoder.Add(OpCode.DropScope);
        encoder.Add(OpCode.Return);
    }
}

public class Quit_Instruction : Instruction
{
    public override void Encode(InstructionEncoder encoder)
    {
        encoder.Add(OpCode.Quit);
    }
}

public class If_Instruction(ScopeRelativeRbpOffset condition, AbsInstructionIndex elseJump) : Instruction
{
    public ScopeRelativeRbpOffset condition = condition;
    public AbsInstructionIndex elseJump = elseJump;

    public override void Encode(InstructionEncoder encoder)
    {
        encoder.Add(OpCode.If);
        encoder.Add(condition);
        encoder.Add(elseJump);
    }

    public void Recode(InstructionEncoder encoder, AbsByteCodeIndex thisBeginIndex, AbsByteCodeIndex byteCodeIndex)
    {
        int toSkip = sizeof(OpCode) + sizeof(int);
        byte[] bytes = BitConverter.GetBytes((int)byteCodeIndex);
        encoder.Set(thisBeginIndex + toSkip, bytes);
    }
}

public class Jump_Instruction(AbsInstructionIndex index) : Instruction
{
    public AbsInstructionIndex index = index;

    public override void Encode(InstructionEncoder encoder)
    {
        encoder.Add(OpCode.Jump);
        encoder.Add(index);
    }

    public void Recode(InstructionEncoder encoder, AbsByteCodeIndex thisBeginIndex, AbsByteCodeIndex byteCodeIndex)
    {
        int toSkip = sizeof(OpCode);
        byte[] bytes = BitConverter.GetBytes((int)byteCodeIndex);
        encoder.Set(thisBeginIndex + toSkip, bytes);
    }
}