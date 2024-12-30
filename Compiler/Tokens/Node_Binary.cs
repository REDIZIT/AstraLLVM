public class Node_Binary : Node
{
    public Node left, right;
    public Token @operator;

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        left.Generate(ctx);
        right.Generate(ctx);

        string asmOperatorName = ((Token_Operator)@operator).asmOperatorName;


        string leftName = left.generatedVariableName;
        if (ctx.IsPointer(leftName))
        {
            leftName = ctx.NextTempVariableName();
            ctx.b.AppendLine($"{leftName} = load i32, i32* {left.generatedVariableName}");
        }


        string rightName = right.generatedVariableName;
        if (ctx.IsPointer(rightName))
        {
            rightName = ctx.NextTempVariableName();
            ctx.b.AppendLine($"{rightName} = load i32, i32* {right.generatedVariableName}");
        }
        

        generatedVariableName = ctx.NextTempVariableName();
        ctx.b.AppendLine($"{generatedVariableName} = {asmOperatorName} i32 {leftName}, {rightName}");

        ctx.b.AppendLine();
    }
}
