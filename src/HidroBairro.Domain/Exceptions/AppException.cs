namespace HidroBairro.Domain.Exceptions;

public abstract class AppException : Exception
{
    public int StatusCode { get; }

    public string Title { get; }

    protected AppException(string title, string message, int statusCode)
        : base(message)
    {
        Title = title;
        StatusCode = statusCode;
    }
}
