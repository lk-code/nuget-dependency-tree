using DotNetFileParser.Contracts;
using DotNetFileParser.Parser.Csproj.Models;

namespace DotNetFileParser.Parser.Csproj;

public class CsprojParser : ParserBase, IParser
{
    public void Load(string fileName, string content)
    {
        FileName = fileName;
        Content = content;
    }

    public async Task<IProjectDescriptor> ParseAsync()
    {
        if (!IsLoaded)
        {
            throw new InvalidOperationException("Parser has no content loaded");
        }

        await Task.CompletedTask;
        
        return new Project(this.FileName!);
    }
}