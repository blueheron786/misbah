using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Misbah.Web.Components.Pages.Notes;
using Misbah.Web.Components;
using Misbah.Core.Services;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace Misbah.BlazorDesktop.Tests
{
    /// <summary>
    /// Tests focused on preventing regressions in markdown rendering and edge cases
    /// that could cause notes to become unrenderable or cause crashes.
    /// </summary>
    [TestFixture]
    public class MarkdownRenderingRegressionTestsClean : Bunit.TestContext
    {
        public MarkdownRenderingRegressionTestsClean()
        {
            Services.AddSingleton<MarkdownRenderer>(new TestMarkdownRenderer());
            Services.AddSingleton<IJSRuntime>(new MockJSRuntime());
        }

        [SetUp]
        public void Setup()
        {
            // bUnit services are already registered in constructor
        }

        #region Malformed Content Edge Cases

        [Test]
        public void MalformedCodeBlocks_DoesNotCrash()
        {
            // Arrange - Malformed code blocks that could crash rendering
            var content = "```csharp\nvar x = 5;\n```\n\n```\nNo language\n```\n\n```incomplete";
            
            // Act
            var component = RenderComponent<TrueLiveMarkdownEditor>(parameters => parameters
                .Add(p => p.Content, content));
            
            // Assert - Should render blocks despite malformed code blocks
            var blocks = component.FindAll(".live-block");
            Assert.That(blocks.Count, Is.GreaterThan(0), "Should render blocks despite malformed code blocks");
        }

        [Test]
        public void MalformedHeaders_DoesNotCrash()
        {
            // Arrange - Various malformed header scenarios
            var content = "# Header 1\n###NoSpace\n# \n####    SpacesOnly\n\n## Normal Header";
            
            // Act
            var component = RenderComponent<TrueLiveMarkdownEditor>(parameters => parameters
                .Add(p => p.Content, content));
            
            // Assert - Should handle malformed headers
            var blocks = component.FindAll(".live-block");
            Assert.That(blocks.Count, Is.GreaterThanOrEqualTo(4), "Should handle malformed headers");
        }

        [Test]
        public void MalformedLists_DoesNotCrash()
        {
            // Arrange - Malformed list items
            var content = "- Item 1\n-NoSpace\n-    \n* Mixed markers\n1. Numbered\n2.NoSpace";
            
            // Act
            var component = RenderComponent<TrueLiveMarkdownEditor>(parameters => parameters
                .Add(p => p.Content, content));
            
            // Assert - Should handle malformed lists
            var blocks = component.FindAll(".live-block");
            Assert.That(blocks.Count, Is.GreaterThan(0), "Should handle malformed lists");
        }

        [Test]
        public void MalformedQuotes_DoesNotCrash()
        {
            // Arrange - Malformed blockquotes
            var content = "> Normal quote\n>NoSpace\n>   \n>> Nested\n> > Spaced nested";
            
            // Act
            var component = RenderComponent<TrueLiveMarkdownEditor>(parameters => parameters
                .Add(p => p.Content, content));
            
            // Assert - Should handle malformed quotes
            var blocks = component.FindAll(".live-block");
            Assert.That(blocks.Count, Is.GreaterThan(0), "Should handle malformed quotes");
        }

        #endregion

        #region Character Encoding and Unicode

        [Test]
        public void UnicodeContent_RendersCorrectly()
        {
            // Arrange - Various Unicode characters
            var content = "# 中文标题\n\n这是一段中文内容。\n\n# العربية\n\nمحتوى باللغة العربية";
            
            // Act
            var component = RenderComponent<TrueLiveMarkdownEditor>(parameters => parameters
                .Add(p => p.Content, content));
            
            // Assert - Should render Unicode content
            var blocks = component.FindAll(".live-block");
            Assert.That(blocks.Count, Is.GreaterThanOrEqualTo(3), "Should render Unicode content");
        }

        [Test]
        public void RtlContent_RendersCorrectly()
        {
            // Arrange - Right-to-left content
            var content = "# שלום עולם\n\nזהו תוכן בעברית.\n\n# العربية\n\nهذا محتوى باللغة العربية";
            
            // Act
            var component = RenderComponent<TrueLiveMarkdownEditor>(parameters => parameters
                .Add(p => p.Content, content));
            
            // Assert - Should handle RTL content
            var blocks = component.FindAll(".live-block");
            Assert.That(blocks.Count, Is.GreaterThanOrEqualTo(3), "Should handle RTL content");
        }

        #endregion

        #region Performance Edge Cases

        [Test]
        public void VeryLargeDocument_DoesNotCrash()
        {
            // Arrange - Large document that could cause performance issues
            var contentParts = new List<string>();
            for (int i = 1; i <= 100; i++)
            {
                contentParts.Add($"# Section {i}\n\nThis is content for section {i}.\n\n- Item 1\n- Item 2");
            }
            var content = string.Join("\n\n", contentParts);
            
            // Act
            var component = RenderComponent<TrueLiveMarkdownEditor>(parameters => parameters
                .Add(p => p.Content, content));
            
            // Assert - Should handle large documents
            var blocks = component.FindAll(".live-block");
            Assert.That(blocks.Count, Is.GreaterThanOrEqualTo(50), "Should handle large documents");
        }

        [Test]
        public void DeeplyNestedContent_DoesNotCrash()
        {
            // Arrange - Deeply nested blockquotes and lists
            var content = "> Level 1\n>> Level 2\n>>> Level 3\n>>>> Level 4\n>>>>> Level 5";
            content += "\n\n- Level 1\n  - Level 2\n    - Level 3\n      - Level 4";
            
            // Act
            var component = RenderComponent<TrueLiveMarkdownEditor>(parameters => parameters
                .Add(p => p.Content, content));
            
            // Assert - Should handle deeply nested content
            var blocks = component.FindAll(".live-block");
            Assert.That(blocks.Count, Is.GreaterThan(0), "Should handle deeply nested content");
        }

        #endregion

        #region Mock Classes

        private class TestMarkdownRenderer : MarkdownRenderer
        {
            public TestMarkdownRenderer() : base() { }

            // Hide the base method with 'new' instead of 'override'
            public new string Render(string markdownText, out List<int> taskLines)
            {
                taskLines = new List<int>();
                
                if (string.IsNullOrEmpty(markdownText))
                    return "<p></p>";
                
                // Simple mock rendering
                if (markdownText.StartsWith("# "))
                    return $"<h1>{markdownText.Substring(2)}</h1>";
                if (markdownText.StartsWith("## "))
                    return $"<h2>{markdownText.Substring(3)}</h2>";
                if (markdownText.StartsWith("> "))
                    return $"<blockquote>{markdownText.Substring(2)}</blockquote>";
                if (markdownText.StartsWith("```"))
                    return $"<pre><code>{markdownText}</code></pre>";
                
                return $"<p>{markdownText}</p>";
            }
        }

        private class MockJSRuntime : IJSRuntime
        {
            public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object[] args)
            {
                return ValueTask.FromResult(default(TValue));
            }

            public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object[] args)
            {
                return ValueTask.FromResult(default(TValue));
            }
        }

        #endregion
    }
}
