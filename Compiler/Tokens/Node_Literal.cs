public class Node_Literal : Node
{
    public Token_Constant constant;

    public override void RegisterRefs(Module module)
    {
    }
    public override void ResolveRefs(Module module)
    {
    }
    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        PrimitiveTypeInfo literalType = PrimitiveTypeInfo.INT;

        generatedVariableName = ctx.NextStackUnnamedVariableName(literalType);
        ctx.b.AppendLine($"{generatedVariableName} = alloca {literalType.asmName}");
        ctx.b.AppendLine($"store {literalType.asmName} {constant.value}, {PrimitiveTypeInfo.PTR} {generatedVariableName}");
        ctx.b.AppendLine();
    }
}
