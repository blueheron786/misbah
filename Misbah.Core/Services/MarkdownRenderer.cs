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
            bool inNormalList = false;
            bool inTaskList = false;
            bool inCodeBlock = false;
            int emptyCount = 0;
            var taskLines = new List<int>();
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            for (int i = 0; i < lines.Length; i++)
            {
                if (IsCodeBlockDelimiter(lines[i]))
                {
                    if (inNormalList) { htmlLines.Add("</ul>"); inNormalList = false; }
                    if (inTaskList) { htmlLines.Add("</ul>"); inTaskList = false; }
                    HandleCodeBlockDelimiter(ref inCodeBlock, htmlLines);
                    emptyCount = 0;
                    continue;
                }
                if (inCodeBlock)
                {
                    htmlLines.Add(System.Net.WebUtility.HtmlEncode(lines[i]) + "\n");
                    continue;
                }
                // Task list
                var isTask = Regex.IsMatch(lines[i], @"^- \[( |x)\] ", RegexOptions.IgnoreCase);
                if (isTask)
                {
                    if (inNormalList) { htmlLines.Add("</ul>"); inNormalList = false; }
                    if (!inTaskList) { htmlLines.Add("<ul class='md-task-list'>"); inTaskList = true; }
                    TryRenderTaskList(lines[i], i, htmlLines, taskLines, ref inTaskList);
                    emptyCount = 0;
                    continue;
                }
                // Normal markdown list (not a task list)
                var isNormalList = Regex.IsMatch(lines[i], @"^\s*[-*+] ") && !isTask;
                if (isNormalList)
                {
                    if (inTaskList) { htmlLines.Add("</ul>"); inTaskList = false; }
                    if (!inNormalList) { htmlLines.Add("<ul>"); inNormalList = true; }
                    string item = lines[i].Trim().Substring(2);
                    // Preprocess for highlight and bold/italic before Markdig for list items
                    item = Regex.Replace(item, "==([^=]+)==", m => $"<span class='md-highlight'>{System.Net.WebUtility.HtmlEncode(m.Groups[1].Value)}</span>");
                    item = Regex.Replace(item, @"\*\*\*([^*]+)\*\*\*", m => $"<b><i>{System.Net.WebUtility.HtmlEncode(m.Groups[1].Value)}</i></b>");
                    // Inline code for list items
                    item = RenderInlineCode(item);
                    var itemHtml = Markdown.ToHtml(item, pipeline);
                    if (itemHtml.StartsWith("<p>") && itemHtml.EndsWith("</p>\n"))
                        itemHtml = itemHtml.Substring(3, itemHtml.Length - 8);
                    htmlLines.Add($"<li>{itemHtml}</li>");
                    emptyCount = 0;
                    continue;
                }
                // Blank line closes any open list
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    if (inNormalList) { htmlLines.Add("</ul>"); inNormalList = false; }
                    if (inTaskList) { htmlLines.Add("</ul>"); inTaskList = false; }
                    emptyCount++;
                    if (emptyCount == 1) htmlLines.Add("<br>");
                    continue;
                }
                if (inNormalList) { htmlLines.Add("</ul>"); inNormalList = false; }
                if (inTaskList) { htmlLines.Add("</ul>"); inTaskList = false; }
                emptyCount = 0;
                // Preprocess for highlight and bold/italic before Markdig
                var processed = lines[i];
                processed = Regex.Replace(processed, "==([^=]+)==", m => $"<span class='md-highlight'>{System.Net.WebUtility.HtmlEncode(m.Groups[1].Value)}</span>");
                processed = Regex.Replace(processed, @"\*\*\*([^*]+)\*\*\*", m => $"<b><i>{System.Net.WebUtility.HtmlEncode(m.Groups[1].Value)}</i></b>");
                var inlineCode = RenderInlineCode(processed);
                var lineHtml = Markdown.ToHtml(inlineCode, pipeline);
                if (lineHtml.StartsWith("<p>") && lineHtml.EndsWith("</p>\n"))
                    lineHtml = lineHtml.Substring(3, lineHtml.Length - 8);
                htmlLines.Add(lineHtml);
            }
            if (inTaskList) { htmlLines.Add("</ul>"); }
            if (inNormalList) { htmlLines.Add("</ul>"); }
            taskLineNumbers = taskLines;
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
                if (!inList) { htmlLines.Add("<ul class='md-task-list'>"); inList = true; }
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
