using System.Text.RegularExpressions;

public abstract class Token
{
}
public class Token_Constant : Token
{
    public string value;

    public override string ToString()
    {
        return base.ToString() + ": " + value;
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
    public string type;

    public static bool TryMatch(string word, out Token_Type token)
    {
        if (IsMatch(word))
        {
            token = new Token_Type()
            {
                type = word
            };
            return true;
        }
        token = null;
        return false;
    }
    public static bool IsMatch(string word)
    {
        if (word.StartsWith("i") && int.TryParse(word[1..], out int bits))
        {
            if (bits <= 0) throw new Exception("Int type can not has zero or less bits");
            return true;
        }
        return false;
    }
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
public class Token_Colon : Token
{
}
public class Token_Visibility : Token
{
    public bool isPublic;

    public static bool TryMatch(string word, out Token_Visibility token)
    {
        if (IsMatch(word))
        {
            token = new Token_Visibility()
            {
                isPublic = word == "public"
            };
            return true;
        }
        token = null;
        return false;
    }
    public static bool IsMatch(string word)
    {
        return word == "public" || word == "private";
    }
}