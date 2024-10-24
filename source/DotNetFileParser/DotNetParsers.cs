using DotNetFileParser.Contracts;
using DotNetFileParser.Contracts.FluentDescriptors;
using DotNetFileParser.FluentDescriptors;
using DotNetFileParser.Parser.Csproj;
using DotNetFileParser.Parser.Sln;
using DotNetFileParser.Parser.Slnx;

namespace DotNetFileParser;

public class DotNetParsers
{
    public static IParserSelector GetForSlnSolution()
    {
        return new ParserSelector(new SlnParser());
    }

    public static IParserSelector GetForSlnxSolution()
    {
        return new ParserSelector(new SlnxParser());
    }

    public static IParserSelector GetForCsprojProject()
    {
        return new ParserSelector(new CsprojParser());
    }
}
