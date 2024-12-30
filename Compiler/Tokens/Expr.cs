public abstract class Node
{
    public string generatedVariableName;

    public virtual void AppendToFlatTree(Dictionary<int, List<Node>> exprsByDepth, int depth)
    {
        if (exprsByDepth.ContainsKey(depth) == false)
        {
            exprsByDepth.Add(depth, new());
        }
    }
    public virtual void Generate(Generator.Context ctx)
    {
    }
}



public class Expr : Node
{
}

public class Node_PrintStatement : Node
{
    public Node expression;

    public override void AppendToFlatTree(Dictionary<int, List<Node>> exprsByDepth, int depth)
    {
        base.AppendToFlatTree(exprsByDepth, depth);

        exprsByDepth[depth].Add(expression);
        depth++;

        expression.AppendToFlatTree(exprsByDepth, depth);
    }
}


public class Expr_Grouping : Expr
{
    public Node expression;

    public override void AppendToFlatTree(Dictionary<int, List<Node>> exprsByDepth, int depth)
    {
        base.AppendToFlatTree(exprsByDepth, depth);

        exprsByDepth[depth].Add(expression);
        depth++;

        expression.AppendToFlatTree(exprsByDepth, depth);
    }
}

public class Node_Block : Node
{
    public List<Node> children = new();

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        foreach (Node child in children)
        {
            child.Generate(ctx);
        }
    }
}

public class Node_While : Node
{
    public Node condition, body;

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