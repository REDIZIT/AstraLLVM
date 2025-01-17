public class UnexpectedTokenException : Exception
{
    public Token unexpectedToken;

    public override string Message => $"Totally unexpected token '{unexpectedToken.GetType()}'";

    public UnexpectedTokenException(Token unexpectedToken)
    {
        this.unexpectedToken = unexpectedToken;
    }
}