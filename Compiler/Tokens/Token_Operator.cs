public abstract class Token_Operator : Token
{
    public string asmOperatorName;
}
public class Token_Equality : Token_Operator
{
    public static bool TryMatch(string word, out Token_Equality op)
    {
        if (IsMatch(word))
        {
            op = new Token_Equality()
            {
                asmOperatorName = word
            };
            return true;
        }

        op = null;
        return false;
    }
    public static bool IsMatch(string word)
    {
        return word == "==" || word == "!=";
    }
}
public class Token_Comprassion : Token_Operator
{
    public static bool TryMatch(string word, out Token_Comprassion op)
    {
        if (IsMatch(word))
        {
            op = new Token_Comprassion()
            {
                asmOperatorName = word
            };
            return true;
        }

        op = null;
        return false;
    }
    public static bool IsMatch(string word)
    {
        return word == ">" || word == ">=" || word == "<" || word == "<=";
    }
}
public class Token_Term : Token_Operator
{
    public static bool TryMatch(string word, out Token_Term op)
    {
        if (IsMatch(word))
        {
            op = new Token_Term();

            if (word == "+") op.asmOperatorName = "add";
            if (word == "-") op.asmOperatorName = "sub";

            return true;
        }

        op = null;
        return false;
    }
    public static bool IsMatch(string word)
    {
        return word == "+" || word == "-";
    }
}
public class Token_Factor : Token_Operator
{
    public static bool TryMatch(string word, out Token_Factor op)
    {
        if (IsMatch(word))
        {
            op = new Token_Factor();

            if (word == "*") op.asmOperatorName = "mul";
            if (word == "/") op.asmOperatorName = "div";

            return true;
        }

        op = null;
        return false;
    }
    public static bool IsMatch(string word)
    {
        return word == "*" || word == "/";
    }
}
public class Token_Unary : Token_Operator
{
    public static bool TryMatch(string word, out Token_Unary op)
    {
        if (IsMatch(word))
        {
            op = new Token_Unary()
            {
                asmOperatorName = word
            };
            return true;
        }

        op = null;
        return false;
    }
    public static bool IsMatch(string word)
    {
        return word == "!" || word == "-";
    }
}