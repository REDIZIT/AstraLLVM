﻿public class Node_VariableDeclaration : Node
{
    public VariableRawData variable;
    public Node initValue;

    private Module module;

    public override void RegisterRefs(Module module)
    {
        this.module = module;
        initValue?.RegisterRefs(module);
    }

    public override void ResolveRefs(Module module)
    {
        initValue?.ResolveRefs(module);
        variable.Resolve(module);
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        generatedVariableName = ctx.RegisterStackVariable(variable.name, variable.type);

        if (initValue == null)
        {
            Generate_WithDefaultValue(ctx);
        }
        else if (initValue is Node_Literal literal)
        {
            Generate_WithInit_Literal(ctx, literal);
        }
        else if (initValue is Node_New tokenNew)
        {
            Generate_WithInit_New(ctx, tokenNew);
        }
        else
        {
            Generate_WithInit_AnyExpression(ctx);
        }

        ctx.b.AppendLine();
    }

    private void Generate_WithDefaultValue(Generator.Context ctx)
    {
        ctx.b.AppendLine($"{generatedVariableName} = alloca {variable.type}");

        if (variable.type is PrimitiveTypeInfo)
        {
            ctx.b.AppendLine($"store {variable.type} 0, i32* {generatedVariableName}");
        }
        else
        {
            ctx.b.AppendLine("; todo: allocate struct (or class) with default value.");
        }
    }
    private void Generate_WithInit_Literal(Generator.Context ctx, Node_Literal literal)
    {
        ctx.b.AppendLine($"{generatedVariableName} = alloca {variable.type}");
        ctx.b.AppendLine($"store {variable.type} {literal.constant.value}, i32* {generatedVariableName}");
    }
    private void Generate_WithInit_AnyExpression(Generator.Context ctx)
    {
        initValue.Generate(ctx);

        ctx.b.AppendLine($"{generatedVariableName} = alloca {variable.type}");
        Utils.MoveValue(initValue.generatedVariableName, generatedVariableName, ctx);
    }
    private void Generate_WithInit_New(Generator.Context ctx, Node_New tokenNew)
    {
        ctx.b.AppendLine($"{generatedVariableName} = alloca %{tokenNew.classInfo.name}");
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