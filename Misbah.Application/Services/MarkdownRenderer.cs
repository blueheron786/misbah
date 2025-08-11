using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Renderers;

namespace Misbah.Application.Services
{
    public class MarkdownRenderer
    {
        /// <summary>
        /// Adds 🌐 emoji to external links in the HTML.
        /// </summary>
        private string AddExternalLinkEmoji(string html)
        {
            // Replace with Font Awesome external-link icon (handle both single and double quotes for href)
            return Regex.Replace(
                html,
                "<a ([^>]*href=(?:'|\")https?://[^'\\\"]+(?:'|\")[^>]*?)>(.*?)</a>",
                m => "<a " + m.Groups[1].Value + ">" + m.Groups[2].Value + "</a><i class='fa fa-external-link-alt' style='font-size:0.95em;vertical-align:middle;'></i>",
                RegexOptions.IgnoreCase);
        }
        /// <summary>
        /// Renders markdown content to HTML, applying all post-processing (wiki links, external link emoji), and extracts task list line numbers.
        /// </summary>
        /// <param name="content">The markdown content.</param>
        /// <param name="taskLineNumbers">Outputs the line numbers of markdown tasks.</param>
        /// <returns>HTML string with all post-processing applied.</returns>
        public string Render(string? content, out List<int> taskLineNumbers)
        {
            // Custom renderer for lists, checkboxes, code blocks, and inline code
            var lines = (content ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            var htmlLines = new List<string>();
            taskLineNumbers = new List<int>();
            bool inList = false;
            bool inCodeBlock = false;
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            bool lastWasBlank = false;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                // Highlight: ==text== to <mark>text</mark>
                line = Regex.Replace(line, "==(.+?)==", m => $"<mark>{System.Net.WebUtility.HtmlEncode(m.Groups[1].Value)}</mark>");

                if (IsCodeBlockDelimiter(line))
                {
                    HandleCodeBlockDelimiter(ref inCodeBlock, htmlLines);
                    lastWasBlank = false;
                    continue;
                }
                if (inCodeBlock)
                {
                    htmlLines.Add(System.Net.WebUtility.HtmlEncode(line));
                    lastWasBlank = false;
                    continue;
                }
                if (string.IsNullOrWhiteSpace(line))
                {
                    // Only insert a single <br> for consecutive blank lines
                    if (!lastWasBlank)
                    {
                        htmlLines.Add("<br>");
                        lastWasBlank = true;
                    }
                    continue;
                }
                if (IsTaskListLine(line))
                {
                    TryRenderTaskList(line, i, htmlLines, taskLineNumbers, ref inList);
                    lastWasBlank = false;
                    continue;
                }
                if (IsNormalListLine(line))
                {
                    if (!inList) { htmlLines.Add("<ul>"); inList = true; }
                    htmlLines.Add($"<li>{line.Substring(2).Trim()}</li>");
                    lastWasBlank = false;
                    continue;
                }
                else if (inList)
                {
                    htmlLines.Add("</ul>");
                    inList = false;
                }

                // Markdown: bold+italic, bold, italic, links
                var processed = line;
                // Bold+italic: ***text***
                processed = Regex.Replace(processed, @"\*\*\*(.+?)\*\*\*", "<em><strong>$1</strong></em>");
                // Bold: **text**
                processed = Regex.Replace(processed, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
                // Italic: *text*
                processed = Regex.Replace(processed, @"\*(.+?)\*", "<em>$1</em>");
                // Markdown links: [text](url)
                processed = Regex.Replace(processed, @"\[([^\]]+)\]\(([^\)]+)\)", "<a href='$2' target='_blank'>$1</a>");
                processed = RenderInlineCode(processed);
                // Add <br> after every normal line (not blank, not list, not code)
                htmlLines.Add(processed + "<br>");
                lastWasBlank = false;
            }

            if (inList) htmlLines.Add("</ul>");
            if (inCodeBlock) htmlLines.Add("</code></pre>");
            var html = string.Join("\n", htmlLines);
            html = ReplaceWikiLinks(html);
            html = AddExternalLinkEmoji(html);
            return html;
        }

        /// <summary>
        /// Replaces Obsidian-style [[Page Name]] wiki links with clickable HTML links.
        /// </summary>
        public string ReplaceWikiLinks(string html)
        {
            // You may want to inject INoteService for real existence check, but for now, simulate with a static list or always missing for test
            return Regex.Replace(
                html,
                @"\[\[([^\]|]+)(?:\|([^\]]+))?\]\]",
                new MatchEvaluator(m =>
                {
                    var page = m.Groups[1].Value.Replace("\"", "&quot;");
                    var display = m.Groups[2].Success ? m.Groups[2].Value : page;
                    // For now, always treat as missing
                    return $"<a href='{page}' target='_blank'>{display}</a>";
                }),
                RegexOptions.Compiled);
        }

        // For testing: set of existing pages
        private HashSet<string>? _existingPages;
        public void SetExistingPages(IEnumerable<string> pages)
        {
            _existingPages = new HashSet<string>(pages, StringComparer.OrdinalIgnoreCase);
        }

        // --- Private helpers ---
        // Helper to detect if a line is a normal (non-task) list item
        private bool IsNormalListLine(string line)
        {
            // Match lines like "- item" but not "- [ ] item" or "- [x] item"
            return Regex.IsMatch(line, "^- (?!\\[[ xX]\\]).+", RegexOptions.IgnoreCase);
        }
        // (removed duplicate IsNormalListLine)
        // Helper to detect if a line is a task list item
        private bool IsTaskListLine(string line)
        {
            return Regex.IsMatch(line, @"^- \[( |x)\] (.*)$", RegexOptions.IgnoreCase);
        }
        private bool IsCodeBlockDelimiter(string line)
        {
            return line.TrimStart().StartsWith("```");
        }

        private void HandleCodeBlockDelimiter(ref bool inCodeBlock, List<string> htmlLines)
        {
            if (!inCodeBlock)
            {
                htmlLines.Add("<pre class='misbah-code'><code class='misbah-code'>");
                inCodeBlock = true;
            }
            else
            {
                htmlLines.Add("</code></pre>");
                inCodeBlock = false;
            }
        }

        private bool TryRenderTaskList(string line, int lineNumber, List<string> htmlLines, List<int> taskLineNumbers, ref bool inList)
        {
            var match = Regex.Match(line, @"^- \[( |x)\] (.*)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                if (!inList) { htmlLines.Add("<ul>"); inList = true; }
                bool isChecked = match.Groups[1].Value.ToLower() == "x";
                string taskText = match.Groups[2].Value;
                string checkbox = $"<input type='checkbox' class='md-task' data-line='{lineNumber}' {(isChecked ? "checked" : "")} onclick=\"window.dispatchEvent(new CustomEvent('misbah-task-toggle',{{detail:{{line:{lineNumber}}}}}));\">";
                htmlLines.Add($"<li>{checkbox} {System.Net.WebUtility.HtmlEncode(taskText)}</li>");
                taskLineNumbers.Add(lineNumber);
                return true;
            }
            return false;
        }

        private string RenderInlineCode(string line)
        {
            return Regex.Replace(
                line,
                "`([^`]+)`",
                m => $"<code class='misbah-code'>{System.Net.WebUtility.HtmlEncode(m.Groups[1].Value)}</code>");
        }
    }
}
