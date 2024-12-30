public class Node_VariableDeclaration : Node
{
    public string variableName;
    public Node initValue;

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        generatedVariableName = ctx.RegisterStackVariable(variableName);
        
        if (initValue == null)
        {
            // Default value
            ctx.b.AppendLine($"{generatedVariableName} = alloca i32");
            ctx.b.AppendLine($"store i32 0, i32* {generatedVariableName}");
        }
        else
        {
            if (initValue is Node_Literal literal)
            {
                ctx.b.AppendLine($"{generatedVariableName} = alloca i32");
                ctx.b.AppendLine($"store i32 {literal.constant.value}, i32* {generatedVariableName}");
            }
            else
            {
                initValue.Generate(ctx);

                ctx.b.AppendLine($"{generatedVariableName} = alloca i32");

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
public class Node_VariableAssign : Node
{
    public string variableName;
    public Node value;

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        value.Generate(ctx);

        Utils.MoveValue(value.generatedVariableName, "%" + variableName, ctx);
    }
}