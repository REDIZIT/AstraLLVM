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

public class Expr_Unray : Expr
{
    public Node right;
    public Token @operator;

    public override void AppendToFlatTree(Dictionary<int, List<Node>> exprsByDepth, int depth)
    {
        base.AppendToFlatTree(exprsByDepth, depth);

        exprsByDepth[depth].Add(right);
        depth++;

        right.AppendToFlatTree(exprsByDepth, depth);
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
public class Node_If : Node
{
    public Node condition, thenBranch, elseBranch;

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        condition.Generate(ctx);

        string valueConditionVariable = Utils.SureNotPointer(condition.generatedVariableName, ctx);
        string castedConditionVariable = ctx.NextTempVariableName();
        ctx.b.AppendLine($"{castedConditionVariable} = trunc i32 {valueConditionVariable} to i1");

        if (elseBranch == null)
        {
            ctx.b.AppendLine($"br i1 {castedConditionVariable}, label %if_true, label %if_end");

            ctx.b.AppendLine("if_true:");
            thenBranch.Generate(ctx);
            ctx.b.AppendLine("br label %if_end");

            ctx.b.AppendLine("if_end:");
        }
        else
        {
            ctx.b.AppendLine($"br i1 {castedConditionVariable}, label %if_true, label %if_false");

            ctx.b.AppendLine("if_true:");
            thenBranch.Generate(ctx);
            ctx.b.AppendLine("br label %if_end");

            ctx.b.AppendLine("if_false:");
            elseBranch.Generate(ctx);
            ctx.b.AppendLine("br label %if_end");

            ctx.b.AppendLine("if_end:");
        }

        ctx.b.AppendLine();
    }
}