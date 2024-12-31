public class Node_Class : Node
{
    public string name;
    public Node body;

    public override void RegisterRefs(Module module)
    {
        module.classInfoByName.Add(name, new ClassInfo()
        {
            name = name
        });

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

    public ClassInfo classInfo;

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

        ctx.b.AppendLine("; alloca class ");
    }
}