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
            TypeInfo type = module.GetType(data.rawType);
            info.returns.Add(type);
        }

        module.functionInfoByName.Add(name, info);


        body.RegisterRefs(module);
    }
    public override void ResolveRefs(Module module)
    {
        body.ResolveRefs(module);
        foreach (VariableRawData rawData in returnValues)
        {
            rawData.Resolve(module);
        }
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