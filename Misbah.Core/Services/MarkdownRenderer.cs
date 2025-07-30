using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Markdig;

namespace Misbah.Core.Services
{
    public class MarkdownRenderer
    {
        /// <summary>
        /// Renders markdown content to HTML, extracting task list line numbers.
        /// </summary>
        /// <param name="content">The markdown content.</param>
        /// <param name="taskLineNumbers">Outputs the line numbers of markdown tasks.</param>
        /// <returns>HTML string.</returns>
        public string Render(string? content, out List<int> taskLineNumbers)
        {
            string[] lines = (content ?? "").Split('\n');
            var htmlLines = new List<string>();
            bool inList = false;
            bool inCodeBlock = false;
            int emptyCount = 0;
            taskLineNumbers = new List<int>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (IsCodeBlockDelimiter(lines[i]))
                {
                    HandleCodeBlockDelimiter(ref inCodeBlock, htmlLines);
                    emptyCount = 0;
                    continue;
                }
                if (inCodeBlock)
                {
                    htmlLines.Add(System.Net.WebUtility.HtmlEncode(lines[i]) + "\n");
                    continue;
                }
                if (TryRenderTaskList(lines[i], i, htmlLines, taskLineNumbers, ref inList))
                {
                    emptyCount = 0;
                    continue;
                }
                if (inList) { htmlLines.Add("</ul>"); inList = false; }
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    emptyCount++;
                    if (emptyCount == 1) htmlLines.Add("<br>");
                    continue;
                }
                emptyCount = 0;
                var inlineCode = RenderInlineCode(lines[i]);
                var lineHtml = Markdown.ToHtml(inlineCode);
                if (lineHtml.StartsWith("<p>") && lineHtml.EndsWith("</p>\n"))
                    lineHtml = lineHtml.Substring(3, lineHtml.Length - 8);
                htmlLines.Add(lineHtml);
            }
            if (inList) { htmlLines.Add("</ul>"); }
            return string.Join("", htmlLines);
        }

        /// <summary>
        /// Renders markdown content to HTML, applying all post-processing (wiki links, external link emoji).
        /// </summary>
        /// <param name="content">The markdown content.</param>
        /// <param name="taskLineNumbers">Outputs the line numbers of markdown tasks.</param>
        /// <returns>HTML string with all post-processing applied.</returns>
        public string RenderFull(string? content, out List<int> taskLineNumbers)
        {
            var html = Render(content, out taskLineNumbers);
            html = ReplaceWikiLinks(html);
            html = AddExternalLinkEmoji(html);
            return html;
        }

        /// <summary>
        /// Adds 🌐 emoji to external links in the HTML.
        /// </summary>
        public string AddExternalLinkEmoji(string html)
        {
            return Regex.Replace(
                html,
                @"<a ([^>]*href=""https?://[^""]+""[^>]*?)>(.*?)</a>",
                m => "<a " + m.Groups[1].Value + ">" + m.Groups[2].Value + "</a> 🌐",
                RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Replaces Obsidian-style [[Page Name]] wiki links with clickable HTML links.
        /// </summary>
        public string ReplaceWikiLinks(string html)
        {
            return Regex.Replace(
                html,
                @"\[\[([^\]]+)\]\]",
                new MatchEvaluator(m =>
                {
                    var page = m.Groups[1].Value.Replace("\"", "&quot;");
                    var linkHtml = "<a href=\"#\" onclick=\"window.dispatchEvent(new CustomEvent('misbah-nav', { detail: { title: '" + page + "' } }));return false;\">" + page + "</a>";
                    return linkHtml;
                }),
                RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Extracts all wiki link page names from markdown content.
        /// </summary>
        public List<string> ExtractWikiLinks(string? content)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(content)) return result;
            foreach (Match m in Regex.Matches(content, @"\[\[([^\]]+)\]\]"))
            {
                result.Add(m.Groups[1].Value);
            }
            return result;
        }

        /// <summary>
        /// Extracts all markdown task lines (with their line numbers and checked state).
        /// </summary>
        public List<(int line, bool isChecked, string text)> ExtractTasks(string? content)
        {
            var result = new List<(int, bool, string)>();
            if (string.IsNullOrEmpty(content)) return result;
            var lines = content.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var match = Regex.Match(lines[i], @"^- \[( |x)\] (.*)$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    bool isChecked = match.Groups[1].Value.ToLower() == "x";
                    string taskText = match.Groups[2].Value;
                    result.Add((i, isChecked, taskText));
                }
            }
            return result;
        }

        // --- Private helpers ---
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
