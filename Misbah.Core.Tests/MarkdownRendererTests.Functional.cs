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
        public void Renders_Nested_List()
        {
            var renderer = new MarkdownRenderer();
            var md = "- parent\n  - child\n    - grandchild\n- sibling";
            var html = renderer.Render(md, out _);

            Assert.That(html, Does.Contain("<li>parent<ul><li>child<ul><li>grandchild</li></ul></li></ul></li>"));
            Assert.That(html, Does.Contain("</ul></li><li>sibling</li></ul>"));
        }

        [Test]
        public void Renders_Nested_Task_List()
        {
            var renderer = new MarkdownRenderer();
            var md = "- [ ] parent\n  - [x] child";
            var html = renderer.Render(md, out var lines);

            Assert.That(html, Does.Contain("<ul class='task-list'><li><input type='checkbox' class='md-task' data-line='0'"));
            Assert.That(html, Does.Contain("<ul class='task-list'><li><input type='checkbox' class='md-task' data-line='1' checked"));
            Assert.That(lines, Is.EquivalentTo(new List<int> { 0, 1 }));
        }

        [Test]
        public void Renders_Wiki_Links()
        {
            var renderer = new MarkdownRenderer();
            var md = "See [[My Note]] for details.";
            var html = renderer.Render(md, out _);
            // Current behavior: Wiki links are rendered as simple links with target='_blank'
            Assert.That(
                html,
                Does.Contain("<a href='My Note' target='_blank'>My Note</a>")
            );
            Assert.That(
                html,
                Does.Contain("See <a href='My Note' target='_blank'>My Note</a> for details.<br>")
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
            var md = "See [[My Note]] for details.";
            var html = renderer.Render(md, out _);
            // All wiki links are rendered with target='_blank'
            Assert.That(
                html,
                Does.Contain("<a href='My Note' target='_blank'>My Note</a>")
            );
            Assert.That(
                html,
                Does.Contain("See <a href='My Note' target='_blank'>My Note</a> for details.<br>")
            );
        }

        [Test]
        public void Collapses_Empty_Lines()
        {
            var renderer = new MarkdownRenderer();
            var md = "Line1\n\n\nLine2";
            var html = renderer.Render(md, out _);
            // Current behavior: Empty lines are skipped, each non-empty line gets <br> at the end
            Assert.That(html, Does.Contain("Line1<br>"));
            Assert.That(html, Does.Contain("Line2<br>"));
        }
    }
}
