public class Node_Return : Node
{
    public Node expr;

    public override void RegisterRefs(RawModule module)
    {
        expr?.RegisterRefs(module);
    }
    public override void ResolveRefs(ResolvedModule module)
    {
        expr?.ResolveRefs(module);
    }
    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        ctx.b.AppendLine();

        if (expr != null)
        {
            expr.Generate(ctx);

            string retVarName = Utils.SureNotPointer(expr.generatedVariableName, ctx);

            ctx.b.AppendLine($"ret {ctx.GetVariableType(retVarName)} {retVarName}");
        }
        else
        {
            ctx.b.AppendLine("ret void");
        }
    }
}