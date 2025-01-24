public class PtrSet_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public string Generate(Generator.Context ctx, string pointerVariableName, string argumentVariableName)
    {
        ctx.b.Space();
        ctx.b.CommentLine($"Set {argumentVariableName} to {pointerVariableName}");

        string argumentValueName = Utils.SureNotPointer(argumentVariableName, ctx);

        string depointed = ctx.NextTempVariableName(PrimitiveTypeInfo.PTR);

        ctx.b.Line($"{depointed} = load i32*, i32* %{pointerVariableName}");
        ctx.b.Line($"store i32 {argumentValueName}, i32* {depointed}");
        ctx.b.Space();

        return null;
    }
}
