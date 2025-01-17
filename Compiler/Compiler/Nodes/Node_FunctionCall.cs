public class Node_FunctionCall : Node
{
    public Node caller;
    public List<Node> arguments;
    public string functionName;

    public FunctionInfo function;

    public override void RegisterRefs(RawModule module)
    {
    }
    public override void ResolveRefs(ResolvedModule module)
    {
        function = module.functionInfoByName[functionName];
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        if (function is EmbeddedFunctionInfo embeddedFunctionInfo)
        {
            if (embeddedFunctionInfo is ToPtr_EmbeddedFunctionInfo toPtr)
            {
                Node_VariableUse variable = (Node_VariableUse)((Node_FieldAccess)caller).target;
                string variableName = variable.variableName;
                generatedVariableName = toPtr.Generate(ctx, variableName);
            }
            else if (embeddedFunctionInfo is PtrSet_EmbeddedFunctionInfo ptrSet)
            {
                Node_VariableUse variable = (Node_VariableUse)((Node_FieldAccess)caller).target;
                string variableName = variable.variableName;
                arguments[0].Generate(ctx);
                generatedVariableName = ptrSet.Generate(ctx, variableName, arguments[0].generatedVariableName);
            }
            else
            {
                throw new Exception($"Unknown EmbeddedFunctionInfo '{embeddedFunctionInfo}'");
            }
        }
        else
        {
            //string functionName = ((Node_VariableUse)caller).variableName;
            string functionName = function.name;


            List<string> paramsDeclars = new();
            foreach (Node arg in arguments)
            {
                arg.Generate(ctx);
                TypeInfo argType = ctx.GetVariableType(arg.generatedVariableName);
                string generatedType = (argType is PrimitiveTypeInfo) ? argType.ToString() : "ptr";

                paramsDeclars.Add($"{generatedType} {arg.generatedVariableName}");
            }
            string paramsStr = string.Join(", ", paramsDeclars);


            if (function.returns.Count > 0)
            {
                TypeInfo returnValueType = function.returns[0];

                string tempName = ctx.NextTempVariableName(returnValueType);
                ctx.b.AppendLine($"{tempName} = call {returnValueType} @{functionName}({paramsStr})");
                generatedVariableName = tempName;
            }
            else
            {
                ctx.b.AppendLine($"call void @{functionName}({paramsStr})");
            }
        }
    }
}