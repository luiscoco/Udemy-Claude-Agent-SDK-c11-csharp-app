using LessonC11.Configuration;
using LessonC11.Web.Components;
using LessonC11.Web.Lessons;

var root = AppRoot.Find();
DotEnv.Load(Path.Combine(root, ".env"));

var builder = WebApplication.CreateBuilder(args);
var lesson = new Lesson();
var port = int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var configuredPort) ? configuredPort : 3000 + lesson.Number;
builder.WebHost.UseUrls($"http://127.0.0.1:{port}");
// Serve framework assets such as _framework/blazor.web.js under `dotnet run` in any environment, not only Development.
builder.WebHost.UseStaticWebAssets();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<ILesson>(lesson);
builder.Services.AddSingleton(new RunJournal(root));
builder.Services.AddSingleton<ExperimentGate>();
builder.Services.AddSingleton<LiveCommentary>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Lifetime.ApplicationStarted.Register(() => Console.WriteLine($"Lesson C{lesson.Number:D2} ready: http://127.0.0.1:{port}"));
app.Run();
