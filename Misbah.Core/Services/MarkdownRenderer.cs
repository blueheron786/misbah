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
            // Do NOT preprocess highlight here; only in RenderFull to avoid double-escaping
            string[] lines = (content ?? "").Split('\n');
            var htmlLines = new List<string>();
            var taskLines = new List<int>();

            var pipelineBuilder = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseEmphasisExtras()
                .UseTaskLists()
                .UsePipeTables()
                .UseListExtras()
                .UseAutoIdentifiers()
                .UseAutoLinks()
                .UseGenericAttributes();
            var pipeline = pipelineBuilder.Build();
            // Enable ==highlight==

            // Only custom-render task lists; let Markdig handle everything else (including normal lists and highlight)
            var block = new List<string>();
            bool inTaskList = false;
            Action flushBlock = () => {
                if (block.Count == 0) return;
                var joined = string.Join("\n", block);
                var html = Markdown.ToHtml(joined, pipeline);
                htmlLines.Add(html);
                block.Clear();
            };

            for (int i = 0; i < lines.Length; i++)
            {
                var isTask = Regex.IsMatch(lines[i], @"^- \[( |x)\] ", RegexOptions.IgnoreCase);
                if (isTask)
                {
                    flushBlock();
                    if (!inTaskList) { htmlLines.Add("<ul class='md-task-list'>"); inTaskList = true; }
                    TryRenderTaskList(lines[i], i, htmlLines, taskLines, ref inTaskList);
                }
                else
                {
                    if (inTaskList) { htmlLines.Add("</ul>"); inTaskList = false; }
                    block.Add(lines[i]);
                }
            }
            if (inTaskList) { htmlLines.Add("</ul>"); }
            flushBlock();
            taskLineNumbers = taskLines;
            var html = string.Join("", htmlLines);
            return html;
        }

        /// <summary>
        /// Preprocesses ==highlight== to <span class='md-highlight'>highlight</span> only outside code blocks.
        /// </summary>
        private string PreprocessHighlight(string markdown)
        {
            var lines = markdown.Split('\n');
            bool inCode = false;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.TrimStart().StartsWith("```") )
                {
                    inCode = !inCode;
                    continue;
                }
                if (!inCode)
                {
                    // Replace ==text== with <span class='md-highlight'>text</span> (no HTML escaping, let Markdig handle it)
                    lines[i] = Regex.Replace(line, @"==(.+?)==", m => $"<span class='md-highlight'>{m.Groups[1].Value}</span>", RegexOptions.Singleline);
                }
            }
            return string.Join("\n", lines);
        }

        /// <summary>
        /// Renders markdown content to HTML, applying all post-processing (wiki links, external link emoji).
        /// </summary>
        /// <param name="content">The markdown content.</param>
        /// <param name="taskLineNumbers">Outputs the line numbers of markdown tasks.</param>
        /// <returns>HTML string with all post-processing applied.</returns>
        public string RenderFull(string? content, out List<int> taskLineNumbers)
        {
            // Preprocess highlight before any Markdown rendering
            var preprocessed = PreprocessHighlight(content ?? "");
            var html = Render(preprocessed, out taskLineNumbers);
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
