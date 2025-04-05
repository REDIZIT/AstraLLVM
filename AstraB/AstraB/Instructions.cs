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
}

public abstract class Instruction
{
    public abstract void Encode(InstructionEncoder encoder);
}

public class AllocateVariable_Instruction : Instruction
{
    public int sizeInBytes;
    
    public AllocateVariable_Instruction(int sizeInBytes)
    {
        this.sizeInBytes = sizeInBytes;
    }

    public override void Encode(InstructionEncoder encoder)
    {
        encoder.Add(OpCode.Allocate_Variable);
        encoder.Add(sizeInBytes);
    }
}

public class Math_Instruction : Instruction
{
    public int leftRbpOffset, rightRbpOffset;
    public int resultRbpOffset;
    
    public Math_Instruction(int leftRbpOffset, int rightRbpOffset, int resultRbpOffset)
    {
        this.leftRbpOffset = leftRbpOffset;
        this.rightRbpOffset = rightRbpOffset;
        this.resultRbpOffset = resultRbpOffset;
    }

    public override void Encode(InstructionEncoder encoder)
    {
        encoder.Add(OpCode.Math);
        encoder.Add((int)0);
        encoder.Add(resultRbpOffset);
        encoder.Add(leftRbpOffset);
        encoder.Add(rightRbpOffset);
    }
}

public class SetValue_Instruction : Instruction
{
    public int mode;
    public int targetRbpOffset, valueRbpOffset, sizeInBytes;
    public byte[] constant;

    private SetValue_Instruction()
    {
    }

    public static SetValue_Instruction Variable_to_Variable(int targetRbpOffset, int valueRbpOffset, int sizeInBytes)
    {
        return new SetValue_Instruction()
        {
            mode = 0,
            targetRbpOffset = targetRbpOffset,
            valueRbpOffset = valueRbpOffset,
            sizeInBytes = sizeInBytes
        };
    }

    public static SetValue_Instruction Const_to_Variable(int targetRbpOffset, byte[] constant)
    {
        return new SetValue_Instruction()
        {
            mode = 1,
            targetRbpOffset = targetRbpOffset,
            sizeInBytes = constant.Length,
            constant = constant
        };
    }
    
    public static SetValue_Instruction Pointer_to_Variable(int targetRbpOffset, int valueRbpOffset, int sizeInBytes)
    {
        return new SetValue_Instruction()
        {
            mode = 2,
            targetRbpOffset = targetRbpOffset,
            valueRbpOffset = valueRbpOffset,
            sizeInBytes = sizeInBytes
        };
    }

    public override void Encode(InstructionEncoder encoder)
    {
        encoder.Add(OpCode.Variable_SetValue);
        encoder.Add(mode);
        encoder.Add(sizeInBytes);
        encoder.Add(targetRbpOffset);
        
        if (mode == 0)
        {
            // Variable to variable
            encoder.Add(valueRbpOffset);
        }
        else if (mode == 1)
        {
            // Variable to variable
            encoder.AddRange(constant, sizeInBytes);
        }
        else if (mode == 2)
        {
            // Variable's pointer to variable
            encoder.Add(valueRbpOffset);
        }
        else
        {
            throw new NotSupportedException();
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
        encoder.Add(OpCode.BeginScope);
        encoder.Add(isExternal ? OpCode.ExternalCall : OpCode.InternalCall);
        encoder.Add(functionInModuleIndex);
        encoder.Add(OpCode.DropScope);
    }
}