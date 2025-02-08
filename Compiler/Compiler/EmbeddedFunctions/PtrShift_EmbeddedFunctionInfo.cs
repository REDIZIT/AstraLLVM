public class PtrShift_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public string Generate(Generator.Context ctx, string pointerVariableName, string shiftVariableName)
    {
        ctx.b.Space();
        ctx.b.CommentLine($"Shift pointer {pointerVariableName} by {shiftVariableName}");

        string shiftValue = Utils.SureNotPointer(shiftVariableName, ctx);
        string depointed = ctx.NextTempVariableName(PrimitiveTypeInfo.PTR);
        string tempValue = ctx.NextTempVariableName(PrimitiveTypeInfo.INT);

        ctx.b.Line($"{depointed} = load i32, i32* %{pointerVariableName}");
        ctx.b.Line($"{tempValue} = add i32 {shiftValue}, {depointed}");

        ctx.b.Line($"store i32 {tempValue}, i32* %{pointerVariableName}");

        ctx.b.Space();

        return null;
    }
}