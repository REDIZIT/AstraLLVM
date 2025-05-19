using System.Reflection;

public partial class VM
{
    public Module CreateVMDependModule()
    {
        Module module = new();

        module.Register(new TypeInfo("byte") { isPrimitive = true, sizeInBytes = 1 });
        module.Register(new TypeInfo("short") { isPrimitive = true, sizeInBytes = 2 });
        module.Register(new TypeInfo("int") { isPrimitive = true, sizeInBytes = 4 });
        module.Register(new TypeInfo("long") { isPrimitive = true, sizeInBytes = 8 });
        module.Register(new TypeInfo("ptr") { isPrimitive = true, sizeInBytes = 4 });
        
        foreach (MethodInfo methodInfo in functions.methods)
        {
            FunctionInfo info = new FunctionInfo()
            {
                name = methodInfo.Name,
                parameters = new(),
                returns = new()
            };
            
            foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
            {
                string astraTypeName = csharpToAstraTypeNames[parameterInfo.ParameterType.Name];
                TypeInfo type = module.GetType(astraTypeName);
                info.parameters.Add(new FieldInfo(type, parameterInfo.Name));
            }

            if (methodInfo.ReturnParameter.ParameterType != typeof(void))
            {
                string astraTypeName = csharpToAstraTypeNames[methodInfo.ReturnParameter.ParameterType.Name];
                TypeInfo type = module.GetType(astraTypeName);
                info.returns.Add(new FieldInfo(type, methodInfo.ReturnParameter.Name));
            }
            
            module.Register(info);
        }

        return module;
    }

    private static Dictionary<string, string> csharpToAstraTypeNames = new()
    {
        { nameof(Int32), "int" },
        { nameof(HeapAddress), "int" },
        { nameof(Ptr), "ptr" },
    };
}