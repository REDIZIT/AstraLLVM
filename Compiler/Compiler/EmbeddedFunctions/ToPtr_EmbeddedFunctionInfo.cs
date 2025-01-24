public class ToPtr_EmbeddedFunctionInfo : EmbeddedFunctionInfo
{
    public string Generate(Generator.Context ctx, string valueName)
    {
        string ptr_name = ctx.NextPointerVariableName(PrimitiveTypeInfo.PTR);
        ctx.b.CommentLine("ptr_name = " + ptr_name + " for " + valueName);
        ctx.b.Line($"{ptr_name} = alloca i32*");
        ctx.b.Line($"store i32* %{valueName}, i32** {ptr_name}");
        ctx.b.Space();
        return ptr_name;
    }
}
