using DotNetFileParser.Parser.Slnx.Contracts;

namespace DotNetFileParser.Parser.Slnx.Models;

public class Solution(string fileName) : ISolution
{
    public string FileName { get; } = fileName;
    public IEnumerable<IProject> Projects { get; }
}