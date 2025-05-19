public interface ITypeInfo
{
    string Name { get; }
    int SizeInBytes { get; }
    bool IsPrimitive { get; }
    bool IsGeneric { get; }
    int RefSizeInBytes => IsPrimitive ? SizeInBytes : Constants.POINTER_SIZE_IN_BYTES;
    List<FieldInfo> Fields { get; }
    List<FunctionInfo> Functions { get; }
}