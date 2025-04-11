public abstract class Token
{
    public int line, endLine;
    public int begin, end;
    public int linedBegin;
    public char[] chars;
}
public class Token_Identifier : Token
{
    public string name;
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

public class Token_Class : Token
{
}
public class Token_New : Token
{
}
public class Token_Dot : Token
{
}
public class Token_SquareBracketOpen : Token
{
}
public class Token_SquareBracketClose : Token
{
}

public class Token_Static() : Token
{
}
public class Token_Abstract() : Token
{
}

public class Token_Try() : Token
{
}
public class Token_Catch() : Token
{
}
public class Token_Throw() : Token
{
}

public class Token_As() : Token
{
}

public abstract class Token_Operator : Token
{
    public abstract MathOperator Operator { get; }
}
public class Token_Plus : Token {}
public class Token_Minus : Token {}
public class Token_Star : Token
{
}
public class Token_Slash : Token
{
}
public class Token_LogicalNot : Token
{
}

public class Token_BracketOpen : Token
{
}

public class Token_BracketClose : Token
{
}

public class Token_BlockOpen : Token
{
}

public class Token_BlockClose : Token
{
}

public class Token_CastTo : Token {}
public class Token_Assign : Token {}
public class Token_Equality : Token {}
// public class Token_Comprassion : Token {}
public class Token_Less : Token {}
public class Token_LessOrEqual : Token {}
public class Token_Greater : Token {}
public class Token_GreaterOrEqual : Token {}

public class Token_Space : Token {}
public class Token_EOF : Token {}
public class Token_Bad : Token {}
public class Token_Comment : Token {}
public class Token_AssignByPointer : Token {}

public class Token_Char(char character) : Token
{
    public char character = character;
}
public class Token_String(string str) : Token
{
    public string str = str;
}
public class Token_Constant(string word) : Token
{
    public string word = word;
}




public enum MathOperator : byte
{
    Add,
    Sub,
    Mul,
    Div,
    Less,
    LessOrEqual,
    Equal,
    Greater,
    GreaterOrEqual
}