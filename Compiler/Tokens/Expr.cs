public abstract class Node
{
    public string generatedVariableName;

    public abstract void RegisterRefs(RawModule module);
    public abstract void ResolveRefs(ResolvedModule resolved);

    public virtual void Generate(Generator.Context ctx)
    {
    }
}

public class Expr_Grouping : Node
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
}

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

        ctx.b.AppendLine("br label %while_condition");
        ctx.b.AppendLine("while_condition:");
        condition.Generate(ctx);

        string conditionName = Utils.SureNotPointer(condition.generatedVariableName, ctx);
        ctx.b.AppendLine($"br i1 {conditionName}, label %while_body, label %while_end");

        ctx.b.AppendLine("while_body:");
        body.Generate(ctx);
        ctx.b.AppendLine("br label %while_condition");

        ctx.b.AppendLine("while_end:");
    }
}