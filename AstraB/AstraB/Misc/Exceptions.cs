public class BadAstraCode(string message) : Exception
{
    public string message = message;

    public override string ToString()
    {
        return message;
    }
}

public class NotSupportedAstraFeature(string message) : BadAstraCode(message)
{
}