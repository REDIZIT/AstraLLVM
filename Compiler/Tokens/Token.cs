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