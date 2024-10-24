using DotNetFileParser.Contracts;
using DotNetFileParser.Contracts.FluentDescriptors;

namespace DotNetFileParser.FluentDescriptors;

public class ParseFrom(IParserSelector parserSelector) : IParseFrom
{
    public async Task<IProjectDescriptor> FromContentAsync(string fileName, string content)
    {
        parserSelector.Parser.Load(fileName, content);
        return await parserSelector.Parser.ParseAsync();
    }

    public async Task<IProjectDescriptor> FromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File {filePath} does not exist");

        string fileName = Path.GetFileName(filePath);
        string content = await File.ReadAllTextAsync(filePath);
        
        parserSelector.Parser.Load(fileName, content);
        return await parserSelector.Parser.ParseAsync();
    }
}