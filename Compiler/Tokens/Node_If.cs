public class Node_If : Node
{
    public Node condition, thenBranch, elseBranch;

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        condition.Generate(ctx);

        string valueConditionVariable = Utils.SureNotPointer(condition.generatedVariableName, ctx);

        if (ctx.GetVariableType(valueConditionVariable) != "i1")
        {
            string castedConditionVariable = ctx.NextTempVariableName("i1");
            ctx.b.AppendLine($"{castedConditionVariable} = trunc {ctx.GetVariableType(valueConditionVariable)} {valueConditionVariable} to i1");
            valueConditionVariable = castedConditionVariable;
        }


        if (elseBranch == null)
        {
            ctx.b.AppendLine($"br i1 {valueConditionVariable}, label %if_true, label %if_end");

            ctx.b.AppendLine("if_true:");
            thenBranch.Generate(ctx);
            ctx.b.AppendLine("br label %if_end");

            ctx.b.AppendLine("if_end:");
        }
        else
        {
            ctx.b.AppendLine($"br i1 {valueConditionVariable}, label %if_true, label %if_false");

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