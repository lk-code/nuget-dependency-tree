using DotNetFileParser.Contracts;

namespace DotNetFileParser.Parser.Slnx.Contracts;

public interface ISolution : IProjectDescriptor
{
    IEnumerable<IProject> Projects
    {
        get;
    }
}