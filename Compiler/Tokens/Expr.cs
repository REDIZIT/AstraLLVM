public abstract class Node
{
    public string generatedVariableName;

    public abstract void RegisterRefs(Module module);
    public abstract void ResolveRefs(Module module);

    public virtual void Generate(Generator.Context ctx)
    {
    }
}

public class Expr_Grouping : Node
{
    public Node expression;

    public override void RegisterRefs(Module module)
    {
        expression.RegisterRefs(module);
    }
    public override void ResolveRefs(Module module)
    {
        expression.ResolveRefs(module);
    }
}

public class Node_Block : Node
{
    public List<Node> children = new();

    public override void RegisterRefs(Module module)
    {
        foreach (Node child in children) child.RegisterRefs(module);
    }

    public override void ResolveRefs(Module module)
    {
        foreach (Node child in children) child.ResolveRefs(module);
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);
        foreach (Node child in children) child.Generate(ctx);
    }
}

public class Node_While : Node
{
    public Node condition, body;

    public override void RegisterRefs(Module module)
    {
        condition.RegisterRefs(module);
        body.RegisterRefs(module);
    }
    public override void ResolveRefs(Module module)
    {
        condition.ResolveRefs(module);
        body.ResolveRefs(module);
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        ctx.b.AppendLine("br label %while_condition");
        ctx.b.AppendLine("while_condition:");
        condition.Generate(ctx);

        string conditionName = Utils.SureNotPointer(condition.generatedVariableName, ctx);
        ctx.b.AppendLine($"br i1 {conditionName}, label %while_body, label %while_end");

        ctx.b.AppendLine("while_body:");
        body.Generate(ctx);
        ctx.b.AppendLine("br label %while_condition");

        ctx.b.AppendLine("while_end:");
    }
}
public class Node_FunctionCall : Node
{
    public Node caller;
    public List<Node> arguments;
    public string functionName;

    public FunctionInfo function;

    public override void RegisterRefs(Module module)
    {
    }
    public override void ResolveRefs(Module module)
    {
        function = module.functionInfoByName[functionName];
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        string returnValueType;
        if (function.returns.Count > 0)
        {
            returnValueType = function.returns[0].asmName;
        }
        else
        {
            throw new Exception("Function does not return any value, but assigning variable.");
        }

        string tempName = ctx.NextTempVariableName(returnValueType);

        ctx.b.AppendLine($"{tempName} = call {returnValueType} @{((Node_VariableUse)caller).variableName}()");
        generatedVariableName = tempName;
    }
}

public class Node_Function : Node
{
    public string name;
    public Node body;
    public List<Node> parameters;
    public List<VariableRawData> returnValues;

    public override void RegisterRefs(Module module)
    {
        FunctionInfo info = new()
        {
            name = name
        };

        foreach (VariableRawData data in returnValues)
        {
            TypeInfo type = module.typeInfoByName[data.type];
            info.returns.Add(type);
        }

        module.functionInfoByName.Add(name, info);


        body.RegisterRefs(module);
    }
    public override void ResolveRefs(Module module)
    {
        body.ResolveRefs(module);
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        if (returnValues.Count > 1)
        {
            throw new NotImplementedException("Function has 1+ return values. Generation for this is not supported yet");
        }

        if (returnValues.Count == 0)
        {
            ctx.b.AppendLine($"define void @{name}()");
        }
        else
        {
            ctx.b.AppendLine($"define {returnValues[0].type} @{name}()");
        }
        
        ctx.b.AppendLine("{");

        body.Generate(ctx);

        ctx.b.AppendLine("}");
    }
}
