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
            }
        }

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

        string tempName = ctx.NextStackUnnamedVariableName(classInfo);
        ctx.b.AppendLine($"{tempName} = alloca %{classInfo.name}");
        ctx.b.AppendLine();
        generatedVariableName = tempName;
    }
}