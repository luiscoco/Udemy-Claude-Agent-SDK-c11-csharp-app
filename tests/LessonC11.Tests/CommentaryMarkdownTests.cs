using LessonC11.Web.Lessons;

namespace LessonC11.Tests;

public class CommentaryMarkdownTests
{
    [Fact]
    public void FormatsTheReportedCommentary()
    {
        var html = CommentaryMarkdown.ToHtml("## Experiment Summary\n\nThe **budget** was **rejected**.\n\nThe deadline is valid.");

        Assert.Contains("<h2>Experiment Summary</h2>", html);
        Assert.Contains("<strong>budget</strong>", html);
        Assert.Contains("<strong>rejected</strong>", html);
        Assert.Contains("<p>The deadline is valid.</p>", html);
        Assert.DoesNotContain("##", html);
        Assert.DoesNotContain("**", html);
    }

    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("<img src=x onerror=alert(1)>")]
    [InlineData("[click](javascript:alert%281%29)")]
    [InlineData("![image](https://example.com/tracking.png)")]
    [InlineData("<https://example.com>")]
    public void ModelContentCannotCreateHtmlScriptsLinksOrImages(string text)
    {
        var html = CommentaryMarkdown.ToHtml(text);

        Assert.DoesNotContain("<script", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<img", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<a ", html, StringComparison.OrdinalIgnoreCase);
    }
}
