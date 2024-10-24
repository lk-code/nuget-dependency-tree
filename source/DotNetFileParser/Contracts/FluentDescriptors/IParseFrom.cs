namespace DotNetFileParser.Contracts.FluentDescriptors;

public interface IParseFrom
{
    Task<IProjectDescriptor> FromContentAsync(string fileName, string content);
    Task<IProjectDescriptor> FromFileAsync(string filePath);
}