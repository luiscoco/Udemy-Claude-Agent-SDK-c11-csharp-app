// Optional one-time provisioning. Reuses IDs already present in .env.
using System.Text.RegularExpressions;
using Anthropic;
using Anthropic.Models.Beta.Agents;
using Anthropic.Models.Beta.Environments;
using LessonC11.Configuration;

var envPath = Path.Combine(AppRoot.Find(), ".env");
DotEnv.Load(envPath);
if (!DotEnv.IsConfigured(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")))
    throw new InvalidOperationException("Set your own ANTHROPIC_API_KEY in .env first.");

var client = new AnthropicClient { MaxRetries = 0 };
var contents = File.Exists(envPath) ? File.ReadAllText(envPath) : "";

void Save(string key, string value)
{
    var pattern = new Regex(@"^\s*" + key + "=.*$", RegexOptions.Multiline);
    contents = pattern.IsMatch(contents) ? pattern.Replace(contents, key + "=" + value) : contents.TrimEnd() + "\n" + key + "=" + value + "\n";
    File.WriteAllText(envPath, contents);
}

if (!DotEnv.IsConfigured(Environment.GetEnvironmentVariable("CLAUDE_MANAGED_ENVIRONMENT_ID")))
{
    var environment = await client.Beta.Environments.Create(new EnvironmentCreateParams
    {
        Name = "C11 student environment",
        Config = new BetaCloudConfigParams { Networking = new BetaLimitedNetworkParams() },
    });
    // Save immediately so a later agent failure does not lose the environment ID.
    Save("CLAUDE_MANAGED_ENVIRONMENT_ID", environment.ID);
}

if (!DotEnv.IsConfigured(Environment.GetEnvironmentVariable("CLAUDE_MANAGED_AGENT_ID")))
{
    var model = Environment.GetEnvironmentVariable("CLAUDE_MODEL") is { Length: > 0 } configured ? configured : "claude-opus-5-5";
    var agent = await client.Beta.Agents.Create(new AgentCreateParams
    {
        Name = "C11 text-only student agent",
        // CLAUDE_MODEL may name any model ID, so pass it as a config object rather than a known-model enum.
        Model = new BetaManagedAgentsModelConfigParams { ID = model },
        System = "Answer the student's question clearly and briefly.",
        Tools = [],
        McpServers = [],
        Skills = [],
    });
    Save("CLAUDE_MANAGED_AGENT_ID", agent.ID);
}

Console.WriteLine("Prerequisite IDs are saved in your local .env. Existing configured IDs were reused.");
