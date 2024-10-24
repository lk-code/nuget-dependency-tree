using DotNetFileParser.Contracts;

namespace DotNetFileParser.Parser;

public class ParserBase
{
    protected string? FileName = null;
    protected string? Content = null;
    protected bool IsLoaded => FileName != null && Content != null;
}