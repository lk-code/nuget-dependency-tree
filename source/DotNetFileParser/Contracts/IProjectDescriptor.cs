namespace DotNetFileParser.Contracts;

public interface IProjectDescriptor
{
    string FileName
    {
        get;
    }

    // IEnumerable<GlobalSection> Global
    // {
    //     get;
    // }
    //
    // IEnumerable<Project> Projects
    // {
    //     get;
    // }
}