public struct InScopeRbpOffset(int value)
{
    public int value = value;
    
    public static implicit operator int(InScopeRbpOffset s) => s.value;
    public static implicit operator InScopeRbpOffset(int v) => new(v);
    
    public override string ToString()
    {
        return value.ToString();
    }
}

public struct ScopeRelativeRbpOffset(int value)
{
    public int value = value;
    
    public static implicit operator int(ScopeRelativeRbpOffset s) => s.value;
    public static implicit operator ScopeRelativeRbpOffset(int v) => new(v);

    public override string ToString()
    {
        return value.ToString();
    }
}

public struct StackAddress(int value)
{
    public int value = value;
    
    public static implicit operator int(StackAddress s) => s.value;
    public static implicit operator StackAddress(int v) => new(v);

    public override string ToString()
    {
        return value.ToString();
    }
}

public struct HeapAddress(int value)
{
    public int value = value;
    
    public static implicit operator int(HeapAddress s) => s.value;
    public static implicit operator HeapAddress(int v) => new(v);

    public override string ToString()
    {
        return value.ToString();
    }
}

public struct Ptr(int value)
{
    public int value = value;
    public static implicit operator int(Ptr s) => s.value;
}

public struct AbsInstructionIndex(int index)
{
    public readonly int index = index;
    public static implicit operator int(AbsInstructionIndex s) => s.index;
    public static AbsInstructionIndex Invalid => new AbsInstructionIndex(-1);
}

public struct AbsByteCodeIndex(int index)
{
    public readonly int index = index;
    public static implicit operator int(AbsByteCodeIndex s) => s.index;
    public static AbsByteCodeIndex Invalid => new AbsByteCodeIndex(-1);
}