# C11 · Session Options, Overrides and Deadlines (C# / Blazor)

A .NET 10 Blazor port of [`../lesson-c11-app`](../lesson-c11-app), using the official Anthropic C# SDK ([`Anthropic` 13.3.0](https://www.nuget.org/packages/Anthropic/13.3.0)) instead of `@anthropic-ai/sdk`.

Resolves a local configuration and deadline plan; it does not update a provider budget. Each run starts from a fresh local seed. The run journal stores results, not production workflow state.

## Concepts students will learn

- Full-field override replacement
- Positive budgets
- Local deadlines differ from remote state

## Configure and run

Use the .NET 10 SDK or later. In PowerShell, from this folder:

The local `NuGet.Config` uses nuget.org for packages and vulnerability checks, so restoring this lesson does not require authentication to private feeds configured on your machine.

```powershell
dotnet --version
dotnet restore
Copy-Item .env.example .env
notepad .env
```

No API key is required for the local experiment. Optional live commentary requires your own API key, agent ID and environment ID. It explains the local result; it does not activate the integration discussed in the lesson. If you already configured the Node app, its `.env` works here unchanged.

For the optional live path, set your API key, then run the setup tool if you do not already have resource IDs. It creates the environment before a text-only agent, saves each ID to `.env` immediately, and reuses IDs already configured. It uses `CLAUDE_MODEL` or the source default (`claude-opus-5-5`). An uncertain network failure may have created a resource: inspect your account before retrying.

```powershell
dotnet run --project src/LessonC11.Setup
```

Start the web application:

```powershell
dotnet run --project src/LessonC11.Web
```

Open `http://127.0.0.1:3011` (or the `PORT` from `.env`). The pages are `/guide#learning`, `/guide#run-guide`, `/architecture` and the application lab at `/`. Stop with Ctrl+C.

## Project layout

| Path | Ported from | Purpose |
|---|---|---|
| `src/LessonC11.Web/Lessons/Lesson.cs` | `src/lesson.ts` | The lesson rules and scenarios |
| `src/LessonC11.Web/Lessons/LessonCore.cs` | `src/core.ts` | Input validation and report helpers |
| `src/LessonC11.Web/Lessons/LiveCommentary.cs` | `src/provider.ts` | Text-only Managed Agents session via `client.Beta.Sessions` |
| `src/LessonC11.Web/Lessons/RunJournal.cs` | `src/server.ts` (journal, busy flag) | `.lab-data/runs.jsonl` and the one-at-a-time gate |
| `src/LessonC11.Web/Components/Pages/Home.razor` | `public/index.html` + `src/client.ts` | Interactive lab (Blazor Server) |
| `src/LessonC11.Web/Components/Pages/Guide.razor`, `Architecture.razor` | `public/guide.html`, `public/architecture.html` | Student guide and walkthrough (static SSR) |
| `src/LessonC11.Setup/Program.cs` | `scripts/setup-resources.mts` | One-time environment and agent provisioning |
| `tests/LessonC11.Tests` | `src/lesson.test.ts` | Scenario tests (xUnit) |

### What changed in the port

- **No JSON API.** The lab runs on the Blazor circuit, so `/api/run`, `/api/live`, `/api/lesson` and `/api/source` are gone, and so are the origin and content-type checks they needed. The 64 KB input limit and the host-wide "one experiment at a time" gate are kept.
- **Sources shown on the pages are embedded resources**, so the code students read always matches the running build.
- **Managed Agents in C#.** `agent_with_overrides` becomes `BetaManagedAgentsAgentWithOverridesParams`, and the event stream is `client.Beta.Sessions.Events.StreamStreaming(...)`. The stream is opened before `user.message` is sent, and on failure the host sends `user.interrupt` and reports the stop as unconfirmed, as the Node app did. The idle stop reason is read from `stop_details.type`.
- **Prism** gets a compact C# grammar in `wwwroot/code-highlighting.js`, because the bundled Prism build has none.

## Validation

```powershell
dotnet test
```

Local scenarios and live provider calls are separate: local success is not evidence of a live provider operation.
