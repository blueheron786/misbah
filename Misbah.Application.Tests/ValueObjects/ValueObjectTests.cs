using NUnit.Framework;
using Misbah.Domain.ValueObjects;
using System;
using System.Linq;

namespace Misbah.Application.Tests.ValueObjects
{
    [TestFixture]
    public class NotePathTests
    {
        [Test]
        public void Constructor_ValidPath_CreatesNotePath()
        {
            // Arrange & Act
            var path = new NotePath(@"C:\docs\note.md");

            // Assert
            Assert.That(path.Value, Is.EqualTo(@"C:\docs\note.md"));
        }

        [Test]
        public void Constructor_EmptyPath_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new NotePath(""));
        }

        [Test]
        public void Constructor_WhitespacePath_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new NotePath("   "));
        }

        [Test]
        public void Constructor_NullPath_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new NotePath(null!));
        }

        [Test]
        public void ImplicitConversion_ToString_ReturnsValue()
        {
            // Arrange
            var path = new NotePath(@"C:\docs\note.md");

            // Act
            string pathString = path;

            // Assert
            Assert.That(pathString, Is.EqualTo(@"C:\docs\note.md"));
        }

        [Test]
        public void ImplicitConversion_FromString_CreatesNotePath()
        {
            // Arrange & Act
            NotePath path = @"C:\docs\note.md";

            // Assert
            Assert.That(path.Value, Is.EqualTo(@"C:\docs\note.md"));
        }

        [Test]
        public void Equals_SamePaths_ReturnsTrue()
        {
            // Arrange
            var path1 = new NotePath(@"C:\docs\note.md");
            var path2 = new NotePath(@"C:\docs\note.md");

            // Act & Assert
            Assert.That(path1, Is.EqualTo(path2));
            Assert.That(path1.GetHashCode(), Is.EqualTo(path2.GetHashCode()));
        }

        [Test]
        public void Equals_DifferentPaths_ReturnsFalse()
        {
            // Arrange
            var path1 = new NotePath(@"C:\docs\note1.md");
            var path2 = new NotePath(@"C:\docs\note2.md");

            // Act & Assert
            Assert.That(path1, Is.Not.EqualTo(path2));
        }

        [Test]
        public void ToString_ReturnsValue()
        {
            // Arrange
            var path = new NotePath(@"C:\docs\note.md");

            // Act
            var result = path.ToString();

            // Assert
            Assert.That(result, Is.EqualTo(@"C:\docs\note.md"));
        }
    }

    [TestFixture]
    public class MarkdownContentTests
    {
        [Test]
        public void Constructor_ValidContent_CreatesMarkdownContent()
        {
            // Arrange & Act
            var content = new MarkdownContent("# Hello World");

            // Assert
            Assert.That(content.RawContent, Is.EqualTo("# Hello World"));
        }

        [Test]
        public void Constructor_EmptyContent_CreatesEmptyMarkdownContent()
        {
            // Arrange & Act
            var content = new MarkdownContent("");

            // Assert
            Assert.That(content.RawContent, Is.EqualTo(""));
            Assert.That(content.IsEmpty, Is.True);
        }

        [Test]
        public void Constructor_NullContent_CreatesEmptyMarkdownContent()
        {
            // Arrange & Act
            var content = new MarkdownContent(null!);

            // Assert
            Assert.That(content.RawContent, Is.EqualTo(""));
            Assert.That(content.IsEmpty, Is.True);
        }

        [Test]
        public void ExtractTags_HashtagTags_ExtractsCorrectly()
        {
            // Arrange
            var content = new MarkdownContent("This has #work and #personal tags");

            // Act
            var tags = content.ExtractTags();

            // Assert
            Assert.That(tags.Count, Is.EqualTo(2));
            Assert.That(tags, Contains.Item("work"));
            Assert.That(tags, Contains.Item("personal"));
        }

        [Test]
        public void ExtractTags_NoTags_ReturnsEmpty()
        {
            // Arrange
            var content = new MarkdownContent("This has no tags");

            // Act
            var tags = content.ExtractTags();

            // Assert
            Assert.That(tags, Is.Empty);
        }

        [Test]
        public void ExtractTags_DuplicateTags_ReturnsUnique()
        {
            // Arrange
            var content = new MarkdownContent("This has #work and #work again");

            // Act
            var tags = content.ExtractTags();

            // Assert
            Assert.That(tags.Count, Is.EqualTo(1));
            Assert.That(tags, Contains.Item("work"));
        }

        [Test]
        public void ExtractWikiLinks_WikiStyleLinks_ExtractsCorrectly()
        {
            // Arrange
            var content = new MarkdownContent("Link to [[Page One]] and [[Page Two]]");

            // Act
            var links = content.ExtractWikiLinks();

            // Assert
            Assert.That(links.Count, Is.EqualTo(2));
            Assert.That(links, Contains.Item("Page One"));
            Assert.That(links, Contains.Item("Page Two"));
        }

        [Test]
        public void ExtractWikiLinks_NoLinks_ReturnsEmpty()
        {
            // Arrange
            var content = new MarkdownContent("No wiki links here");

            // Act
            var links = content.ExtractWikiLinks();

            // Assert
            Assert.That(links, Is.Empty);
        }

        [Test]
        public void ExtractTitle_H1Header_ExtractsTitle()
        {
            // Arrange
            var content = new MarkdownContent("# My Great Title\nSome content here");

            // Act
            var title = content.ExtractTitle();

            // Assert
            Assert.That(title, Is.EqualTo("My Great Title"));
        }

        [Test]
        public void ExtractTitle_NoH1Header_ReturnsEmpty()
        {
            // Arrange
            var content = new MarkdownContent("## Not H1\nSome content here");

            // Act
            var title = content.ExtractTitle();

            // Assert
            Assert.That(title, Is.EqualTo(""));
        }

        [Test]
        public void WordCount_MultipleWords_CountsCorrectly()
        {
            // Arrange
            var content = new MarkdownContent("This has exactly five words");

            // Act
            var count = content.WordCount;

            // Assert
            Assert.That(count, Is.EqualTo(5));
        }

        [Test]
        public void WordCount_EmptyContent_ReturnsZero()
        {
            // Arrange
            var content = new MarkdownContent("");

            // Act
            var count = content.WordCount;

            // Assert
            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void WordCount_OnlyWhitespace_ReturnsZero()
        {
            // Arrange
            var content = new MarkdownContent("   \n  \t  ");

            // Act
            var count = content.WordCount;

            // Assert
            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void IsEmpty_EmptyContent_ReturnsTrue()
        {
            // Arrange
            var content = new MarkdownContent("");

            // Act & Assert
            Assert.That(content.IsEmpty, Is.True);
        }

        [Test]
        public void IsEmpty_WhitespaceContent_ReturnsTrue()
        {
            // Arrange
            var content = new MarkdownContent("   \n  ");

            // Act & Assert
            Assert.That(content.IsEmpty, Is.True);
        }

        [Test]
        public void IsEmpty_ContentWithText_ReturnsFalse()
        {
            // Arrange
            var content = new MarkdownContent("Some content");

            // Act & Assert
            Assert.That(content.IsEmpty, Is.False);
        }

        [Test]
        public void Equals_SameContent_ReturnsTrue()
        {
            // Arrange
            var content1 = new MarkdownContent("# Same content");
            var content2 = new MarkdownContent("# Same content");

            // Act & Assert
            Assert.That(content1, Is.EqualTo(content2));
            Assert.That(content1.GetHashCode(), Is.EqualTo(content2.GetHashCode()));
        }

        [Test]
        public void Equals_DifferentContent_ReturnsFalse()
        {
            // Arrange
            var content1 = new MarkdownContent("# First content");
            var content2 = new MarkdownContent("# Second content");

            // Act & Assert
            Assert.That(content1, Is.Not.EqualTo(content2));
        }

        [Test]
        public void ImplicitConversion_ToString_ReturnsRawContent()
        {
            // Arrange
            var content = new MarkdownContent("# Test content");

            // Act
            string contentString = content;

            // Assert
            Assert.That(contentString, Is.EqualTo("# Test content"));
        }

        [Test]
        public void ImplicitConversion_FromString_CreatesMarkdownContent()
        {
            // Arrange & Act
            MarkdownContent content = "# Test content";

            // Assert
            Assert.That(content.RawContent, Is.EqualTo("# Test content"));
        }
    }
}
