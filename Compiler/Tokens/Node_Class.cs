public class Node_Class : Node
{
    public string name;
    public Node body;

    public override void RegisterRefs(Module module)
    {
        ClassTypeInfo typeInfo = new ClassTypeInfo()
        {
            name = name
        };

        module.typeInfoByName.Add(name, typeInfo);
        module.classInfoByName.Add(name, typeInfo);

        body.RegisterRefs(module);
    }
    public override void ResolveRefs(Module module)
    {
        body.ResolveRefs(module);
    }
    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        body.Generate(ctx);
    }
}
public class Node_New : Node
{
    public string className;

    public ClassTypeInfo classInfo;

    public override void RegisterRefs(Module module)
    {
        
    }
    public override void ResolveRefs(Module module)
    {
        classInfo = module.classInfoByName[className];
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