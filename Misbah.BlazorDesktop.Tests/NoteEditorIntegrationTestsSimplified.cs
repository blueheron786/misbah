using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Misbah.BlazorDesktop.Components.Pages.Notes;
using Misbah.Core.Services;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Misbah.BlazorDesktop.Tests
{
    /// <summary>
    /// Simplified NUnit tests for NoteEditor integration scenarios
    /// </summary>
    [TestFixture]
    public class NoteEditorIntegrationTestsSimplified : Bunit.TestContext
    {
        public NoteEditorIntegrationTestsSimplified()
        {
            Services.AddSingleton<MarkdownRenderer>(new MockMarkdownRenderer());
            Services.AddSingleton<IJSRuntime>(new MockJSRuntime());
        }

        [SetUp]
        public void Setup()
        {
            // bUnit services are already registered in constructor
        }

        [Test]
        public void BasicEditor_RendersWithoutErrors()
        {
            // Simple test to ensure basic rendering works
            var component = RenderComponent<TrueLiveMarkdownEditor>(parameters => parameters
                .Add(p => p.Content, "# Test Header\n\nTest content"));

            var blocks = component.FindAll(".live-block");
            Assert.That(blocks.Count, Is.GreaterThan(0));
        }

        [Test]
        public void EmptyEditor_RendersPlaceholder()
        {
            var component = RenderComponent<TrueLiveMarkdownEditor>(parameters => parameters
                .Add(p => p.Content, ""));

            var placeholder = component.Find(".empty-placeholder");
            Assert.That(placeholder, Is.Not.Null);
        }

        #region Mock Classes

        private class MockMarkdownRenderer : MarkdownRenderer
        {
            public MockMarkdownRenderer() : base() { }

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
