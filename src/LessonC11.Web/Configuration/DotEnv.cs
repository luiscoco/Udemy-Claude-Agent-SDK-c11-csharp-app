namespace LessonC11.Configuration;

/// <summary>
/// Minimal .env loader, equivalent to Node's process.loadEnvFile: KEY=VALUE lines,
/// # comments, optional quotes. Variables already set in the process environment win.
/// </summary>
public static class DotEnv
{
    public static void Load(string path)
    {
        if (!File.Exists(path)) return;
        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            if (line.StartsWith("export ")) line = line[7..].TrimStart();
            var separator = line.IndexOf('=');
            if (separator <= 0) continue;
            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim();
            if (value.Length >= 2 && (value[0] == '"' || value[0] == '\'') && value[^1] == value[0]) value = value[1..^1];
            if (Environment.GetEnvironmentVariable(key) is null) Environment.SetEnvironmentVariable(key, value);
        }
    }

    /// <summary>True when a value is present and is not a "your_..." placeholder from .env.example.</summary>
    public static bool IsConfigured(string? value) => !string.IsNullOrWhiteSpace(value) && !value.StartsWith("your_");
}
