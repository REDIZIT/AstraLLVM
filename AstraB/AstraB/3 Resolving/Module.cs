public class Module
{
    public List<TypeInfo> types = new();
    public List<FunctionInfo> functions = new();

    public Dictionary<string, TypeInfo> typeByName = new();
    public Dictionary<TypeInfo, List<GenericImplementationInfo>> genericTypesByBase = new();
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

    public void Register(GenericImplementationInfo type)
    {
        if (genericTypesByBase.ContainsKey(type.baseType) == false)
        {
            genericTypesByBase.Add(type.baseType, new());
        }
        
        genericTypesByBase[type.baseType].Add(type);
    }

    public ITypeInfo GetBaseOrGenericType(string name)
    {
        if (TryGetType(name, out TypeInfo typeInfo))  return typeInfo;
        if (TryGetGeneric(name, out GenericImplementationInfo genericInfo)) return genericInfo;
        throw new Exception($"No base or generic type '{name}' not found in module or module's usings");
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

    public GenericImplementationInfo GetGeneric(TypeInfo baseType, IEnumerable<TypeInfo> concreteTypes)
    {
        if (TryGetGeneric(baseType, concreteTypes, out GenericImplementationInfo info)) return info;
        throw new Exception($"Generic implementation with base type '{baseType.name}' and concrete types {string.Join(", ", concreteTypes.Select(t => "'" + t.name + "'"))} not found");
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

    public bool TryGetGeneric(string name, out GenericImplementationInfo info)
    {
        ParseGenericType(name, out TypeInfo baseType, out List<TypeInfo> concreteTypes);
        return TryGetGeneric(baseType, concreteTypes, out info);
    }
    public bool TryGetGeneric(TypeInfo baseType, IEnumerable<TypeInfo> concreteTypes, out GenericImplementationInfo info)
    {
        info = null;
        
        if (genericTypesByBase.TryGetValue(baseType, out var list) == false)
            return false;

        foreach (GenericImplementationInfo genericInfo in list)
        {
            bool isFound = true;
            
            for (int i = 0; i < genericInfo.genericTypes.Count; i++)
            {
                TypeInfo genericConcreteType = genericInfo.genericTypes[i];
                TypeInfo askedConcreteType = concreteTypes.ElementAt(i);

                if (genericConcreteType != askedConcreteType)
                {
                    isFound = false;
                    break;
                }
            }

            if (isFound)
            {
                info = genericInfo;
                return true;
            }
        }

        return false;
    }

    public void ParseGenericType(string name, out TypeInfo baseType, out List<TypeInfo> concreteTypes)
    {
        baseType = GetType(name.Split("<")[0]);

        int beginIndex = name.IndexOf("<") + 1;
        int endIndex = name.IndexOf(">");
        
        string[] split = name.Substring(beginIndex , endIndex - beginIndex).Replace(" ", "").Split(",");
        concreteTypes = new();
        foreach (string splitName in split)
        {
            concreteTypes.Add(GetType(splitName));
        }
    }
}




public class FunctionInfo
{
    public string name;
    public Node_FunctionDeclaration node;

    public List<FieldInfo> parameters;
    public List<FieldInfo> returns;
    
    public ITypeInfo owner;
    
    public Module module;
    public int inModuleIndex;
    
    public FunctionInfo()
    {
    }
}

public class FieldInfo
{
    public ITypeInfo type;
    public string name;
    public int offsetInBytes;

    public FieldInfo()
    {
    }

    public FieldInfo(ITypeInfo type, string name)
    {
        this.type = type;
        this.name = name;
    }
}

public class RawFieldInfo(string typeName, string fieldName)
{
    public string typeName = typeName, fieldName = fieldName;
}