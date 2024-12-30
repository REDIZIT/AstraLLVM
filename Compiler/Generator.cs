using System.Text;

public static class Generator
{
    public class Context
    {
        public StringBuilder b = new();
        public HashSet<string> variables = new();
        public int tempVariablesCount = 0;

        public string NextLocalVariableName()
        {
            string varName = $"%local_{variables.Count}";
            variables.Add(varName);
            return varName;
        }
        public string NextTempVariableName()
        {
            return $"%tmp_{tempVariablesCount++}";
        }
    }

    public static string Generate(List<Statement> statements)
    {
        Context ctx = new();

        foreach (Statement statement in statements)
        {
            Dictionary<int, List<Expr>> exprsByDepth = new();
            statement.AppendToFlatTree(exprsByDepth, 0);

            for (int i = exprsByDepth.Count - 1; i >= 0; i--)
            {
                foreach (Expr expression in exprsByDepth[i])
                {
                    expression.Generate(ctx);
                }
            }
        }

        return FormatLLVM(ctx.b.ToString());
    }


    private static string FormatLLVM(string llvm)
    {
        int depth = 0;

        string[] lines = llvm.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (line.Contains("}"))
            {
                depth--;
            }
            
            if (depth > 0)
            {
                lines[i] = '\t' + line;
            }

            if (line.Contains("{"))
            {
                depth++;
            }
        }

        return string.Join('\n', lines);
    }
}
