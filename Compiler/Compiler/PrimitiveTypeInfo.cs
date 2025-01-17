public class PrimitiveTypeInfo : TypeInfo
{
    public string asmName;

    public static PrimitiveTypeInfo BOOL;
    public static PrimitiveTypeInfo BYTE;
    public static PrimitiveTypeInfo SHORT;
    public static PrimitiveTypeInfo INT;
    public static PrimitiveTypeInfo LONG;

    public static PrimitiveTypeInfo PTR;
    public static PrimitiveTypeInfo ARRAY;

    public override string ToString()
    {
        return asmName;
    }
}
