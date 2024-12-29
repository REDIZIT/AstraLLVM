using System.Text;

public static class Generator
{
    public static string Generate(List<Token> tokens)
    {
        StringBuilder b = new();

        for (int i = 0; i < tokens.Count; i++)
        {
            Token token = tokens[i];

            token.Generate(b);
        }

        return FormatLLVM(b.ToString());
    }

    public static string FormatLLVM(string llvm)
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
