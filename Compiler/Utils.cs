public static class Utils
{
    public static void MoveValue(string sourceVarName, string destVarName, Generator.Context ctx)
    {
        bool isPtr_source = ctx.IsPointer(sourceVarName);
        bool isPtr_dest = ctx.IsPointer(destVarName);

        if (isPtr_source == false && isPtr_dest)
        {
            ctx.b.AppendLine($"store i32 {sourceVarName}, i32* {destVarName}");
        }
        else if (isPtr_source && isPtr_dest)
        {
            string tempName = ctx.NextTempVariableName();
            ctx.b.AppendLine($"{tempName} = load i32, i32* {sourceVarName}");
            ctx.b.AppendLine($"store i32 {tempName}, i32* {destVarName}");
        }
        else
        {
            throw new NotImplementedException();
        }
    }
    public static string SureNotPointer(string varName, Generator.Context ctx)
    {
        if (ctx.IsPointer(varName) == false) return varName;
        
        string tempName = ctx.NextTempVariableName();
        ctx.b.AppendLine($"{tempName} = load i32, i32* {varName}");
        return tempName;
    }
}
