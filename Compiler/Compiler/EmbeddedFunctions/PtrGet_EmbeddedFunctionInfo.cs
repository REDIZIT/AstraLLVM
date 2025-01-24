public class PtrGet_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public string Generate(Generator.Context ctx, string pointerVariableName)
    {
        ctx.b.Space();
        ctx.b.CommentLine($"Get value from {pointerVariableName}");


        string depointed = ctx.NextTempVariableName(PrimitiveTypeInfo.PTR);

        string tempValue = ctx.NextTempVariableName(PrimitiveTypeInfo.INT);

        ctx.b.Line($"{depointed} = load i32*, i32* %{pointerVariableName}");
        ctx.b.Line($"{tempValue} = load i32, i32* {depointed}");
        ctx.b.Space();

        return tempValue;
    }
}
