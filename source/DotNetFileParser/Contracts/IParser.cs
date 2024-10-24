using DotNetFileParser.Contracts.FluentDescriptors;

namespace DotNetFileParser.Contracts;

public interface IParser
{
    void Load(string fileName, string content);
    Task<IProjectDescriptor> ParseAsync();
}
