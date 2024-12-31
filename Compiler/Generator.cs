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

        public Dictionary<string, string> typeByVariableName = new();

        public string NextStackUnnamedVariableName()
        {
            string varName = $"%local_{localVariablesCount}";
            localVariablesCount++;
            stackVariables.Add(varName);
            return varName;
        }
        public string NextTempVariableName(string type)
        {
            string varName = $"%tmp_{tempVariablesCount}";
            tempVariablesCount++;
            tempVariables.Add(varName);
            typeByVariableName.Add(varName, type);
            return varName;
        }
        public string RegisterStackVariable(string name, string type)
        {
            string varName = "%" + name;
            stackVariables.Add(varName);
            typeByVariableName.Add(varName, type);
            return varName;
        }

        public bool IsPointer(string generatedName)
        {
            return stackVariables.Contains(generatedName);
        }

        public string GetVariableType(string variableName)
        {
            return typeByVariableName[variableName];
        }
    }

    public static string Generate(List<Node> statements)
    {
        Context ctx = new();

        foreach (Node statement in statements)
        {
            statement.Generate(ctx);
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
            
            if (depth > 0 && line.Contains(":") == false)
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
