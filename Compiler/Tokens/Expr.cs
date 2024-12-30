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

//public class Node_Program : Node
//{
//    public List<Node> children = new();
//}
//public class Node_VariableDeclaration : Node
//{
//    public string name;
//    public Node child;

//    public override void AppendToFlatTree(Dictionary<int, List<Node>> exprsByDepth, int depth)
//    {
//        base.AppendToFlatTree(exprsByDepth, depth);

//        exprsByDepth[depth].Add(child);
//        depth++;

//        child.AppendToFlatTree(exprsByDepth, depth);
//    }

//    public override void Generate(Generator.Context ctx)
//    {
//        base.Generate(ctx);

//        generatedVariableName = ctx.RegisterLocalVariable(name);
//        ctx.b.AppendLine($"{generatedVariableName} = alloca i32");

//        if (child == null)
//        {
//            ctx.b.AppendLine($"store i32 0, i32* {generatedVariableName}");
//        }
//        else
//        {
//            ctx.b.AppendLine($"store i32 {child.generatedVariableName}, i32* {generatedVariableName}");
//        }
//    }
//}
//public class Node_Statement : Node
//{
//    public Node child;
//}

//public class Node_ExpressionStatement : Node
//{
//    public Node expression;

//    public override void AppendToFlatTree(Dictionary<int, List<Node>> exprsByDepth, int depth)
//    {
//        base.AppendToFlatTree(exprsByDepth, depth);

//        exprsByDepth[depth].Add(expression);
//        depth++;

//        expression.AppendToFlatTree(exprsByDepth, depth);
//    }
//}
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


//public class Node_Expression : Expr
//{
//    public Node left;
//    public Token @operator;
//    public Node right;

//    public override void AppendToFlatTree(Dictionary<int, List<Node>> exprsByDepth, int depth)
//    {
//        base.AppendToFlatTree(exprsByDepth, depth);

//        exprsByDepth[depth].Add(left);
//        exprsByDepth[depth].Add(right);
//        depth++;

//        left.AppendToFlatTree(exprsByDepth, depth);
//        right.AppendToFlatTree(exprsByDepth, depth);
//    }
//}
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