namespace DotNetFileParser.Contracts.FluentDescriptors;

public interface IParserSelector
{
    IParser Parser { get; }
    IParseFrom Parse();
}