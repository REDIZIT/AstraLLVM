﻿public class Node_Literal : Node
{
    // TODO
    public Token value;

    public override void AppendToFlatTree(Dictionary<int, List<Node>> exprsByDepth, int depth)
    {
        base.AppendToFlatTree(exprsByDepth, depth);

        exprsByDepth[depth].Add(this);
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        generatedVariableName = ctx.NextStackUnnamedVariableName();
        ctx.b.AppendLine($"{generatedVariableName} = alloca i32");
        ctx.b.AppendLine($"store i32 {((Token_Constant)value).value}, i32* {generatedVariableName}");
        ctx.b.AppendLine();
    }
}