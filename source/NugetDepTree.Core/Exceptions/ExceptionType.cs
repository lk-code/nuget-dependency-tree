namespace NugetDepTree.Core.Exceptions;

public enum ExceptionType
{
    /// <summary>
    /// the given project is null or empty
    /// </summary>
    PROJECT_CONTENT_EMPTY,
    /// <summary>
    /// the given project is not valid *.csproj Project
    /// </summary>
    PROJECT_CONTENT_INVALID,
}
