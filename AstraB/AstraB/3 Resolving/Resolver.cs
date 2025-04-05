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
                genericTypeAliases = typeNode.genericTypeAliases,
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
        // Pass 4: Found and Register Generics
        //
        foreach (Node child in root.EnumerateEachChild())
        {
            if (child is Node_VariableDeclaration declaration)
            {
                TypeInfo type = module.GetType(declaration.typeName);

                if (type.IsGeneric)
                {
                    List<TypeInfo> concreteTypes = new();
                    for (int i = 0; i < type.genericTypeAliases.Count; i++)
                    {
                        Token_Identifier alias = type.genericTypeAliases[i];
                        Token_Identifier concrete = declaration.concreteGenericTypes[i];

                        TypeInfo concreteType = module.GetType(concrete.name);
                        concreteTypes.Add(concreteType);
                    }

                    GenericImplementationInfo genericType;
                    if (module.TryGetGeneric(type, concreteTypes, out genericType) == false)
                    {
                        genericType = new GenericImplementationInfo()
                        {
                            baseType = type,
                            genericTypes = concreteTypes
                        };
                        module.Register(genericType);
                    }
                }
            }
        }
        
        //
        // Pass 5: Register functions
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
        module.Register(new TypeInfo("ptr") { isPrimitive = true, sizeInBytes = 4 });
        
        module.Register(new FunctionInfo("print")
        {
            parameters = new()
            {
                new FieldInfo(module.GetType("int"), "number")
            }
        });
        
        
        module.Register(new FunctionInfo("set_int")
        {
            parameters = new()
            {
                new FieldInfo(module.GetType("ptr"), "pointer"),
                new FieldInfo(module.GetType("int"), "value"),
            }
        });
        
        module.Register(new FunctionInfo("get_int")
        {
            parameters = new()
            {
                new FieldInfo(module.GetType("ptr"), "pointer"),
            },
            returns = new()
            {
                new FieldInfo(module.GetType("int"), "value"),
            }
        });
        
        module.Register(new FunctionInfo("print_ptr")
        {
            parameters = new()
            {
                new FieldInfo(module.GetType("ptr"), "number")
            }
        });

        return module;
    }
}