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

        public Dictionary<string, TypeInfo> typeByVariableName = new();

        public Module module;

        public string NextStackUnnamedVariableName(TypeInfo type)
        {
            string varName = $"%local_{localVariablesCount}";
            localVariablesCount++;
            stackVariables.Add(varName);
            typeByVariableName.Add(varName, type);
            return varName;
        }
        public string NextTempVariableName(TypeInfo type)
        {
            string varName = $"%tmp_{tempVariablesCount}";
            tempVariablesCount++;
            tempVariables.Add(varName);
            typeByVariableName.Add(varName, type);
            return varName;
        }
        public string RegisterStackVariable(string name, TypeInfo type)
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

        public TypeInfo GetVariableType(string variableName)
        {
            return typeByVariableName[variableName];
        }
    }

    public static string Generate(List<Node> statements, Module module)
    {
        Context ctx = new()
        {
            module = module
        };

        ctx.b.AppendLine($";");
        ctx.b.AppendLine($"; Structs");
        ctx.b.AppendLine($";");
        foreach (ClassTypeInfo info in module.classInfoByName.Values)
        {
            ctx.b.AppendLine($"%{info.name} = type {{ i32 }}");
        }

        ctx.b.AppendLine();
        ctx.b.AppendLine();
        ctx.b.AppendLine(";");
        ctx.b.AppendLine("; Methods");
        ctx.b.AppendLine(";");

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
