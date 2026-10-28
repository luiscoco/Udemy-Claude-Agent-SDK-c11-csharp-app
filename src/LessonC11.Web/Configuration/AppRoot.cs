namespace LessonC11.Configuration;

/// <summary>
/// The lesson folder that holds LessonC11.slnx, .env and .lab-data, wherever `dotnet run` was started from.
/// </summary>
public static class AppRoot
{
    public static string Find()
    {
        for (var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "LessonC11.slnx"))) return dir.FullName;
        return Directory.GetCurrentDirectory();
    }
}
