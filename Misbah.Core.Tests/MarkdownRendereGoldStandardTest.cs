using NUnit.Framework;
using Misbah.Core.Services;

namespace Misbah.Core.Tests
{
    [TestFixture]
    public class MarkdownRendererGoldStandardTests
    {
        private MarkdownRenderer _renderer;

        [SetUp]
        public void SetUp()
        {
            _renderer = new MarkdownRenderer();
        }

        [Test]
        // One test to rule them all, and in the darkness bind them.
        // One test to cover all features, all custom rendering, all in one shot.
        public void ComprehensiveMarkdownTest_AllFeatures()
        {
            // This is a comprehensive markdown document that tests ALL features
            var markdownInput = @"# Main Heading

This is a paragraph with **bold text**, *italic text*, and ***bold italic text***.

Here's some `inline code` and ==highlighted text==.

## Links and References

External link: [Google](https://google.com)
Wiki link: [[My Note]]
Wiki link with display text: [[CSharp|C# Programming]]

## Lists

### Task Lists
- [ ] Incomplete task
- [x] Completed task
- [ ] Another incomplete task

### Regular Lists
- Item one
- Item two
- Item three

## Code Blocks

```
function hello() {
    console.log('Hello, World!');
}
```

Another code block:
```javascript
const greeting = 'Hello';
console.log(greeting);
```

## Special Cases

Empty lines should be collapsed:


Multiple empty lines above.

HTML should pass through: <b>bold</b> and <mark>marked</mark>.

Special characters in links: [Test & < > "" ''](https://example.com?param=value&other=test)

Edge case with backticks: `code with ` incomplete backtick

Nested formatting: **Bold with *italic* inside**

## Line Breaks

Line one
Line two
Line three

End of document.";

            // Expected HTML output - this captures the EXACT current behavior
            var expectedHtml = @"<h1>Main Heading</h1>
<br>
This is a paragraph with <strong>bold text</strong>, <em>italic text</em>, and <em><strong>bold italic text</strong></em>.<br>
<br>
Here's some <code class='misbah-code'>inline code</code> and <mark>highlighted text</mark>.<br>
<br>
<h2>Links and References</h2>
<br>
External link: <a href='https://google.com' target='_blank'>Google</a><i class='fa fa-external-link-alt' style='font-size:0.95em;vertical-align:middle;'></i><br>
Wiki link: <a href='My Note' target='_blank'>My Note</a><br>
Wiki link with display text: <a href='CSharp' target='_blank'>C# Programming</a><br>
<br>
<h2>Lists</h2>
<br>
<h3>Task Lists</h3>
<ul>
<li><input type='checkbox' class='md-task' data-line='15'  onclick=""window.dispatchEvent(new CustomEvent('misbah-task-toggle',{detail:{line:15}}));""> Incomplete task</li>
<li><input type='checkbox' class='md-task' data-line='16' checked onclick=""window.dispatchEvent(new CustomEvent('misbah-task-toggle',{detail:{line:16}}));""> Completed task</li>
<li><input type='checkbox' class='md-task' data-line='17'  onclick=""window.dispatchEvent(new CustomEvent('misbah-task-toggle',{detail:{line:17}}));""> Another incomplete task</li>
<br>
</ul>
<h3>Regular Lists</h3>
<ul>
<li>Item one</li>
<li>Item two</li>
<li>Item three</li>
<br>
</ul>
<h2>Code Blocks</h2>
<br>
<pre class='misbah-code'><code class='misbah-code'>
function hello() {
    console.log(&#39;Hello, World!&#39;);
}
</code></pre>
<br>
Another code block:<br>
<pre class='misbah-code'><code class='misbah-code'>
const greeting = &#39;Hello&#39;;
console.log(greeting);
</code></pre>
<br>
<h2>Special Cases</h2>
<br>
Empty lines should be collapsed:<br>
<br>
Multiple empty lines above.<br>
<br>
HTML should pass through: <b>bold</b> and <mark>marked</mark>.<br>
<br>
Special characters in links: <a href='https://example.com?param=value&other=test' target='_blank'>Test & < > "" ''</a><i class='fa fa-external-link-alt' style='font-size:0.95em;vertical-align:middle;'></i><br>
<br>
Edge case with backticks: <code class='misbah-code'>code with </code> incomplete backtick<br>
<br>
Nested formatting: <strong>Bold with <em>italic</em> inside</strong><br>
<br>
<h2>Line Breaks</h2>
<br>
Line one<br>
Line two<br>
Line three<br>
<br>
End of document.<br>";

            // Act
            var actualHtml = _renderer.Render(markdownInput, out var taskLines);

            // Assert - Golden Master Test
            // Clean up whitespace for comparison
            var cleanExpected = CleanHtml(expectedHtml);
            var cleanActual = CleanHtml(actualHtml);

            if (cleanActual != cleanExpected)
            {
                // Print diff for debugging
                TestContext.Out.WriteLine("=== EXPECTED HTML ===");
                TestContext.Out.WriteLine(cleanExpected);
                TestContext.Out.WriteLine("\n=== ACTUAL HTML ===");
                TestContext.Out.WriteLine(cleanActual);
                TestContext.Out.WriteLine("\n=== DIFF (showing first difference) ===");
                ShowFirstDifference(cleanExpected, cleanActual);
            }

            Assert.That(cleanActual, Is.EqualTo(cleanExpected),
                "HTML output has changed! This indicates the markdown rendering has been modified. " +
                "If this change is intentional, update the expected HTML in this test.");

            // Also verify task lines are correct
            var expectedTaskLines = new[] { 15, 16, 17 }; // Lines where checkboxes appear
            Assert.That(taskLines, Is.EquivalentTo(expectedTaskLines),
                "Task line numbers have changed!");
        }

        [Test]
        public void EmptyAndNullInputs_GoldenMaster()
        {
            // Test edge cases
            var emptyResult = _renderer.Render("", out var emptyTaskLines);
            var nullResult = _renderer.Render(null, out var nullTaskLines);
            var whitespaceResult = _renderer.Render("   \n  \t  \n    ", out var whitespaceTaskLines);

            Assert.That(emptyResult, Is.EqualTo("<br>"));
            Assert.That(nullResult, Is.EqualTo("<br>"));
            Assert.That(whitespaceResult, Is.EqualTo("<br>"));
            
            Assert.That(emptyTaskLines, Is.Empty);
            Assert.That(nullTaskLines, Is.Empty);
            Assert.That(whitespaceTaskLines, Is.Empty);
        }

        private string CleanHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            return html
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Trim();
        }

        private void ShowFirstDifference(string expected, string actual)
        {
            var minLength = Math.Min(expected.Length, actual.Length);
            
            for (int i = 0; i < minLength; i++)
            {
                if (expected[i] != actual[i])
                {
                    var start = Math.Max(0, i - 50);
                    var expectedSnippet = expected.Substring(start, Math.Min(100, expected.Length - start));
                    var actualSnippet = actual.Substring(start, Math.Min(100, actual.Length - start));
                    
                    TestContext.Out.WriteLine($"First difference at position {i}:");
                    TestContext.Out.WriteLine($"Expected: ...{expectedSnippet}...");
                    TestContext.Out.WriteLine($"Actual:   ...{actualSnippet}...");
                    TestContext.Out.WriteLine($"Character at difference - Expected: '{expected[i]}' (0x{(int)expected[i]:X2}), Actual: '{actual[i]}' (0x{(int)actual[i]:X2})");
                    break;
                }
            }

            if (expected.Length != actual.Length)
            {
                TestContext.Out.WriteLine($"Length difference - Expected: {expected.Length}, Actual: {actual.Length}");
            }
        }
    }
}
