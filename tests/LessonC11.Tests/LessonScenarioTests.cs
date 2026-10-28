using System.Text.Json.Nodes;
using LessonC11.Web.Lessons;

namespace LessonC11.Tests;

public class LessonScenarioTests
{
    private static readonly Lesson Lesson = new();

    public static TheoryData<string> Scenarios => [.. Lesson.Cases.Select(scenario => scenario.Id)];

    [Theory]
    [MemberData(nameof(Scenarios))]
    public async Task ScenarioProducesExpectedOutcomeAndEvidence(string id)
    {
        var scenario = Lesson.Cases.Single(candidate => candidate.Id == id);
        var result = await Lesson.RunAsync(scenario.Input.DeepClone().AsObject());

        Assert.Equal(scenario.Expected, result.Outcome);
        foreach (var (key, expected) in scenario.Checks ?? [])
            Assert.True(JsonNode.DeepEquals(expected, result.Evidence[key]), $"{key}: expected {expected?.ToJsonString()}, got {result.Evidence[key]?.ToJsonString()}");
    }

    [Theory]
    [InlineData(1.5)]
    [InlineData(-3)]
    public async Task NonPositiveOrFractionalBudgetIsRejected(double budget)
    {
        var result = await Lesson.RunAsync(new JsonObject { ["budgetCents"] = budget, ["deadlineMs"] = 3000, ["clearTools"] = true });
        Assert.Equal(Outcome.Rejected, result.Outcome);
    }

    [Fact]
    public async Task KeepingToolsReplacesTheWholeList()
    {
        var result = await Lesson.RunAsync(new JsonObject { ["budgetCents"] = 50, ["deadlineMs"] = 3000, ["clearTools"] = false });
        Assert.True(JsonNode.DeepEquals(new JsonArray("read", "glob"), result.Evidence["effective"]!["tools"]));
        Assert.Equal("not established", result.Evidence["remoteStopOnDeadline"]!.GetValue<string>());
    }

    [Fact]
    public async Task ZeroBudgetDoesNotInvalidatePositiveDeadline()
    {
        var result = await Lesson.RunAsync(new JsonObject { ["budgetCents"] = 0, ["deadlineMs"] = 3000 });

        Assert.Equal(Outcome.Rejected, result.Outcome);
        Assert.Equal("Budget must be a positive safe integer in cents.", result.Summary);
        Assert.Equal(0, result.Evidence["budgetCents"]!.GetValue<double>());
        Assert.Equal(3000, result.Evidence["localDeadlineMs"]!.GetValue<double>());
        var errors = result.Evidence["validationErrors"]!.AsObject();
        Assert.True(errors.ContainsKey("budgetCents"));
        Assert.False(errors.ContainsKey("deadlineMs"));
    }

    [Theory]
    [InlineData(50, 0, false, true)]
    [InlineData(0, 0, true, true)]
    [InlineData(1.5, 3000, true, false)]
    public async Task RejectionIdentifiesOnlyInvalidFields(double budget, double deadline, bool invalidBudget, bool invalidDeadline)
    {
        var result = await Lesson.RunAsync(new JsonObject { ["budgetCents"] = budget, ["deadlineMs"] = deadline });

        Assert.Equal(Outcome.Rejected, result.Outcome);
        var errors = result.Evidence["validationErrors"]!.AsObject();
        Assert.Equal(invalidBudget, errors.ContainsKey("budgetCents"));
        Assert.Equal(invalidDeadline, errors.ContainsKey("deadlineMs"));
    }

    [Fact]
    public async Task WrongInputTypeIsReported()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Lesson.RunAsync(new JsonObject { ["budgetCents"] = "50", ["deadlineMs"] = 3000 }));
    }
}
