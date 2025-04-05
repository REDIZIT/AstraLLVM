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