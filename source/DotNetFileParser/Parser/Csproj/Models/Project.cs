using DotNetFileParser.Parser.Csproj.Contracts;

namespace DotNetFileParser.Parser.Csproj.Models;

public class Project(string fileName) : IProject
{
    public string FileName { get; } = fileName;
    public IEnumerable<IProjectReference> References { get; }
}