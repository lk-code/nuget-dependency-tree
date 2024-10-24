using DotNetFileParser.Contracts;
using DotNetFileParser.Contracts.FluentDescriptors;

namespace DotNetFileParser.FluentDescriptors;

public class ParserSelector(IParser parser) : IParserSelector
{
    public IParser Parser { get; } = parser;
    
    public IParseFrom Parse()
    {
        return new ParseFrom(this);
    }
}