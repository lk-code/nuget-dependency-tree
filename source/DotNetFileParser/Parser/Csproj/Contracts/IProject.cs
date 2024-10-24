using DotNetFileParser.Contracts;

namespace DotNetFileParser.Parser.Csproj.Contracts;

public interface IProject : IProjectDescriptor
{
    IEnumerable<IProjectReference> References
    {
        get;
    }
}