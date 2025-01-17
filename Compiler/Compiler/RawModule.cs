public class RawModule
{
    public Dictionary<string, RawTypeInfo> typeInfoByName = new();
    public Dictionary<string, RawClassTypeInfo> classInfoByName = new();
    public Dictionary<string, RawFunctionInfo> functionInfoByName = new();

    public void RegisterClass(RawClassTypeInfo classInfo)
    {
        typeInfoByName.Add(classInfo.name, classInfo);
        classInfoByName.Add(classInfo.name, classInfo);
    }
    public void RegisterFunction(RawFunctionInfo functionInfo)
    {
        functionInfoByName.Add(functionInfo.name, functionInfo);
    }
}
public class RawFunctionInfo
{
    public string name;

    public List<RawTypeInfo> arguments = new();
    public List<RawTypeInfo> returns = new();
}
public class RawClassTypeInfo : RawTypeInfo
{
    public List<RawFieldInfo> fields = new();
}
public class RawFieldInfo
{
    public string name;
    public string typeName;
}
public class RawTypeInfo
{
    public string name;
}
public class RawPrimitiveTypeInfo : RawTypeInfo
{
    public string asmName;
}