public abstract class Expr
{

}

public class Node_Return : Expr
{
    public Expr child;
}



public class Node_Expression : Expr
{
    public Node_Expression left;
    public Token @operator;
    public Node_Expression right;
}
public class Expr_Binary : Expr
{
    public Expr left, right;
    public Token @operator;
}
public class Expr_Unray : Expr
{
    public Expr right;
    public Token @operator;
}
public class Expr_Literal : Expr
{
    // TODO
    public Token value;
}
public class Expr_Grouping : Expr
{
    public Expr expression;
}