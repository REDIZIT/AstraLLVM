public abstract class Token_Operator : Token
{
    public virtual string ResultType => "int";

    public string asmOperatorName;

    public override string ToString()
    {
        return base.ToString() + ": " + asmOperatorName;
    }
}
public class Token_Equality : Token_Operator
{
    public override string ResultType => "bool";

    public static bool TryMatch(string word, out Token_Equality op)
    {
        if (IsMatch(word))
        {
            op = new Token_Equality();

            if (word == "==") op.asmOperatorName = "icmp eq";
            if (word == "!=") op.asmOperatorName = "icmp ne";

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
    public override string ResultType => "bool";

    public static bool TryMatch(string word, out Token_Comprassion op)
    {
        if (IsMatch(word))
        {
            op = new Token_Comprassion();

            if (word == ">") op.asmOperatorName = "icmp sgt";
            if (word == ">=") op.asmOperatorName = "icmp sge";
            if (word == "<") op.asmOperatorName = "icmp slt";
            if (word == "<=") op.asmOperatorName = "icmp sle";

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
public class Token_AddSub : Token_Operator
{
    public static bool TryMatch(string word, out Token_AddSub op)
    {
        if (IsMatch(word))
        {
            op = new Token_AddSub();

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
        return word == "not";
    }
}