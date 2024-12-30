public class Node_VariableDeclaration : Node
{
    public string variableName;
    public Node initValue;

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        generatedVariableName = ctx.RegisterStackVariable(variableName);
        ctx.b.AppendLine($"{generatedVariableName} = alloca i32");

        if (initValue == null)
        {
            // Default value
            ctx.b.AppendLine($"store i32 0, i32* {generatedVariableName}");
        }
        else
        {
            initValue.Generate(ctx);

            if (ctx.IsPointer(initValue.generatedVariableName))
            {
                string tempName = ctx.NextTempVariableName();
                ctx.b.AppendLine($"{tempName} = load i32, i32* {initValue.generatedVariableName}");
                ctx.b.AppendLine($"store i32 {tempName}, i32* {generatedVariableName}");
            }
            else
            {
                ctx.b.AppendLine($"store i32 {initValue.generatedVariableName}, i32* {generatedVariableName}");
            }
        }

        ctx.b.AppendLine();
    }
}
public class Node_VariableUse : Node
{
    public string variableName;

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        generatedVariableName = "%" + variableName;
    }
}