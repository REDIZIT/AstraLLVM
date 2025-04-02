public class Module
{
    public List<TypeInfo> types = new();
    public List<FunctionInfo> functions = new();

    public Dictionary<string, TypeInfo> typeByName = new();
    public Dictionary<string, FunctionInfo> functionByName = new();

    public List<Module> usings = new();

    public void Register(TypeInfo type)
    {
        typeByName.Add(type.name, type);

        type.module = this;
        type.inModuleIndex = types.Count;
        
        types.Add(type);
    }
    public void Register(FunctionInfo function)
    {
        functionByName.Add(function.name, function);

        function.module = this;
        function.inModuleIndex = functions.Count;
        
        functions.Add(function);
    }

    public TypeInfo GetType(string name)
    {
        if (TryGetType(name, out TypeInfo info)) return info;
        throw new Exception($"Type '{name}' not found in module or module's usings");
    }
    public FunctionInfo GetFunction(string name)
    {
        if (TryGetFunction(name, out FunctionInfo info)) return info;
        throw new Exception($"Function '{name}' not found in module or module's usings");
    }

    public bool TryGetType(string name, out TypeInfo info)
    {
        if (typeByName.TryGetValue(name, out info)) return true;

        foreach (Module another in usings)
        {
            if (another.TryGetType(name, out info)) return true;
        }

        info = null;
        return false;
    }
    public bool TryGetFunction(string name, out FunctionInfo info)
    {
        if (functionByName.TryGetValue(name, out info)) return true;

        foreach (Module another in usings)
        {
            if (another.TryGetFunction(name, out info)) return true;
        }

        info = null;
        return false;
    }
}

public class TypeInfo
{
    public string name;
    public List<FieldInfo> fields;
    public Node_TypeDeclaration node;

    public bool isPrimitive;
    public int sizeInBytes;
    
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

public class FunctionInfo
{
    public string name;
    public Node_FunctionDeclaration node;

    public List<FieldInfo> parameters;
    
    public Module module;
    public int inModuleIndex;
    
    public FunctionInfo()
    {
    }

    public FunctionInfo(string name)
    {
        this.name = name;
    }
}

public class FieldInfo
{
    public TypeInfo type;
    public string name;

    public FieldInfo()
    {
    }

    public FieldInfo(TypeInfo type, string name)
    {
        this.type = type;
        this.name = name;
    }
}