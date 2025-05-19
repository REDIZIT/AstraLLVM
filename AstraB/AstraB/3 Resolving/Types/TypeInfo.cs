public class TypeInfo : ITypeInfo
{
    public string Name => name;
    public int SizeInBytes => sizeInBytes;
    public bool IsGeneric => genericTypeAliases != null && genericTypeAliases.Count > 0;
    public bool IsPrimitive => isPrimitive;
    public List<FieldInfo> Fields => fields;
    public List<FunctionInfo> Functions => functions;

    public string name;
    public List<FieldInfo> fields;
    public List<FunctionInfo> functions;
    public Node_TypeDeclaration node;

    public bool isPrimitive;
    public int sizeInBytes;
    
    public List<Token_Identifier> genericTypeAliases;
    
    public Module module;
    public int inModuleIndex;

    public TypeInfo()
    {
    }

    public TypeInfo(string name)
    {
        this.name = name;
    }
}