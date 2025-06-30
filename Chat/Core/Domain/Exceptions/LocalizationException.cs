namespace Domain.Exceptions;

public class LocalizationException : Exception
{
    public int StatusCode { get; }

    public LocalizationException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }

    public LocalizationException(string message, Exception innerException, int statusCode = 400) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public string GetError()
    {
        return Message;
    }
} 