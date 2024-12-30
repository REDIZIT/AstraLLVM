using System.Text;
using System.Text.RegularExpressions;

public abstract class Token
{
    public virtual void Generate(StringBuilder b) { }
}
public class Token_Constant : Token
{
    public string value;

    public override string ToString()
    {
        return base.ToString() + ": " + value;
    }

    public override void Generate(StringBuilder b)
    {
        b.Append($"i32 {value}\n");
    }
}
public class Token_BracketOpen : Token
{
}
public class Token_BracketClose : Token
{
}
public class Token_Type : Token
{
}
public class Token_Identifier : Token
{
    public string name;

    public static bool IsMatch(string word)
    {
        return Regex.IsMatch(word, "[a-zA-Z0-9_]");
    }
}
public class Token_Assign : Token
{
}
public class Token_BlockOpen : Token
{
}
public class Token_BlockClose : Token
{
}
public class Token_If : Token
{
}
public class Token_Else : Token
{
}
public class Token_While : Token
{
}
public class Token_For : Token
{
}
public class Token_Semicolon : Token_Terminator
{
}
public class Token_Comma : Token
{
}
public class Token_Fn : Token
{
}
public class Token_Return : Token
{
}
public class Token_Terminator : Token
{
}