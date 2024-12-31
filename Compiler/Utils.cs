public static class Utils
{
    public static void MoveValue(string sourceVarName, string destVarName, Generator.Context ctx)
    {
        bool isPtr_source = ctx.IsPointer(sourceVarName);
        bool isPtr_dest = ctx.IsPointer(destVarName);

        TypeInfo type_source = ctx.GetVariableType(sourceVarName);
        TypeInfo type_dest = ctx.GetVariableType(destVarName);

        if (type_source != type_dest)
        {
            throw new Exception($"Can not move values with different types. Source '{type_source}', dest '{type_dest}'");
        }


        if (isPtr_source == false && isPtr_dest)
        {
            ctx.b.AppendLine($"store {type_source} {sourceVarName}, i32* {destVarName}");
        }
        else if (isPtr_source && isPtr_dest)
        {
            string tempName = ctx.NextTempVariableName(type_source);
            ctx.b.AppendLine($"{tempName} = load {type_source}, i32* {sourceVarName}");
            ctx.b.AppendLine($"store {type_source} {tempName}, i32* {destVarName}");
        }
        else
        {
            throw new NotImplementedException();
        }
    }
    public static string SureNotPointer(string varName, Generator.Context ctx)
    {
        if (ctx.IsPointer(varName) == false) return varName;

        TypeInfo type = ctx.GetVariableType(varName);
        string tempName = ctx.NextTempVariableName(type);
        ctx.b.AppendLine($"{tempName} = load {type}, {type}* {varName}");
        return tempName;
    }
}
