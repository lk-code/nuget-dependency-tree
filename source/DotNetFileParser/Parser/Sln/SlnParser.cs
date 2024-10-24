using DotNetFileParser.Contracts;
using DotNetFileParser.Parser.Sln.Models;

namespace DotNetFileParser.Parser.Sln;

public class SlnParser : ParserBase, IParser
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
        
        return new Solution(this.FileName!);
    }
}