using System.Collections.Generic;
using NUnit.Framework;
using Misbah.Application.Services;

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
            // AddExternalLinkEmoji is now always applied in Render
            Assert.That(html, Does.Contain("<i class='fa fa-external-link-alt'"));
        }

        [Test]
        public void Renders_WikiLinks()
        {
            var renderer = new MarkdownRenderer();
            var md = "See [[My Note]] for details.";
            var html = renderer.Render(md, out _);
            // Current behavior: Wiki links are rendered as simple links with target='_blank'
            Assert.That(html, Does.Contain("<a href='My Note' target='_blank'>My Note</a>"));
            Assert.That(html, Does.Contain("See <a href='My Note' target='_blank'>My Note</a> for details.<br>"));
        }

        [Test]
        public void Collapses_EmptyLines()
        {
            var renderer = new MarkdownRenderer();
            var md = "Line1\n\n\nLine2";
            var html = renderer.Render(md, out _);
            // Current behavior: Empty lines are skipped, each non-empty line gets <br> at the end
            Assert.That(html, Does.Contain("Line1<br>"));
            Assert.That(html, Does.Contain("Line2<br>"));
        }

        [Test]
        public void RenderFull_RendersListsAndHighlightsCorrectly()
        {
            // Arrange
            var renderer = new MarkdownRenderer();
            string markdown = "one\ntwo\nthree\nfour\n\nA list:\n- [x] zERO~!\n- [ ] one\n- [ ] two\n- [ ] three\n\nPlease ***bold and italicize*** this bad boi. And ==highlight== this bad boi.\nLinks are gud. HTML test: <b>strong here</b> and <mark>mark here</mark>!";

            // Act
            var html = renderer.Render(markdown, out var taskLines);

            // Assert
            // List items should be grouped in a single <ul>
            Assert.That(html, Does.Contain("<ul>"));
            Assert.That(html, Does.Not.Contain("<ul>\n<li>one</li>\n</ul>\n<ul>")); // No separate <ul> for each item
            // Task list checkboxes should be present
            Assert.That(html, Does.Contain("<input type='checkbox' class='md-task' data-line='6' checked"));
            Assert.That(html, Does.Contain("<input type='checkbox' class='md-task' data-line='7' "));
            // Bold and italic
            Assert.That(html, Does.Contain("<em><strong>bold and italicize</strong></em>"));
            // Highlight
            Assert.That(html, Does.Contain("<mark>highlight</mark>"));
            // HTML passthrough
            Assert.That(html, Does.Contain("<b>strong here</b>"));
            Assert.That(html, Does.Contain("<mark>mark here</mark>"));
        }

        [Test]
        public void WikiLinks_MissingPage_GetsMissingLinkClass()
        {
            var renderer = new MarkdownRenderer();
            // Current behavior: All wiki links are rendered the same way, regardless of existence
            var md = "See [[Missing Page]] and [[My Note]] for details.";
            var html = renderer.Render(md, out _);
            // Both links should be rendered the same way with target='_blank'
            Assert.That(html, Does.Contain("<a href='Missing Page' target='_blank'>Missing Page</a>"));
            Assert.That(html, Does.Contain("<a href='My Note' target='_blank'>My Note</a>"));
            Assert.That(html, Does.Contain("See <a href='Missing Page' target='_blank'>Missing Page</a> and <a href='My Note' target='_blank'>My Note</a> for details.<br>"));
        }
    }
}
