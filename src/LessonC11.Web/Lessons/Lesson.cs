using System.Text.Json.Nodes;
using static LessonC11.Web.Lessons.LessonCore;

namespace LessonC11.Web.Lessons;

// A focused local experiment derived from this lesson's reviewed presentation.
public sealed class Lesson : ILesson
{
    public int Number => 11;
    public string Title => "Session Options, Overrides and Deadlines";
    public string Description => "An agent defines reusable behavior, but an application may need to vary one session. You might test another model, compare behavior without the base system prompt, limit list cost, or stop waiting after a web-request deadline.";
    public IReadOnlyList<string> Concepts => ["Full-field override replacement", "Positive budgets", "Local deadlines differ from remote state"];
    public string Scope => "Resolves a local configuration and deadline plan; it does not update a provider budget. Each run starts from a fresh local seed. The run journal stores results, not production workflow state.";
    public IReadOnlyList<Scenario> Cases =>
    [
        new("0", "Resolve a text-only override", new() { ["budgetCents"] = 50, ["deadlineMs"] = 3000, ["clearTools"] = true }, Outcome.Passed, new() { ["toolCount"] = 0 }),
        new("1", "Reject an invalid budget", new() { ["budgetCents"] = 0, ["deadlineMs"] = 3000, ["clearTools"] = true }, Outcome.Rejected, new()),
    ];

    public Task<LessonResult> RunAsync(JsonObject input)
    {
        double budget = Number(input, "budgetCents"), deadline = Number(input, "deadlineMs");
        var validationErrors = new JsonObject();
        if (!IsSafeInteger(budget) || budget < 1)
            validationErrors["budgetCents"] = "Budget must be a positive safe integer in cents.";
        if (deadline < 1)
            validationErrors["deadlineMs"] = "Deadline must be at least 1 millisecond.";
        if (validationErrors.Count > 0)
            return Task.FromResult(Report(Outcome.Rejected,
                string.Join(" ", validationErrors.Select(error => error.Value!.GetValue<string>())),
                new()
                {
                    ["budgetCents"] = budget,
                    ["localDeadlineMs"] = deadline,
                    ["validationErrors"] = validationErrors,
                }, "Validate configuration before session creation"));

        string[] tools = Flag(input, "clearTools") ? [] : ["read", "glob"];

        return Task.FromResult(Report(Outcome.Passed, "Resolved the selected session configuration", new()
        {
            ["toolCount"] = tools.Length,
            ["effective"] = new JsonObject
            {
                ["model"] = "base-model",
                ["system"] = null,
                ["tools"] = new JsonArray([.. tools.Select(tool => JsonValue.Create(tool))]),
                ["mcp_servers"] = new JsonArray(),
                ["skills"] = new JsonArray(),
            },
            ["budgetCents"] = budget,
            ["localDeadlineMs"] = deadline,
            ["remoteStopOnDeadline"] = "not established",
        }, "Inherit omitted model", "Clear system with null", "Replace the entire tools list", "Keep local timeout separate from remote interruption"));
    }
}
