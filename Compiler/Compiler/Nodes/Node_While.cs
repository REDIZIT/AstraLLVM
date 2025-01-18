public class Node_While : Node
{
    public Node condition, body;

    public override void RegisterRefs(RawModule module)
    {
        condition.RegisterRefs(module);
        body.RegisterRefs(module);
    }
    public override void ResolveRefs(ResolvedModule resolved)
    {
        condition.ResolveRefs(resolved);
        body.ResolveRefs(resolved);
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        ctx.b.Line("br label %while_condition");
        ctx.b.Line("while_condition:");
        condition.Generate(ctx);

        string conditionName = Utils.SureNotPointer(condition.generatedVariableName, ctx);
        ctx.b.Line($"br i1 {conditionName}, label %while_body, label %while_end");

        ctx.b.Line("while_body:");
        body.Generate(ctx);
        ctx.b.Line("br label %while_condition");

        ctx.b.Line("while_end:");
    }
}