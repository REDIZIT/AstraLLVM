public struct InScopeRbpOffset(int value)
{
    public int value = value;
    
    public static implicit operator int(InScopeRbpOffset s) => s.value;
    
    public override string ToString()
    {
        return value.ToString();
    }
}

public struct ScopeRelativeRbpOffset(int value)
{
    public int value = value;
    
    public static implicit operator int(ScopeRelativeRbpOffset s) => s.value;

    public override string ToString()
    {
        return value.ToString();
    }
}

public struct MemoryAddress(int value)
{
    public int value = value;
    
    public static implicit operator int(MemoryAddress s) => s.value;

    public override string ToString()
    {
        return value.ToString();
    }
}

public struct StackAddress(int value)
{
    public int value = value;
    
    public static implicit operator int(StackAddress s) => s.value;

    public override string ToString()
    {
        return value.ToString();
    }
}

public struct HeapAddress(int value)
{
    public int value = value;
    
    public static implicit operator int(HeapAddress s) => s.value;

    public override string ToString()
    {
        return value.ToString();
    }
}