using System.Text;

public static class Generator
{
    public class Context
    {
        public StringBuilder b = new();
        public HashSet<string> stackVariables = new();
        public HashSet<string> tempVariables = new();
        public int tempVariablesCount = 0;
        public int localVariablesCount = 0;

        public string NextStackUnnamedVariableName()
        {
            string varName = $"%local_{localVariablesCount}";
            localVariablesCount++;
            stackVariables.Add(varName);
            return varName;
        }
        public string NextTempVariableName()
        {
            string varName = $"%tmp_{tempVariablesCount}";
            tempVariablesCount++;
            tempVariables.Add(varName);
            return varName;
        }
        public string RegisterStackVariable(string name)
        {
            string varName = "%" + name;
            stackVariables.Add(varName);
            return varName;
        }

        public bool IsPointer(string generatedName)
        {
            return stackVariables.Contains(generatedName);
        }
    }

    public static string Generate(List<Node> statements)
    {
        Context ctx = new();

        foreach (Node statement in statements)
        {
            //Dictionary<int, List<Node>> exprsByDepth = new();
            //statement.AppendToFlatTree(exprsByDepth, 0);

            //for (int di = exprsByDepth.Count - 1; di >= 0; di--)
            //{
            //    for (int ri = 0; ri < exprsByDepth[di].Count; ri++)
            //    {
            //        Node expression = exprsByDepth[di][ri];
            //        expression.Generate(ctx);
            //    }
            //}

            statement.Generate(ctx);
        }

        return $"define i32 @main() {{\nentry:\n{FormatLLVM(ctx.b.ToString())}\nret i32 %tmp_5\n}}";
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
