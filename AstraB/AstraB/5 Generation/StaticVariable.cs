public class StaticVariable
{
    public bool IsField => sliceParent != null;
    
    public string name;
    public int rbpOffset, sizeInBytes;
    public ITypeInfo type;
    public Scope_GenerationPhase scope;
    
    public StaticVariable sliceParent;
    public int sliceOffsetInBytes;

    // public StaticVariable CreateSliceVariable(int offsetInBytes, ITypeInfo slicedType)
    // {
    //     Interval parentRange = Interval.FromBeginAndLength(rbpOffset, this.sizeInBytes);
    //     Interval sliceRange = Interval.FromBeginAndLength(rbpOffset + offsetInBytes, slicedType.RefSizeInBytes);
    //
    //     if (parentRange.IsInside(sliceRange) == false)
    //         throw new Exception($"Failed to create variable slice due to slice is out of parent's bounds");
    //
    //     return new StaticVariable()
    //     {
    //         name = "Slice of " + name,
    //         rbpOffset = sliceRange.begin,
    //         sizeInBytes = sliceRange.Length,
    //         type = slicedType,
    //         sliceOffsetInBytes = offsetInBytes,
    //         sliceParent = this,
    //     };
    // }

    public StaticVariable CreateFieldVariable(FieldInfo field)
    {
        return new StaticVariable()
        {
            name = this.name + "." + field.name,
            rbpOffset = this.rbpOffset,
            sizeInBytes = field.type.RefSizeInBytes,
            type = field.type,
            sliceOffsetInBytes = field.offsetInBytes,
            sliceParent = this
        };
    }
}