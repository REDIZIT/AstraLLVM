public class BadAstraCode : Exception
{
    public BadAstraCode(string message) : base(message)
    {
    }
}

public class NotSupportedAstraFeature(string message) : BadAstraCode(message)
{
}