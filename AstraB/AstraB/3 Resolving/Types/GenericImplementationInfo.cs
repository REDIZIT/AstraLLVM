public class GenericImplementationInfo : ITypeInfo
{
    public string Name => baseType.Name;
    public int SizeInBytes => baseType.SizeInBytes;
    public bool IsPrimitive => baseType.IsPrimitive;
    public bool IsGeneric => baseType.IsGeneric;
    public List<FieldInfo> Fields => baseType.Fields;
    public List<FunctionInfo> Functions => baseType.Functions;

    public TypeInfo baseType;
    public List<TypeInfo> genericTypes;

    public override string ToString()
    {
        return baseType.name + "<" + string.Join(", ", genericTypes.Select(t => t.name)) + ">";
    }
}