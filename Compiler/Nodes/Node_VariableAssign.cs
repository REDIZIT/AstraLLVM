
public class Node_VariableAssign : Node
{
    public Node target; // variable or field get
    public Node value;

    public override void RegisterRefs(RawModule module)
    {
        value.RegisterRefs(module);
    }
    public override void ResolveRefs(ResolvedModule module)
    {
        value.ResolveRefs(module);
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        target.Generate(ctx);
        value.Generate(ctx);

        Utils.MoveValue(value.generatedVariableName, target.generatedVariableName, ctx);
    }
}