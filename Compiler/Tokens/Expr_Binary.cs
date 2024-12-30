public class Expr_Binary : Expr
{
    public Expr left, right;
    public Token @operator;

    public override void AppendToFlatTree(Dictionary<int, List<Expr>> exprsByDepth, int depth)
    {
        base.AppendToFlatTree(exprsByDepth, depth);

        exprsByDepth[depth].Add(left);
        exprsByDepth[depth].Add(right);
        depth++;

        left.AppendToFlatTree(exprsByDepth, depth);
        right.AppendToFlatTree(exprsByDepth, depth);
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        string asmOperatorName = ((Token_Operator)@operator).asmOperatorName;


        string leftName = left.generatedVariableName;
        if (ctx.variables.Contains(leftName))
        {
            leftName = ctx.NextTempVariableName();
            ctx.b.AppendLine($"{leftName} = load i32, i32* {left.generatedVariableName}");
        }


        string rightName = right.generatedVariableName;
        if (ctx.variables.Contains(rightName))
        {
            rightName = ctx.NextTempVariableName();
            ctx.b.AppendLine($"{rightName} = load i32, i32* {right.generatedVariableName}");
        }
        

        generatedVariableName = ctx.NextTempVariableName();
        ctx.b.AppendLine($"{generatedVariableName} = {asmOperatorName} i32 {leftName}, {rightName}");

        ctx.b.AppendLine();
    }
}
