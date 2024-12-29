using System.Text;

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
public class Token_Operator : Token
{
    public string @operator;

    public static Dictionary<string, int> predenceByOperator = new()
    {
        { "*", 4 },
        { "/", 4 },
        { "%", 4 },
        { "+", 3 },
        { "-", 3 },
        { ">", 2 },
        { "<", 2 },
        { ">=", 2 },
        { "<=", 2 },
        { "==", 2 },
        { "!=", 2 },
        { "not", 1 },
        { "and", 0 },
        { "or", 0 },
    };

    public override string ToString()
    {
        return base.ToString() + ": " + @operator;
    }

    public static bool IsOperator(string word)
    {
        return predenceByOperator.ContainsKey(word);
    }
}
public class Token_Equality : Token
{
    public static bool IsMatch(string word)
    {
        return word == "==" || word == "!=";
    }
}
public class Token_Comprassion : Token
{
    public static bool IsMatch(string word)
    {
        return word == ">" || word == ">=" || word == "<" || word == "<=";
    }
}
public class Token_Term : Token
{
    public static bool IsMatch(string word)
    {
        return word == "+" || word == "-";
    }
}
public class Token_Factor : Token
{
    public static bool IsMatch(string word)
    {
        return word == "*" || word == "/";
    }
}
public class Token_Unary : Token
{
    public static bool IsMatch(string word)
    {
        return word == "!" || word == "-";
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