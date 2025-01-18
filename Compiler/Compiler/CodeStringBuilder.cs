public class CodeStringBuilder
{
    public List<string> lines = new();
    public int maxSpace = 0;

    public void CommentLine(string comment)
    {
        Line("; -- " + comment);
    }
    public void Line(string line)
    {
        for (int i = 0; i < maxSpace; i++)
        {
            lines.Add(string.Empty);
        }
        maxSpace = 0;

        lines.Add(line);
    }
    public void Space(int space = 1)
    {
        if (lines.Last() == "{" || lines.Last().StartsWith(";")) return;

        maxSpace = Math.Max(maxSpace, space);
    }

    public string BuildString()
    {
        string result = string.Join('\n', lines);
        lines.Clear();
        maxSpace = 0;
        return result;
    }
}
