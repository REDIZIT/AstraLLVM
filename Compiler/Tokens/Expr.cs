using System.Text;

public abstract class Statement
{
    public virtual void AppendToFlatTree(Dictionary<int, List<Expr>> exprsByDepth, int depth)
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

public abstract class Expr : Statement
{
    public string generatedVariableName;
}

public class Node_Return : Expr
{
    public Expr child;

    public override void AppendToFlatTree(Dictionary<int, List<Expr>> exprsByDepth, int depth)
    {
        base.AppendToFlatTree(exprsByDepth, depth);

        exprsByDepth[depth].Add(child);
        depth++;

        child.AppendToFlatTree(exprsByDepth, depth);
    }
}


public class ExprStmt : Statement
{
    public Node_Expression expression;

    public override void AppendToFlatTree(Dictionary<int, List<Expr>> exprsByDepth, int depth)
    {
        base.AppendToFlatTree(exprsByDepth, depth);

        exprsByDepth[depth].Add(expression);
        depth++;

        expression.AppendToFlatTree(exprsByDepth, depth);
    }
}
public class PrintStmt : Statement
{
    public Expr expression;

    public override void AppendToFlatTree(Dictionary<int, List<Expr>> exprsByDepth, int depth)
    {
        base.AppendToFlatTree(exprsByDepth, depth);

        exprsByDepth[depth].Add(expression);
        depth++;

        expression.AppendToFlatTree(exprsByDepth, depth);
    }
}


public class Node_Expression : Expr
{
    public Node_Expression left;
    public Token @operator;
    public Node_Expression right;

    public override void AppendToFlatTree(Dictionary<int, List<Expr>> exprsByDepth, int depth)
    {
        base.AppendToFlatTree(exprsByDepth, depth);

        exprsByDepth[depth].Add(left);
        exprsByDepth[depth].Add(right);
        depth++;

        left.AppendToFlatTree(exprsByDepth, depth);
        right.AppendToFlatTree(exprsByDepth, depth);
    }
}
public class Expr_Unray : Expr
{
    public Expr right;
    public Token @operator;

    public override void AppendToFlatTree(Dictionary<int, List<Expr>> exprsByDepth, int depth)
    {
        base.AppendToFlatTree(exprsByDepth, depth);

        exprsByDepth[depth].Add(right);
        depth++;

        right.AppendToFlatTree(exprsByDepth, depth);
    }
}
public class Expr_Grouping : Expr
{
    public Expr expression;

    public override void AppendToFlatTree(Dictionary<int, List<Expr>> exprsByDepth, int depth)
    {
        base.AppendToFlatTree(exprsByDepth, depth);

        exprsByDepth[depth].Add(expression);
        depth++;

        expression.AppendToFlatTree(exprsByDepth, depth);
    }
}