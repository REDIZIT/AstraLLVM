public class Expr_Literal : Expr
{
    // TODO
    public Token value;

    public override void AppendToFlatTree(Dictionary<int, List<Expr>> exprsByDepth, int depth)
    {
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        generatedVariableName = ctx.NextLocalVariableName();
        ctx.b.AppendLine($"{generatedVariableName} = alloca i32");
        ctx.b.AppendLine($"store i32 {((Token_Constant)value).value}, i32* {generatedVariableName}");
        ctx.b.AppendLine();
    }
}
