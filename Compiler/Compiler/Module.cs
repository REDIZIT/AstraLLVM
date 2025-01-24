public class ResolvedModule
{
    public Dictionary<string, FunctionInfo> functionInfoByName = new();
    public Dictionary<string, TypeInfo> typeInfoByName = new();
    public Dictionary<string, ClassTypeInfo> classInfoByName = new();

    public void RegisterClass(ClassTypeInfo classInfo)
    {
        //RegisterType(classInfo);
        classInfoByName.Add(classInfo.name, classInfo);
    }
    public void RegisterFunction(FunctionInfo functionInfo)
    {
        functionInfoByName.Add(functionInfo.name, functionInfo);
    }
    public void RegisterType(TypeInfo typeInfo)
    {
        typeInfoByName.Add(typeInfo.name, typeInfo);
    }

    public TypeInfo GetType(string name)
    {
        return typeInfoByName[name];
    }
    public TypeInfo GetType(RawTypeInfo rawTypeInfo)
    {
        return GetType(rawTypeInfo.name);
    }
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
public class ClassTypeInfo : TypeInfo
{
    public List<FieldInfo> fields = new();

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

    public void Resolve(ResolvedModule module)
    {
        type = module.GetType(rawType);
    }
}