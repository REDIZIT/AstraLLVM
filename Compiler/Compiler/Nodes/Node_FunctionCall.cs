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
                string valueVariableName = variable.variableName;
                generatedVariableName = toPtr.Generate(ctx, valueVariableName);
            }
            else if (embeddedFunctionInfo is PtrSet_EmbeddedFunctionInfo ptrSet)
            {
                Node_VariableUse variable = (Node_VariableUse)((Node_FieldAccess)caller).target;
                string pointerVariableName = variable.variableName;
                arguments[0].Generate(ctx);
                generatedVariableName = ptrSet.Generate(ctx, pointerVariableName, arguments[0].generatedVariableName);
            }
            else if (embeddedFunctionInfo is PtrGet_EmbeddedFunctionInfo ptrGet)
            {
                Node_VariableUse variable = (Node_VariableUse)((Node_FieldAccess)caller).target;
                string pointerVariableName = variable.variableName;
                generatedVariableName = ptrGet.Generate(ctx, pointerVariableName);
            }
            else if (embeddedFunctionInfo is PtrShift_EmbeddedFunctionInfo ptrShift)
            {
                Node_VariableUse variable = (Node_VariableUse)((Node_FieldAccess)caller).target;
                string pointerVariableName = variable.variableName;
                arguments[0].Generate(ctx);
                generatedVariableName = ptrShift.Generate(ctx, pointerVariableName, arguments[0].generatedVariableName);
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
                ctx.b.Line($"{tempName} = call {returnValueType} @{functionName}({paramsStr})");
                generatedVariableName = tempName;
            }
            else
            {
                ctx.b.Line($"call void @{functionName}({paramsStr})");
            }
        }
    }
}