public class Node_VariableDeclaration : Node
{
    public VariableRawData variable;
    public Node initValue;

    public override void RegisterRefs(Module module)
    {
        initValue?.RegisterRefs(module);
    }

    public override void ResolveRefs(Module module)
    {
        initValue?.ResolveRefs(module);
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        generatedVariableName = ctx.RegisterStackVariable(variable.name, variable.type);
        string type = variable.type;

        if (initValue == null)
        {
            // Default value
            ctx.b.AppendLine($"{generatedVariableName} = alloca {type}");
            ctx.b.AppendLine($"store {type} 0, i32* {generatedVariableName}");
        }
        else
        {
            if (initValue is Node_Literal literal)
            {
                ctx.b.AppendLine($"{generatedVariableName} = alloca {type}");
                ctx.b.AppendLine($"store {type} {literal.constant.value}, i32* {generatedVariableName}");
            }
            else
            {
                initValue.Generate(ctx);

                ctx.b.AppendLine($"{generatedVariableName} = alloca {type}");

                if (ctx.IsPointer(initValue.generatedVariableName))
                {
                    string tempName = ctx.NextTempVariableName("{type}");
                    ctx.b.AppendLine($"{tempName} = load {type}, i32* {initValue.generatedVariableName}");
                    ctx.b.AppendLine($"store {type} {tempName}, i32* {generatedVariableName}");
                }
                else
                {
                    ctx.b.AppendLine($"store {type} {initValue.generatedVariableName}, i32* {generatedVariableName}");
                }
            }
        }

        ctx.b.AppendLine();
    }
}
public class Node_VariableUse : Node
{
    public string variableName;

    public override void RegisterRefs(Module module)
    {
    }
    public override void ResolveRefs(Module module)
    {
    }

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

    public override void RegisterRefs(Module module)
    {
        value.RegisterRefs(module);
    }
    public override void ResolveRefs(Module module)
    {
        value.ResolveRefs(module);
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        value.Generate(ctx);

        Utils.MoveValue(value.generatedVariableName, "%" + variableName, ctx);
    }
}