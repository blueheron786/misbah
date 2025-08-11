using NUnit.Framework;
using Misbah.Application.Services;

namespace Misbah.Core.Tests
{
    [TestFixture]
    public class MarkdownRendererTests_LineBreaks
    {
        [Test]
        public void Render_ShouldSupport_WikiLinks_With_DisplayText()
        {
            // Arrange
            var renderer = new MarkdownRenderer();
            var input = "[[CSharp|C#]]";

            // Act
            var html = renderer.Render(input, out _);

            // Assert
            Assert.That(html, Does.Contain("href='CSharp"));
            Assert.That(html, Does.Contain(">C#<"));
            // Should not display the page name if display text is present
            Assert.That(html, Does.Not.Contain(">CSharp<"));
        }
        [Test]
        public void Render_ShouldPreserveLineBreaks_ForSimpleLines()
        {
            // Arrange
            var renderer = new MarkdownRenderer();
            var input = "one\ntwo\nthree";

            // Act
            var html = renderer.Render(input, out _);

            // Assert
            Assert.That(html, Does.Contain("one"));
            Assert.That(html, Does.Contain("two"));
            Assert.That(html, Does.Contain("three"));
            // Should not collapse to a single line
            Assert.That(html.Replace("\n", " "), Does.Not.Contain("one two three"));
            // Should contain <br> or <p> for each line
            Assert.That(html.Contains("<br>") || html.Contains("<br />") || html.Split("<p>").Length > 2, Is.True);
        }
    }
}