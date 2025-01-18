public class Node_Class : Node
{
    public string name;
    public Node_Block body;

    public override void RegisterRefs(RawModule raw)
    {
        var rawInfo = new RawClassTypeInfo()
        {
            name = name
        };

        raw.RegisterClass(rawInfo);




        Node_Block ctorBlock = new();

        //
        // Register RawFieldInfos
        //
        foreach (Node statement in body.children)
        {
            if (statement is Node_VariableDeclaration declaration)
            {
                RawFieldInfo field = new()
                {
                    name = declaration.variable.name,
                    typeName = declaration.variable.rawType
                };
                rawInfo.fields.Add(field);

                

                ctorBlock.children.Add(new Node_VariableAssign()
                {
                    target = new Node_FieldAccess()
                    {
                        target = new Node_VariableUse()
                        {
                            variableName = "self"
                        },
                        targetFieldName = field.name
                    },
                    value = declaration.initValue
                });
            }
        }

        ctorBlock.children.Add(new Node_Return());

        Node_Function ctorNode = new Node_Function()
        {
            name = name + "__ctor",
            body = ctorBlock,
        };
        ctorNode.parameters.Add(new VariableRawData()
        {
            name = "self",
            rawType = name
        });
        body.children.Add(ctorNode);


        body.RegisterRefs(raw);
    }
    public override void ResolveRefs(ResolvedModule resolved)
    {
        body.ResolveRefs(resolved);
    }
    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        foreach (Node statement in body.children)
        {
            if (statement is Node_Function)
            {
                statement.Generate(ctx);
            }
            else if (statement is Node_VariableDeclaration == false)
            {
                throw new Exception($"For class generation expected only {nameof(Node_Function)} or {nameof(Node_VariableDeclaration)} but got {statement}");
            }
        }
    }
}
public class Node_New : Node
{
    public string className;

    public ClassTypeInfo classInfo;

    public override void RegisterRefs(RawModule module)
    {
        
    }
    public override void ResolveRefs(ResolvedModule resolved)
    {
        classInfo = resolved.classInfoByName[className];
    }
    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        string tempName = ctx.NextPointerVariableName(classInfo);
        Generate(ctx, tempName);
    }
    public void Generate(Generator.Context ctx, string generatedName)
    {
        ctx.b.Line($"{generatedName} = alloca %{classInfo.name}");
        ctx.b.Line($"call void @{classInfo.name}__ctor(ptr {generatedName})");
        ctx.b.Space();
        generatedVariableName = generatedName;
    }
}