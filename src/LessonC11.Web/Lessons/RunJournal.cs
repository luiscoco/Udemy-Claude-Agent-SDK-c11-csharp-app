using System.Text.Json;
using System.Text.Json.Nodes;

namespace LessonC11.Web.Lessons;

/// <summary>Appends local experiment results to .lab-data/runs.jsonl. Not production workflow state.</summary>
public sealed class RunJournal(string root)
{
    // Plain UTF-8 lines, like JSON.stringify: no +-style escaping of timestamps or text.
    private static readonly JsonSerializerOptions JournalJson = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    private readonly string _path = Path.Combine(root, ".lab-data", "runs.jsonl");

    public async Task<string> AppendAsync(int lesson, LessonResult result)
    {
        var id = Guid.NewGuid().ToString();
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var entry = new JsonObject
        {
            ["id"] = id,
            ["at"] = DateTimeOffset.UtcNow.ToString("O"),
            ["lesson"] = lesson,
            ["result"] = JsonSerializer.SerializeToNode(result),
        };
        await File.AppendAllTextAsync(_path, entry.ToJsonString(JournalJson) + "\n");
        return id;
    }
}

/// <summary>
/// One experiment at a time across the whole host, like the Node server's busy flag.
/// </summary>
public sealed class ExperimentGate
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public bool TryEnter() => _semaphore.Wait(0);

    public void Exit() => _semaphore.Release();
}
