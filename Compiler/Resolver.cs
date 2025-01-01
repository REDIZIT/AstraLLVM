public static class Resolver
{
    public static ResolvedModule DiscoverModule(List<Node> ast)
    {
        Access.Set(Stage.DisoverModule_Begin);



        RawModule raw = new();

        AppendRawLLVMTypes(raw);
        Access.Set(Stage.DisoverModule_Done_LLVMTypes);


        foreach (Node node in ast)
        {
            node.RegisterRefs(raw);
        }
        Access.Set(Stage.DisoverModule_Done_RegisterRefs);





        ResolvedModule resolved = ResolveRawModule(raw);

        foreach (Node node in ast)
        {
            node.ResolveRefs(resolved);
        }
        Access.Set(Stage.DisoverModule_Done_ResolveRefs);




        return resolved;
    }


    private static ResolvedModule ResolveRawModule(RawModule raw)
    {
        ResolvedModule resolved = new();
        AppendResolvedLLVMTypes(resolved);

        //
        // Resolve Types (include custom Classes/Structs)
        //
        foreach (RawTypeInfo rawInfo in raw.typeInfoByName.Values)
        {
            // Primities already resolved in AppendResolvedLLVMTypes
            if (rawInfo is RawPrimitiveTypeInfo) continue;


            TypeInfo typeInfo;
            if (rawInfo is RawClassTypeInfo)
            {
                typeInfo = new ClassTypeInfo()
                {
                    name = rawInfo.name
                };
            }
            else
            {
                typeInfo = new()
                {
                    name = rawInfo.name
                };
            }

            resolved.RegisterType(typeInfo);
        }


        //
        // Resolve Functions
        //
        foreach (RawFunctionInfo rawInfo in raw.functionInfoByName.Values)
        {
            FunctionInfo functionInfo = new()
            {
                name = rawInfo.name
            };

            foreach (RawTypeInfo rawTypeInfo in rawInfo.arguments)
            {
                functionInfo.arguments.Add(resolved.GetType(rawTypeInfo));
            }
            foreach (RawTypeInfo rawTypeInfo in rawInfo.returns)
            {
                functionInfo.returns.Add(resolved.GetType(rawTypeInfo));
            }
            resolved.RegisterFunction(functionInfo);
        }

        //
        // Resolve Classes
        //
        foreach (RawClassTypeInfo rawInfo in raw.classInfoByName.Values)
        {
            ClassTypeInfo classInfo = (ClassTypeInfo)resolved.GetType(rawInfo.name);

            foreach (RawFieldInfo rawField in rawInfo.fields)
            {
                FieldInfo fieldInfo = new FieldInfo()
                {
                    name = rawField.name,
                    type = resolved.GetType(rawField.typeName)
                };

                classInfo.fields.Add(fieldInfo);
            }
            resolved.RegisterClass(classInfo);

        }
        return resolved;
    }


    private static void AppendRawLLVMTypes(RawModule module)
    {
        for (int i = 1; i <= 64; i *= 2)
        {
            RawPrimitiveTypeInfo type = new()
            {
                name = "i" + i,
                asmName = "i" + i
            };
            module.typeInfoByName[type.name] = type;
        }

        RawPrimitiveTypeInfo ptrType = new()
        {
            name = "ptr",
            asmName = "ptr"
        };
        module.typeInfoByName[ptrType.name] = ptrType;
    }
    private static void AppendResolvedLLVMTypes(ResolvedModule module)
    {
        for (int i = 1; i <= 64; i *= 2)
        {
            PrimitiveTypeInfo type = new PrimitiveTypeInfo()
            {
                name = "i" + i,
                asmName = "i" + i
            };
            module.typeInfoByName[type.name] = type;
        }

        PrimitiveTypeInfo.BOOL = (PrimitiveTypeInfo)module.GetType("i1");
        PrimitiveTypeInfo.BYTE = (PrimitiveTypeInfo)module.GetType("i8");
        PrimitiveTypeInfo.SHORT = (PrimitiveTypeInfo)module.GetType("i16");
        PrimitiveTypeInfo.INT = (PrimitiveTypeInfo)module.GetType("i32");
        PrimitiveTypeInfo.LONG = (PrimitiveTypeInfo)module.GetType("i64");

        PrimitiveTypeInfo ptrType = new()
        {
            name = "ptr",
            asmName = "ptr",
        };
        module.typeInfoByName[ptrType.name] = ptrType;
        PrimitiveTypeInfo.PTR = ptrType;
    }
}
