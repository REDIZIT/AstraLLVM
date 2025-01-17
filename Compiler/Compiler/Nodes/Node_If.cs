﻿public class Node_If : Node
{
    public Node condition, thenBranch, elseBranch;

    public override void RegisterRefs(RawModule module)
    {
        condition.RegisterRefs(module);
        thenBranch.RegisterRefs(module);
        elseBranch?.RegisterRefs(module);
    }
    public override void ResolveRefs(ResolvedModule module)
    {
        condition.ResolveRefs(module);
        thenBranch.ResolveRefs(module);
        elseBranch?.ResolveRefs(module);
    }
    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        condition.Generate(ctx);

        string valueConditionVariable = Utils.SureNotPointer(condition.generatedVariableName, ctx);

        if (ctx.GetVariableType(valueConditionVariable) != PrimitiveTypeInfo.BOOL)
        {
            string castedConditionVariable = ctx.NextTempVariableName(PrimitiveTypeInfo.BOOL);
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