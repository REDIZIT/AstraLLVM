public class Node_FieldAccess : Node
{
    public Node target;
    public string targetFieldName;

    // Valid after Generate
    protected FieldInfo fieldInfo;

    public override void RegisterRefs(RawModule module)
    {
    }

    public override void ResolveRefs(ResolvedModule resolved)
    {
    }

    public override void Generate(Generator.Context ctx)
    {
        base.Generate(ctx);

        target.Generate(ctx);

        string typeName = ctx.GetPointedType(target.generatedVariableName).name;
        ClassTypeInfo targetType = ctx.module.classInfoByName[typeName];

        int indexOfField = targetType.fields.IndexOf(i => i.name == targetFieldName);
        if (indexOfField == -1) throw new Exception($"Field '{targetFieldName}' not found in class '{targetType}'");
        fieldInfo = targetType.fields[indexOfField];

        string ptr = ctx.NextPointerVariableName(fieldInfo.type);
        ctx.b.Line($"{ptr} = getelementptr {targetType}, ptr {target.generatedVariableName}, i32 0, i32 {indexOfField}");
        ctx.b.Space();

        generatedVariableName = ptr;
    }
}