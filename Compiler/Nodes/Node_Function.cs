public class Node_Function : Node
{
    public string name;
    public Node body;
    public List<Node> parameters;
    public List<VariableRawData> returnValues = new();

    public override void RegisterRefs(RawModule raw)
    {
        RawFunctionInfo rawInfo = new()
        {
            name = name
        };

        foreach (VariableRawData data in returnValues)
        {
            rawInfo.returns.Add(new RawTypeInfo()
            {
                name = data.rawType
            });
        }

        raw.RegisterFunction(rawInfo);

        body.RegisterRefs(raw);
    }
    public override void ResolveRefs(ResolvedModule module)
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