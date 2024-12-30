using System.Text;
using System.Text.RegularExpressions;

public abstract class Token
{
    public virtual void Generate(StringBuilder b) { }
}
public class Token_FunctionDefine : Token
{
    public string name;

    public override string ToString()
    {
        return base.ToString() + ": " + name;
    }

    public override void Generate(StringBuilder b)
    {
        b.AppendLine($"define i32 @{name}()");
    }
}
public class Token_FunctionCall : Token
{
    public string name;

    public override string ToString()
    {
        return base.ToString() + ": " + name;
    }

    public override void Generate(StringBuilder b)
    {
        b.AppendLine($"call i32 @{name}()");
    }
}
public class Token_Block : Token
{
    public bool isClosing;

    public override string ToString()
    {
        return base.ToString() + ": " + (isClosing ? "end" : "begin");
    }

    public override void Generate(StringBuilder b)
    {
        b.AppendLine(isClosing ? "}" : "{");
    }
}
public class Token_Return : Token
{
    public override void Generate(StringBuilder b)
    {
        b.Append("ret ");
    }
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
    public static bool IsMatch(string word)
    {
        return word == "(";
    }
}
public class Token_BracketClose : Token
{
    public static bool IsMatch(string word)
    {
        return word == ")";
    }
}
public class Token_Print : Token
{
    public static bool IsMatch(string word)
    {
        return word == "print";
    }
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
    public static bool IsMatch(string word)
    {
        return word == "=";
    }
}
public class Token_BlockOpen : Token
{
    public static bool IsMatch(string word)
    {
        return word == "{";
    }
}
public class Token_BlockClose : Token
{
    public static bool IsMatch(string word)
    {
        return word == "}";
    }
}
public class Token_If : Token
{
    public static bool IsMatch(string word)
    {
        return word == "if";
    }
}
public class Token_Else : Token
{
    public static bool IsMatch(string word)
    {
        return word == "else";
    }
}