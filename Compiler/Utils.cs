public static class Utils
{
    public static void MoveValue(string sourceVarName, string destVarName, Generator.Context ctx)
    {
        if (ctx.IsPointer(destVarName) && ctx.IsPointer(sourceVarName) == false)
        {
            ctx.b.AppendLine($"store i32 {sourceVarName}, i32* {destVarName}");
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}
