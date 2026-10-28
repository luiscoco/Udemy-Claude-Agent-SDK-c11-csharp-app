using Markdig;
using Markdig.Parsers.Inlines;

namespace LessonC11.Web.Lessons;

/// <summary>Formats model commentary without accepting HTML or active links/images.</summary>
public static class CommentaryMarkdown
{
    private static readonly MarkdownPipeline Pipeline = CreatePipeline();

    public static string ToHtml(string text) => Markdown.ToHtml(text, Pipeline);

    private static MarkdownPipeline CreatePipeline()
    {
        var builder = new MarkdownPipelineBuilder().DisableHtml();
        // Commentary needs text formatting; keep any supplied URLs as inert text.
        foreach (var parser in builder.InlineParsers
            .Where(parser => parser is LinkInlineParser or AutolinkInlineParser).ToArray())
            builder.InlineParsers.Remove(parser);
        return builder.Build();
    }
}
