public static class Resolver
{
    public static Module Resolve(Node_Root root, VM vm)
    {
        Module module = new();
        
        Module vmModule = vm.CreateVMDependModule();
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
                parameters = new(),
                returns = new(),
            };
            module.Register(functionInfo);

            functionNode.functionInfo = functionInfo;

            foreach (string typeName in functionNode.returns)
            {
                functionInfo.returns.Add(new(module.GetType(typeName), String.Empty));
            }
        }

        return module;
    }

    
}