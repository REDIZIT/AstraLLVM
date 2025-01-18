public class Node_VariableDeclaration : Node
{
    public VariableRawData variable;
    public Node initValue;

    public override void RegisterRefs(RawModule module)
    {
        initValue?.RegisterRefs(module);
    }

    public override void ResolveRefs(ResolvedModule module)
    {
        initValue?.ResolveRefs(module);
        variable.Resolve(module);
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        generatedVariableName = ctx.NextPointerVariableName(variable.type, variable.name);

        if (initValue == null)
        {
            Generate_WithDefaultValue(ctx);
        }
        //else if (initValue is Node_Literal literal)
        //{
        //    Generate_WithInit_Literal(ctx, literal);
        //}
        else if (initValue is Node_New tokenNew)
        {
            Generate_WithInit_New(ctx, tokenNew);
        }
        else
        {
            Generate_WithInit_AnyExpression(ctx);
        }

        ctx.b.Space();
    }

    private void Generate_WithDefaultValue(Generator.Context ctx)
    {
        if (variable.type == PrimitiveTypeInfo.ARRAY)
        {
            Generate_Array_WithDefaultValue(ctx);
            return;
        }

        ctx.b.Line($"{generatedVariableName} = alloca {variable.type}");

        if (variable.type is PrimitiveTypeInfo)
        {
            ctx.b.Line($"store {variable.type} 0, i32* {generatedVariableName}");
        }
        else
        {
            ctx.b.Line("; todo: allocate struct (or class) with default value.");
        }
    }
    private void Generate_WithInit_Literal(Generator.Context ctx, Node_Literal literal)
    {
        ctx.b.Line($"{generatedVariableName} = alloca {variable.type}");
        ctx.b.Line($"store {variable.type} {literal.constant.value}, i32* {generatedVariableName}");
    }
    private void Generate_WithInit_AnyExpression(Generator.Context ctx)
    {
        initValue.Generate(ctx);

        ctx.b.Line($"{generatedVariableName} = alloca {variable.type}");
        Utils.MoveValue(initValue.generatedVariableName, generatedVariableName, ctx);
    }
    private void Generate_WithInit_New(Generator.Context ctx, Node_New tokenNew)
    {
        tokenNew.Generate(ctx, "%" + variable.name);
    }


    private void Generate_Array_WithDefaultValue(Generator.Context ctx)
    {
        ctx.b.Line($"{generatedVariableName} = alloca [10 x i32]");
    }
}

public class Node_VariableUse : Node
{
    public string variableName;

    public override void RegisterRefs(RawModule module)
    {
    }
    public override void ResolveRefs(ResolvedModule module)
    {
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        generatedVariableName = "%" + variableName;
    }
}
