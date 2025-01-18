public static class Utils
{
    public static void MoveValue(string sourceVarName, string destVarName, Generator.Context ctx)
    {
        bool isPtr_source = ctx.IsPointer(sourceVarName);
        bool isPtr_dest = ctx.IsPointer(destVarName);

        TypeInfo type_source = ctx.GetVariableType(sourceVarName);
        TypeInfo type_dest = ctx.GetVariableType(destVarName);

        bool isPtr_any = isPtr_source || isPtr_dest;

        if (type_source != type_dest && isPtr_any == false)
        {
            throw new Exception($"Can not move values with different types. Source '{type_source}', dest '{type_dest}'");
        }

        ctx.b.Space();

        if (isPtr_source == false && isPtr_dest)
        {
            // src = type
            // dst = ptr
            ctx.b.CommentLine($"ptr {destVarName} = val {sourceVarName}");
            ctx.b.Line($"store {type_source} {sourceVarName}, ptr {destVarName}");
        }
        else if (isPtr_source && isPtr_dest == false)
        {
            // src = ptr
            // dst = type
            ctx.b.CommentLine($"val {destVarName} = ptr {sourceVarName}");
            ctx.b.Line($"store {type_source} {sourceVarName}, ptr {destVarName}");
        }
        else if (isPtr_source && isPtr_dest)
        {
            // src = ptr
            // dst = ptr
            string tempName = ctx.NextTempVariableName(type_source);
            TypeInfo valueType = ctx.GetPointedType(sourceVarName);

            ctx.b.CommentLine($"ptr {destVarName} = ptr {sourceVarName}");
            ctx.b.Line($"{tempName} = load {valueType}, ptr {sourceVarName}");
            ctx.b.Line($"store {valueType} {tempName}, ptr {destVarName}");
        }
        else
        {
            // src = type
            // dst = type

            throw new Exception("Failed to move value between 2 value-types (both are not pointer). This feature is not supported by LLVM IR");
        }

        ctx.b.Space();
    }

    public static string SureNotPointer(string varName, Generator.Context ctx)
    {
        if (ctx.IsPointer(varName) == false) return varName;

        TypeInfo type = ctx.GetPointedType(varName);
        string tempName = ctx.NextTempVariableName(type);
        ctx.b.Line($"{tempName} = load {type}, ptr {varName}");
        return tempName;
    }
}
