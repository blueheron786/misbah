using System.Collections.Generic;
using NUnit.Framework;
using Misbah.Application.Services;

namespace Misbah.Core.Tests
{
    [TestFixture]
    public class MarkdownRendererEdgeCasesTests
    {
        private MarkdownRenderer _renderer;

        [SetUp]
        public void SetUp()
        {
            _renderer = new MarkdownRenderer();
        }

        [Test]
        public void Renders_Empty_Input()
        {
            // Act
            var html = _renderer.Render(string.Empty, out var taskLineNumbers);

            // Assert - Current implementation adds a <br> for empty input
            Assert.That(html, Is.EqualTo("<br>"));
            Assert.That(taskLineNumbers, Is.Empty);
        }

        [Test]
        public void Renders_Null_Input()
        {
            // Act
            var html = _renderer.Render(null, out var taskLineNumbers);

            // Assert - Current implementation adds a <br> for null input
            Assert.That(html, Is.EqualTo("<br>"));
            Assert.That(taskLineNumbers, Is.Empty);
        }

        [Test]
        public void Renders_Only_Whitespace()
        {
            // Arrange
            var md = "   \n  \t  \n    ";

            // Act
            var html = _renderer.Render(md, out var taskLineNumbers);

            // Assert
            Assert.That(html, Is.EqualTo("<br>"));
            Assert.That(taskLineNumbers, Is.Empty);
        }

        [Test]
        public void Renders_Multiple_Consecutive_Empty_Lines()
        {
            // Arrange
            var md = "Line 1\n\n\n\nLine 2";

            // Act
            var html = _renderer.Render(md, out _);

            // Assert
            Assert.That(html, Does.Contain("Line 1<br>"));
            Assert.That(html, Does.Contain("Line 2<br>"));
            var lines = html.Split("<br>", StringSplitOptions.RemoveEmptyEntries);
            Assert.That(lines.Length, Is.EqualTo(3)); // 2 lines, empty after last line
        }

        [Test]
        public void Renders_CodeBlock_With_Empty_Lines()
        {
            // Arrange
            var md = "```\nline1\n\nline2\n\n\nline3\n```";

            // Act
            var html = _renderer.Render(md, out _);

            // Assert
            Assert.That(html, Does.Contain("<pre class='misbah-code'><code class='misbah-code'>"));
            Assert.That(html, Does.Contain("line1"));
            Assert.That(html, Does.Contain("line2"));
            Assert.That(html, Does.Contain("line3"));
            Assert.That(html, Does.Contain("</code></pre>"));
        }

        [Test]
        public void Renders_Nested_Markdown()
        {
            // Arrange
            var md = "**Bold with *italic* inside**";

            // Act
            var html = _renderer.Render(md, out _);

            // Assert
            Assert.That(html, Does.Contain("<strong>Bold with <em>italic</em> inside</strong>"));
        }

        [Test]
        public void Renders_Code_With_Backticks()
        {
            // Arrange
            var md = "This is a `code with ` backtick` example";

            // Act
            var html = _renderer.Render(md, out _);

            // Assert - The renderer treats the first backtick as the end of the code span
            Assert.That(html, Does.Contain("<code class='misbah-code'>code with </code> backtick` example<br>"));
        }

        [Test]
        public void Renders_Link_With_Special_Characters()
        {
            // Arrange
            var md = "[Special & < > \" ' Characters](https://example.com/path?param=value&param2=value2)";

            // Act
            var html = _renderer.Render(md, out _);

            // Assert - Current implementation doesn't HTML-encode the URL parameters
            Assert.That(html, Does.Contain("<a href='https://example.com/path?param=value&param2=value2' target='_blank'>"));
            Assert.That(html, Does.Contain("Special & < > \" ' Characters"));
        }

        [Test]
        public void Renders_Link_With_Emoji_And_Special_Characters()
        {
            // Arrange
            var md = "[Test 😊 & < > \" '](https://example.com)";

            // Act
            var html = _renderer.Render(md, out _);

            // Assert - Current implementation adds external link emoji and doesn't encode special chars in link text
            Assert.That(html, Does.Contain("<a href='https://example.com' target='_blank'>Test 😊 & < > \" '</a>"));
            Assert.That(html, Does.Contain("<i class='fa fa-external-link-alt'"));
        }
    }
}
