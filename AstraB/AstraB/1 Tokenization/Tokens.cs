public abstract class Token_Operator : Token
{
    public abstract MathOperator Operator { get; }
}
public class Token_Plus : Token, IGroup_PlusMinus
{
}
public class Token_Minus : Token, IGroup_PlusMinus
{
}
public class Token_Star : Token
{
}
public class Token_Slash : Token
{
}
public class Token_LogicalNot : Token
{
}

public interface IGroup_PlusMinus
{
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