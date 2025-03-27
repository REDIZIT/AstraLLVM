public static class Resolver
{
    public static Module Resolve(Node_Root root)
    {
        Module module = new();
        
        Module vmModule = CreateVMDependModule();
        module.usings.Add(vmModule);

        //
        // Pass 1: Register types
        //
        foreach (Node_TypeDeclaration typeNode in root.children.Where(n => n is Node_TypeDeclaration))
        {
            TypeInfo typeInfo = new TypeInfo()
            {
                name = typeNode.name,
                fields = new(),
                node = typeNode,
            };
            module.Register(typeInfo);
        }
        
        //
        // Pass 2: Resolve type's fields
        //
        foreach (TypeInfo type in module.types)
        {
            foreach (Node_FieldDeclaration fieldNode in type.node.block.children.Where(n => n is Node_FieldDeclaration))
            {
                TypeInfo fieldType = module.GetType(fieldNode.typeName);
                type.fields.Add(new FieldInfo()
                {
                    type = fieldType,
                    name = fieldNode.fieldName
                });
            }
        }
        
        //
        // Pass 3: Register functions
        //
        foreach (Node_FunctionDeclaration functionNode in root.children.Where(n => n is Node_FunctionDeclaration))
        {
            FunctionInfo functionInfo = new FunctionInfo()
            {
                name = functionNode.name,
                node = functionNode,
            };
            module.Register(functionInfo);
        }

        return module;
    }

    private static Module CreateVMDependModule()
    {
        Module module = new();

        module.Register(new TypeInfo("int"));
        module.Register(new TypeInfo("long"));
        
        module.Register(new FunctionInfo("print"));

        return module;
    }
}