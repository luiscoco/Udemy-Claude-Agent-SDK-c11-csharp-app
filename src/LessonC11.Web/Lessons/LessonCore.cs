using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace LessonC11.Web.Lessons;

[JsonConverter(typeof(JsonStringEnumConverter<Outcome>))]
public enum Outcome
{
    [JsonStringEnumMemberName("passed")] Passed,
    [JsonStringEnumMemberName("rejected")] Rejected,
    [JsonStringEnumMemberName("paused")] Paused,
    [JsonStringEnumMemberName("conflict")] Conflict,
    [JsonStringEnumMemberName("incomplete")] Incomplete,
    [JsonStringEnumMemberName("unconfirmed")] Unconfirmed,
}

public sealed record LessonResult(
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("outcome")] Outcome Outcome,
    [property: JsonPropertyName("summary")] string Summary,
    [property: JsonPropertyName("evidence")] JsonObject Evidence,
    [property: JsonPropertyName("steps")] IReadOnlyList<string> Steps);

public sealed record Scenario(string Id, string Label, JsonObject Input, Outcome Expected, JsonObject? Checks = null);

public interface ILesson
{
    int Number { get; }
    string Title { get; }
    string Description { get; }
    IReadOnlyList<string> Concepts { get; }
    string Scope { get; }
    IReadOnlyList<Scenario> Cases { get; }
    Task<LessonResult> RunAsync(JsonObject input);
}

/// <summary>Input validation and reporting helpers shared by lesson implementations.</summary>
public static class LessonCore
{
    public static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public static string Text(JsonObject input, string key, string fallback = "")
    {
        var node = input[key];
        if (node is null) return fallback;
        if (node.GetValueKind() != JsonValueKind.String || node.GetValue<string>() is not { Length: <= 20000 } value)
            throw new ArgumentException($"{key} must be bounded text");
        return value;
    }

    public static double Number(JsonObject input, string key, double fallback = 0)
    {
        var node = input[key];
        if (node is null) return fallback;
        if (node.GetValueKind() != JsonValueKind.Number)
            throw new ArgumentException($"{key} must be a finite number");
        var value = node.Deserialize<double>();
        if (!double.IsFinite(value)) throw new ArgumentException($"{key} must be a finite number");
        return value;
    }

    public static bool Flag(JsonObject input, string key, bool fallback = false)
    {
        var node = input[key];
        if (node is null) return fallback;
        return node.GetValueKind() switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw new ArgumentException($"{key} must be true or false"),
        };
    }

    public static JsonArray List(JsonObject input, string key)
    {
        if (input[key] is not JsonArray { Count: <= 200 } value)
            throw new ArgumentException($"{key} must be a list of at most 200 values");
        return value;
    }

    public static JsonObject Object(JsonNode? value) =>
        value as JsonObject ?? throw new ArgumentException("Expected an object");

    public static string Hash(string value) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    public static LessonResult Report(Outcome outcome, string summary, JsonObject evidence, params string[] steps) =>
        new("local-experiment", outcome, summary, evidence, steps);

    public static bool IsSafeInteger(double value) => Math.Floor(value) == value && Math.Abs(value) <= 9007199254740991;

    public static bool SafePath(string value)
    {
        if (string.IsNullOrEmpty(value) || value.StartsWith('/') || value.Contains('\\') || value.Contains(':')) return false;
        var parts = value.Split('/');
        return !parts.Any(part => part is ".." or "." or "" or ".git" or ".env" or ".agents" or ".codex");
    }
}
