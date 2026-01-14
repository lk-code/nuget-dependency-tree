namespace NugetDepTree.Core.Exceptions;

public class ProjectException : Exception
{
    public ExceptionType ExceptionType { get; init; }

    public ProjectException(ExceptionType ExceptionType)
    {
        this.ExceptionType = ExceptionType;
    }

    public ProjectException(ExceptionType ExceptionType, string? message) : base(message)
    {
        this.ExceptionType = ExceptionType;
    }

    public ProjectException(ExceptionType ExceptionType, string? message, Exception? innerException) : base(message, innerException)
    {
        this.ExceptionType = ExceptionType;
    }
}
