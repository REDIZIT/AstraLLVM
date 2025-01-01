public class Node_FieldGet : Node
{
    public Node target;
    public string targetFieldName;

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

        string typeName = ctx.GetVariableType(target.generatedVariableName).name;
        ClassTypeInfo targetType = ctx.module.classInfoByName[typeName];

        int indexOfField = targetType.fields.IndexOf(i => i.name == targetFieldName);
        if (indexOfField == -1) throw new Exception($"Field '{targetFieldName}' not found in class '{targetType}'");
        FieldInfo fieldInfo = targetType.fields[indexOfField];

        string ptr = ctx.NextTempVariableName(PrimitiveTypeInfo.PTR);
        ctx.b.AppendLine($"{ptr} = getelementptr {targetType}, ptr {target.generatedVariableName}, i32 0, i32 {indexOfField}");

        generatedVariableName = ctx.NextTempVariableName(fieldInfo.type);
        ctx.b.AppendLine($"{generatedVariableName} = load {fieldInfo.type}, ptr {ptr}");

    }
}
