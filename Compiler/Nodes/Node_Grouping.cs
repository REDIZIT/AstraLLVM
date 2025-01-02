public class Node_Grouping : Node
{
    public Node expression;

    public override void RegisterRefs(RawModule module)
    {
        expression.RegisterRefs(module);
    }
    public override void ResolveRefs(ResolvedModule resolved)
    {
        expression.ResolveRefs(resolved);
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        expression.Generate(ctx);
        generatedVariableName = expression.generatedVariableName;
    }
}
