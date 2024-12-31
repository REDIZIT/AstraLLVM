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

        TypeInfo returnValueType;
        if (function.returns.Count > 0)
        {
            returnValueType = function.returns[0];
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