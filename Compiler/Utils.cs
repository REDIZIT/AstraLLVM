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


        if (isPtr_source == false && isPtr_dest)
        {
            // src = type
            // dst = ptr
            ctx.b.AppendLine($"store {type_source} {sourceVarName}, ptr {destVarName}");
        }
        else if (isPtr_source && isPtr_dest == false)
        {
            // src = ptr
            // dst = type
            ctx.b.AppendLine($"; {destVarName} = {sourceVarName}");
            ctx.b.AppendLine($"store {type_source} {sourceVarName}, ptr {destVarName}");
        }
        else if (isPtr_source && isPtr_dest)
        {
            // src = ptr
            // dst = ptr
            string tempName = ctx.NextTempVariableName(type_source);
            ctx.b.AppendLine($"{tempName} = load {type_source}, ptr {sourceVarName}");
            ctx.b.AppendLine($"store {type_source} {tempName}, ptr {destVarName}");
        }
        else
        {
            // src = type
            // dst = type

            throw new Exception("Failed to move value between 2 value-types (both are not pointer). This feature is not supported by LLVM IR");
        }
    }

    public static string SureNotPointer(string varName, Generator.Context ctx)
    {
        if (ctx.IsPointer(varName) == false) return varName;

        TypeInfo type = ctx.GetPointedType(varName);
        string tempName = ctx.NextTempVariableName(type);
        ctx.b.AppendLine($"{tempName} = load {type}, ptr {varName}");
        return tempName;
    }
}
