namespace SkillHub.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class BusinessRuleException : Exception
{
    public int StatusCode { get; }

    public BusinessRuleException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}
