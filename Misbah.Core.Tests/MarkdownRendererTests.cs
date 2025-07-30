using System.Collections.Generic;
using NUnit.Framework;
using Misbah.Core.Services;

namespace Misbah.Core.Tests
{
    [TestFixture]
    public class MarkdownRendererTests
    {
        [Test]
        public void Renders_TaskList_Checkboxes_And_Persists()
        {
            var renderer = new MarkdownRenderer();
            var md = "- [ ] Task one\n- [x] Task two\n- [ ] Task three";
            var html = renderer.Render(md, out var lines);
            Assert.That(html, Does.Contain("input type='checkbox' class='md-task' data-line='0'"));
            Assert.That(html, Does.Contain("input type='checkbox' class='md-task' data-line='1' checked"));
            Assert.That(html, Does.Contain("Task one"));
            Assert.That(html, Does.Contain("Task two"));
            Assert.That(html, Does.Contain("Task three"));
            Assert.That(lines, Is.EquivalentTo(new List<int> { 0, 1, 2 }));
        }

        [Test]
        public void Renders_CodeBlocks_And_InlineCode()
        {
            var renderer = new MarkdownRenderer();
            var md = @"```
code block
```
Normal text with `inline` code.";
            var html = renderer.Render(md, out _);
            Assert.That(html, Does.Contain("<pre class='misbah-code'><code class='misbah-code'>"));
            Assert.That(html, Does.Contain("code block"));
            Assert.That(html, Does.Contain("<code class='misbah-code'>inline</code>"));
        }

        [Test]
        public void Renders_ExternalLinks_With_Emoji()
        {
            var renderer = new MarkdownRenderer();
            var md = "[Google](https://google.com)";
            var html = renderer.Render(md, out _);
            html = renderer.AddExternalLinkEmoji(html);
            Assert.That(html, Does.Contain("🌐"));
        }

        [Test]
        public void Renders_WikiLinks()
        {
            var renderer = new MarkdownRenderer();
            var md = "See [[My Note]] for details.";
            var html = renderer.Render(md, out _);
            html = renderer.ReplaceWikiLinks(html);
            Assert.That(html, Does.Contain("misbah-nav"));
            Assert.That(html, Does.Contain("My Note"));
        }

        [Test]
        public void Collapses_EmptyLines()
        {
            var renderer = new MarkdownRenderer();
            var md = "Line1\n\n\nLine2";
            var html = renderer.Render(md, out _);
            // Only one <br> between lines
            Assert.That(html.Replace("\n", ""), Does.Contain("Line1<br>Line2"));
        }

        [Test]
        public void Renders_Highlighted_Text()
        {
            var renderer = new MarkdownRenderer();
            var md = "This is ==very important== and ==yellow==!";
            var html = renderer.Render(md, out _);
            Assert.That(html, Does.Contain("<span class='md-highlight'>very important</span>"));
            Assert.That(html, Does.Contain("<span class='md-highlight'>yellow</span>"));
        }

        [Test]
        public void TaskList_Uses_Custom_Ul_Class_And_No_Bullets()
        {
            var renderer = new MarkdownRenderer();
            var md = "- [ ] Task one\n- [x] Task two";
            var html = renderer.Render(md, out _);
            Assert.That(html, Does.Contain("<ul class='md-task-list'>"));
            Assert.That(html, Does.Not.Contain("<ul>\n<li>")); // Should not use default ul/li
        }

        [Test]
        public void Renders_All_Markdown_Features_Together()
        {
            var renderer = new MarkdownRenderer();
            var md = """
- one
- two
- three

A list:

- [ ] one
- [ ] two
- [ ] three

Please ***bold and italicize*** this bad boi. And ==highlight== this bad boi.

Links to [[Stats]] and [YouTube](https://youtube.com) are fair game.
""";
            var html = renderer.RenderFull(md, out var lines);
            // Normal list: all items present
            Assert.That(System.Text.RegularExpressions.Regex.Matches(html, "<li>one</li>").Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(System.Text.RegularExpressions.Regex.Matches(html, "<li>two</li>").Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(System.Text.RegularExpressions.Regex.Matches(html, "<li>three</li>").Count, Is.GreaterThanOrEqualTo(1));
            // Task list: checkboxes present and correct count
            int checkboxCount = System.Text.RegularExpressions.Regex.Matches(html, "<input type='checkbox' class='md-task'").Count;
            Assert.That(checkboxCount, Is.EqualTo(3));
            Assert.That(html, Does.Contain("<ul class='md-task-list'>"));
            // Bold+italic
            Assert.That(html, Does.Contain("<b><i>bold and italicize</i></b>"));
            // Highlight
            Assert.That(html, Does.Contain("<span class='md-highlight'>highlight</span>"));
            // Wiki link
            Assert.That(html, Does.Contain("misbah-nav"));
            Assert.That(html, Does.Contain("Stats"));
            // External link with emoji
            Assert.That(html, Does.Contain("youtube.com"));
            Assert.That(html, Does.Contain("🌐"));
        }
    }
}
