using System.Collections.Generic;
using System.Text.RegularExpressions;
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
            Assert.That(html, Does.Contain("<ul class='task-list'>"));
            Assert.That(Regex.Matches(html, "<ul class='task-list'>").Count, Is.EqualTo(1));
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

        [Test]
        public void Renders_Obsidian_Style_Table()
        {
            var renderer = new MarkdownRenderer();
            var md = @"| Species      | Name            | World Type                                       | Niche / Strength                        | Tone                                    |
| ------------ | --------------- | ------------------------------------------------ | --------------------------------------- | --------------------------------------- |
| 🐱 Cats      | [[Nyssari v1]]  | Verdant jungle or desert luxury world            | Traders, artists, aristocrats, duelists | Elegant, vain, sarcastic                |
| 🐧 Penguins  | [[Glacari]]     | Ice-crusted world with geothermal seas           | Cryo-tech engineers, survivalists       | Stoic, dry-humored, quietly badass      |
| 🦊 Foxes     | [[Feydra]]      | Lush temperate forests, diplomatic cities        | Spies, diplomats, tricksters,           | Clever, political, sardonic             |
| 🐘 Elephants | [[Tromians]]    | Massive gravity world; rocky, slow-changing      | Builders, archivists, strategists       | Stoic, ancient, dignified               |
| 🐦 Birds     | [[Aetherin]]    | Floating cities on gas giant moons or sky cliffs | Scouts, messengers, fast attack         | Swift, loyal, poetic                    |
| 🐙 Octopuses | [[Quaralune]]   | Oceanic abyss world with biotechnological reefs  | Engineers, philosophers, fluid thinkers | Alien, cerebral, flowing                |
| 🐍 Snakes    | [[Slytherix]]   | Swampy, humid, bioluminescent jungles            | Biotech, assassins, stealth             | Mysterious, eerie, precise              |
| 🐺 Doggos    | [[Dromakai v1]] | Mountainous, volcanic world of ash and obsidian  | Loyal oath-bound warriors, duelists     | Quiet, focused, disciplined, loyal      |
| 🦀 Crabs     | [[Korithal v1]] | Varies: from desert to ocean                     | Ancient, unknowable, watching           | Ancient. Alien. Unsettling. Unknowable. |
| 🐞 Bugs      | [[Chirrix v1]]  | Vast hives underground and desolate plains       | Assimilate all, adapt endlessly         | Ravenous, terrifying, hive-minded       |";

            var html = renderer.Render(md, out _);

            Assert.That(html, Does.Contain("<table>"));
            Assert.That(html, Does.Contain("<th>Species</th>"));
            Assert.That(html, Does.Contain("<td><a href='Nyssari v1' target='_blank'>Nyssari v1</a></td>"));
            Assert.That(Regex.Matches(html, "<tr>").Count, Is.EqualTo(11));
        }

        [Test]
        public void EscapedCharacters_RenderWithoutBackslashes()
        {
            var renderer = new MarkdownRenderer();
            var md = "\\* literal star\n\\## heading literal";

            var html = renderer.Render(md, out _);

            Assert.That(html, Does.Contain("* literal star<br>"));
            Assert.That(html, Does.Contain("## heading literal<br>"));
        }

        [Test]
        public void EscapedCharacters_RenderInsideTables()
        {
            var renderer = new MarkdownRenderer();
            var md = "| Col |\n| --- |\n| \\* literal star |";

            var html = renderer.Render(md, out _);

            Assert.That(html, Does.Contain("<td>* literal star</td>"));
        }

        [Test]
        public void HeadingLines_RenderAsHeadings()
        {
            var renderer = new MarkdownRenderer();
            var md = "# Title\n## Section\n### Sub";

            var html = renderer.Render(md, out _);

            Assert.That(html, Does.Contain("<h1>Title</h1>"));
            Assert.That(html, Does.Contain("<h2>Section</h2>"));
            Assert.That(html, Does.Contain("<h3>Sub</h3>"));
        }

        [Test]
        public void EscapedHeadingPrefix_DoesNotRenderAsHeading()
        {
            var renderer = new MarkdownRenderer();
            var md = "\\## Not heading";

            var html = renderer.Render(md, out _);

            Assert.That(html, Does.Contain("## Not heading<br>"));
        }
    }
}
