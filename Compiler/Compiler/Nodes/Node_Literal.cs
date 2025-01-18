public class Node_Literal : Node
{
    public Token_Constant constant;

    public override void RegisterRefs(RawModule module)
    {
    }
    public override void ResolveRefs(ResolvedModule module)
    {
    }
    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        PrimitiveTypeInfo literalType = PrimitiveTypeInfo.INT;

        generatedVariableName = ctx.NextPointerVariableName(literalType);
        ctx.b.Line($"{generatedVariableName} = alloca {literalType.asmName}");
        ctx.b.Line($"store {literalType.asmName} {constant.value}, {PrimitiveTypeInfo.PTR} {generatedVariableName}");
        ctx.b.Space();
    }
}
