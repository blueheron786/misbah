using System.Collections.Generic;
using NUnit.Framework;
using Misbah.Core.Services;

namespace Misbah.Core.Tests
{
    [TestFixture]
    public class MarkdownRendererFunctionalTests
    {
        [Test]
        public void Renders_Highlight()
        {
            var renderer = new MarkdownRenderer();
            var md = "Hello ==World== !!!";
            var html = renderer.Render(md, out _);
            Assert.That(html, Does.Contain("Hello <mark>World</mark> !!!"));
        }

        [Test]
        public void Renders_Bold_Italic()
        {
            var renderer = new MarkdownRenderer();
            var md = "This is ***bold and italic***, **bold**, and *italic*.";
            var html = renderer.Render(md, out _);
            Assert.That(html, Does.Contain("<em><strong>bold and italic</strong></em>"));
            Assert.That(html, Does.Contain("<strong>bold</strong>"));
            Assert.That(html, Does.Contain("<em>italic</em>"));
        }

        [Test]
        public void Renders_Inline_Code()
        {
            var renderer = new MarkdownRenderer();
            var md = "This is `inline code`.";
            var html = renderer.Render(md, out _);
            Assert.That(html, Does.Contain("<code class='misbah-code'>inline code</code>"));
        }

        [Test]
        public void Renders_Code_Block()
        {
            var renderer = new MarkdownRenderer();
            var md = "```\ncode block\n```";
            var html = renderer.Render(md, out _);
            Assert.That(html, Does.Contain("<pre class='misbah-code'><code class='misbah-code'>"));
            Assert.That(html, Does.Contain("code block"));
        }

        [Test]
        public void Renders_Task_List()
        {
            var renderer = new MarkdownRenderer();
            var md = "- [ ] Task one\n- [x] Task two";
            var html = renderer.Render(md, out var lines);
            Assert.That(html, Does.Contain("input type='checkbox' class='md-task' data-line='0'"));
            Assert.That(html, Does.Contain("input type='checkbox' class='md-task' data-line='1' checked"));
            Assert.That(lines, Is.EquivalentTo(new List<int> { 0, 1 }));
        }

        [Test]
        public void Renders_Normal_List()
        {
            var renderer = new MarkdownRenderer();
            var md = "- item one\n- item two";
            var html = renderer.Render(md, out _);
            Assert.That(html, Does.Contain("<ul>"));
            Assert.That(html, Does.Contain("<li>item one</li>"));
            Assert.That(html, Does.Contain("<li>item two</li>"));
        }

        [Test]
        public void Renders_Wiki_Links()
        {
            var renderer = new MarkdownRenderer();
            var md = "See [[My Note]] for details.";
            var html = renderer.Render(md, out _);
            // Assert that the wiki link renders as an <a> tag with correct onclick and text
            Assert.That(
                html.Replace("\r", "").Replace("\n", ""),
                Does.Contain("<a href=\"#\" onclick=\"window.dispatchEvent(new CustomEvent('misbah-nav', { detail: { title: 'My Note' } }));return false;\">My Note</a>")
            );
        }

        [Test]
        public void Renders_External_Links_With_Emoji()
        {
            var renderer = new MarkdownRenderer();
            var md = "[Google](https://google.com)";
            var html = renderer.Render(md, out _);
            Assert.That(html, Does.Contain("<i class='fa fa-external-link-alt'"));
        }

        [Test]
        public void Wiki_Link_To_Existing_Page_Is_Not_Missing()
        {
            var renderer = new MarkdownRenderer();
            renderer.SetExistingPages(new[] { "My Note" });
            var md = "See [[My Note]] for details.";
            var html = renderer.Render(md, out _);
            // Assert that the wiki link renders as a non-missing <a> tag
            Assert.That(
                html.Replace("\r", "").Replace("\n", ""),
                Does.Contain("<a href=\"#\" onclick=\"window.dispatchEvent(new CustomEvent('misbah-nav', { detail: { title: 'My Note' } }));return false;\">My Note</a>")
            );
            Assert.That(html, Does.Not.Contain("class='missing-link'"));
        }

        [Test]
        public void Collapses_Empty_Lines()
        {
            var renderer = new MarkdownRenderer();
            var md = "Line1\n\n\nLine2";
            var html = renderer.Render(md, out _);
            Assert.That(html.Replace("\n", ""), Does.Contain("Line1<br>Line2"));
        }
    }
}
