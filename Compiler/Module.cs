public class Module
{
    public Dictionary<string, FunctionInfo> functionInfoByName = new();
    public Dictionary<string, TypeInfo> typeInfoByName = new();
    public Dictionary<string, ClassInfo> classInfoByName = new();
}
public class Scope
{

}
public class ClassInfo
{
    public string name;
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
public class TypeInfo
{
    public string astraName, asmName;
}
public class VariableRawData
{
    public string name;
    public string type;
}