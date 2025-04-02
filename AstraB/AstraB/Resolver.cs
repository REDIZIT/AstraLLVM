public static class Resolver
{
    public static Module Resolve(Node_Root root)
    {
        Module module = new();
        
        Module vmModule = CreateVMDependModule();
        module.usings.Add(vmModule);

        int pointerSizeInBytes = 4;

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
        // Pass 3: Calculate sizeInBytes
        //
        foreach (TypeInfo type in module.types)
        {
            type.sizeInBytes = type.fields.Sum(f => f.type.isPrimitive ? f.type.sizeInBytes : pointerSizeInBytes);
        }
        
        //
        // Pass 4: Register functions
        //
        foreach (Node_FunctionDeclaration functionNode in root.children.Where(n => n is Node_FunctionDeclaration))
        {
            FunctionInfo functionInfo = new FunctionInfo()
            {
                name = functionNode.name,
                node = functionNode,
                parameters = new()
            };
            module.Register(functionInfo);
        }

        return module;
    }

    private static Module CreateVMDependModule()
    {
        Module module = new();

        module.Register(new TypeInfo("byte") { isPrimitive = true, sizeInBytes = 1 });
        module.Register(new TypeInfo("short") { isPrimitive = true, sizeInBytes = 2 });
        module.Register(new TypeInfo("int") { isPrimitive = true, sizeInBytes = 4 });
        module.Register(new TypeInfo("long") { isPrimitive = true, sizeInBytes = 8 });
        
        module.Register(new FunctionInfo("print")
        {
            parameters = new ()
            {
                new FieldInfo(module.GetType("int"), "number")
            }
        });

        return module;
    }
}