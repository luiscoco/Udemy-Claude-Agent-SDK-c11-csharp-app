namespace LessonC11.Web.Lessons;

/// <summary>
/// Reads source files embedded at build time (see the csproj), so the pages always show the code that is running.
/// </summary>
public static class SourceFiles
{
    public static string Read(string path)
    {
        using var stream = typeof(SourceFiles).Assembly.GetManifestResourceStream("source/" + path)
            ?? throw new FileNotFoundException($"{path} is not an embedded source file");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
