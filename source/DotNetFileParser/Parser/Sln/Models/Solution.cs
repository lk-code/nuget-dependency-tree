using DotNetFileParser.Parser.Sln.Contracts;

namespace DotNetFileParser.Parser.Sln.Models;

public class Solution(string fileName) : ISolution
{
    public string FileName { get; } = fileName;
    public IEnumerable<IProject> Projects { get; }
}