public class Node_Block : Node
{
    public List<Node> children = new();

    public override void RegisterRefs(RawModule module)
    {
        foreach (Node child in children) child.RegisterRefs(module);
    }

    public override void ResolveRefs(ResolvedModule resolved)
    {
        foreach (Node child in children) child.ResolveRefs(resolved);
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);
        foreach (Node child in children) child.Generate(ctx);
    }
}
