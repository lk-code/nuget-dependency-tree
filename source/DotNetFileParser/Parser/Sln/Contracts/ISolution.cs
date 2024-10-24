using DotNetFileParser.Contracts;

namespace DotNetFileParser.Parser.Sln.Contracts;

public interface ISolution : IProjectDescriptor
{
    IEnumerable<IProject> Projects
    {
        get;
    }
}