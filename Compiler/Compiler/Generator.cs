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
        public Dictionary<string, TypeInfo> pointedTypeByVariableName = new();

        public ResolvedModule module;

        public string NextTempVariableName(TypeInfo type)
        {
            string varName = $"%tmp_{tempVariablesCount}_{type.name}";
            tempVariablesCount++;
            tempVariables.Add(varName);
            typeByVariableName.Add(varName, type);
            return varName;
        }
        public string NextPointerVariableName(TypeInfo pointedType, string name = null)
        {
            string generatedName;
            if (string.IsNullOrWhiteSpace(name))
            {
                generatedName = $"%ptr_{localVariablesCount}_{pointedType.name}";
                localVariablesCount++;
            }
            else
            {
                generatedName = "%" + name;
            }


            stackVariables.Add(generatedName);

            typeByVariableName.Add(generatedName, PrimitiveTypeInfo.PTR);
            pointedTypeByVariableName.Add(generatedName, pointedType);

            return generatedName;
        }

        public bool IsPointer(string generatedName)
        {
            return typeByVariableName[generatedName] == PrimitiveTypeInfo.PTR;
        }

        public TypeInfo GetVariableType(string variableName)
        {
            return typeByVariableName[variableName];
        }
        public TypeInfo GetPointedType(string pointerVariableName)
        {
            return pointedTypeByVariableName[pointerVariableName];
        }
    }

    public static string Generate(List<Node> statements, ResolvedModule module)
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
            string typesStr = string.Join(", ", info.fields.Select(f => f.type.ToString()));
            ctx.b.AppendLine($"%{info.name} = type {{ {typesStr} }}");
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
