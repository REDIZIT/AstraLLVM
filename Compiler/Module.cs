public class Module
{
    public Dictionary<string, FunctionInfo> functionInfoByName = new();
    public Dictionary<string, TypeInfo> typeInfoByName = new();
    public Dictionary<string, ClassTypeInfo> classInfoByName = new();

    public TypeInfo GetType(string name)
    {
        return typeInfoByName[name];
    }
}
public class Scope
{

}

public class FunctionInfo
{
    public string name;

    public List<TypeInfo> arguments = new();
    public List<TypeInfo> returns = new();
}
public class FieldInfo
{
    public string name;
    public TypeInfo type;

}

public class PrimitiveTypeInfo : TypeInfo
{
    public string asmName;

    public static PrimitiveTypeInfo BOOL;
    public static PrimitiveTypeInfo BYTE;
    public static PrimitiveTypeInfo SHORT;
    public static PrimitiveTypeInfo INT;
    public static PrimitiveTypeInfo LONG;

    public static PrimitiveTypeInfo PTR;

    public override string ToString()
    {
        return asmName;
    }
}
public class ClassTypeInfo : TypeInfo
{
    public override string ToString()
    {
        return "%" + name;
    }
}
public class TypeInfo
{
    public string name;

    public override string ToString()
    {
        return name;
    }
}


public class VariableRawData
{
    public string name;
    public string rawType;
    public TypeInfo type;

    public void Resolve(Module module)
    {
        type = module.GetType(rawType);
    }
}