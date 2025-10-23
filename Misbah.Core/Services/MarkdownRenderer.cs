using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Renderers;

namespace Misbah.Core.Services
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
            bool inCodeBlock = false;
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            bool lastWasBlank = false;
            var tableBuffer = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                if (IsCodeBlockDelimiter(line))
                {
                    if (FlushTableBuffer(tableBuffer, htmlLines))
                    {
                        lastWasBlank = false;
                    }
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

                bool isTableLine = IsTableLine(line);
                if (!isTableLine && FlushTableBuffer(tableBuffer, htmlLines))
                {
                    lastWasBlank = false;
                }

                if (isTableLine)
                {
                    tableBuffer.Add(line);
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
                var trimmedLine = line.TrimStart();
                if (TryParseListLine(trimmedLine) is ListLineInfo)
                {
                    var listHtml = RenderListBlock(lines, ref i, taskLineNumbers);
                    if (!string.IsNullOrEmpty(listHtml))
                    {
                        htmlLines.Add(listHtml);
                    }
                    lastWasBlank = false;
                    continue;
                }

                if (TryRenderHeadingLine(line, htmlLines))
                {
                    lastWasBlank = false;
                    continue;
                }

                // Markdown: bold+italic, bold, italic, links
                var processed = ProcessInlineText(line);
                // Add <br> after every normal line (not blank, not list, not code)
                htmlLines.Add(processed + "<br>");
                lastWasBlank = false;
            }

            if (FlushTableBuffer(tableBuffer, htmlLines))
            {
                lastWasBlank = false;
            }
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
        private ListLineInfo? TryParseListLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            var taskMatch = Regex.Match(line, @"^[-*+]\s+\[([ xX])\]\s+(.*)$");
            if (taskMatch.Success)
            {
                bool isChecked = string.Equals(taskMatch.Groups[1].Value, "x", StringComparison.OrdinalIgnoreCase);
                return new ListLineInfo(isOrdered: false, isTaskList: true, isChecked: isChecked, content: taskMatch.Groups[2].Value.Trim());
            }

            var unorderedMatch = Regex.Match(line, @"^[-*+]\s+(.*)$");
            if (unorderedMatch.Success)
            {
                return new ListLineInfo(isOrdered: false, isTaskList: false, isChecked: false, content: unorderedMatch.Groups[1].Value.Trim());
            }

            var orderedMatch = Regex.Match(line, @"^\d+\.\s+(.*)$");
            if (orderedMatch.Success)
            {
                return new ListLineInfo(isOrdered: true, isTaskList: false, isChecked: false, content: orderedMatch.Groups[1].Value.Trim());
            }

            return null;
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

        private string RenderListBlock(string[] lines, ref int index, List<int> taskLineNumbers)
        {
            var sb = new StringBuilder();
            var listStack = new Stack<ListLevel>();
            bool processedAny = false;

            int lineIndex = index;
            for (; lineIndex < lines.Length; lineIndex++)
            {
                var rawLine = lines[lineIndex];
                if (string.IsNullOrWhiteSpace(rawLine))
                {
                    break;
                }

                var trimmed = rawLine.TrimStart();
                var info = TryParseListLine(trimmed);
                if (info is null)
                {
                    break;
                }

                processedAny = true;
                int indent = rawLine.Length - trimmed.Length;
                AdjustListStack(sb, listStack, indent, info.Value);

                var currentLevel = listStack.Peek();
                sb.Append("<li>");
                sb.Append(BuildListItemContent(info.Value, lineIndex, taskLineNumbers));
                currentLevel.HasOpenItem = true;
            }

            while (listStack.Count > 0)
            {
                var level = listStack.Pop();
                if (level.HasOpenItem)
                {
                    sb.Append("</li>");
                    level.HasOpenItem = false;
                }
                sb.Append(level.IsOrdered ? "</ol>" : "</ul>");
            }

            if (processedAny)
            {
                index = lineIndex - 1;
                return sb.ToString();
            }

            return string.Empty;
        }

        private void AdjustListStack(StringBuilder sb, Stack<ListLevel> listStack, int indent, ListLineInfo info)
        {
            while (listStack.Count > 0 && indent < listStack.Peek().Indent)
            {
                CloseListLevel(sb, listStack.Pop());
            }

            if (listStack.Count == 0)
            {
                OpenNewList(sb, listStack, indent, info);
                return;
            }

            var current = listStack.Peek();

            if (indent > current.Indent)
            {
                OpenNewList(sb, listStack, indent, info);
                return;
            }

            if (indent == current.Indent)
            {
                if (current.IsOrdered != info.IsOrdered || current.IsTaskList != info.IsTaskList)
                {
                    CloseListLevel(sb, listStack.Pop());
                    OpenNewList(sb, listStack, indent, info);
                }
                else if (current.HasOpenItem)
                {
                    sb.Append("</li>");
                    current.HasOpenItem = false;
                }
            }
            else
            {
                // indent fell between existing levels; open a new list for this indent
                OpenNewList(sb, listStack, indent, info);
            }
        }

        private void OpenNewList(StringBuilder sb, Stack<ListLevel> listStack, int indent, ListLineInfo info)
        {
            if (info.IsOrdered)
            {
                sb.Append("<ol>");
            }
            else if (info.IsTaskList)
            {
                sb.Append("<ul class='task-list'>");
            }
            else
            {
                sb.Append("<ul>");
            }

            listStack.Push(new ListLevel(indent, info.IsOrdered, info.IsTaskList));
        }

        private void CloseListLevel(StringBuilder sb, ListLevel level)
        {
            if (level.HasOpenItem)
            {
                sb.Append("</li>");
                level.HasOpenItem = false;
            }

            sb.Append(level.IsOrdered ? "</ol>" : "</ul>");
        }

        private string BuildListItemContent(ListLineInfo info, int lineNumber, List<int> taskLineNumbers)
        {
            if (info.IsTaskList)
            {
                taskLineNumbers.Add(lineNumber);
                string taskText = RemoveMarkdownEscapes(info.Content);
                string checkbox = $"<input type='checkbox' class='md-task' data-line='{lineNumber}' {(info.IsChecked ? "checked" : string.Empty)} onclick=\"window.dispatchEvent(new CustomEvent('misbah-task-toggle',{{detail:{{line:{lineNumber}}}}}));\">";
                return $"{checkbox} {System.Net.WebUtility.HtmlEncode(taskText)}";
            }

            return ProcessInlineText(info.Content);
        }

        private readonly struct ListLineInfo
        {
            public ListLineInfo(bool isOrdered, bool isTaskList, bool isChecked, string content)
            {
                IsOrdered = isOrdered;
                IsTaskList = isTaskList;
                IsChecked = isChecked;
                Content = content;
            }

            public bool IsOrdered { get; }
            public bool IsTaskList { get; }
            public bool IsChecked { get; }
            public string Content { get; }
        }

        private sealed class ListLevel
        {
            public ListLevel(int indent, bool isOrdered, bool isTaskList)
            {
                Indent = indent;
                IsOrdered = isOrdered;
                IsTaskList = isTaskList;
            }

            public int Indent { get; }
            public bool IsOrdered { get; }
            public bool IsTaskList { get; }
            public bool HasOpenItem { get; set; }
        }

        private string RenderInlineCode(string line)
        {
            return Regex.Replace(
                line,
                "`([^`]+)`",
                m => $"<code class='misbah-code'>{System.Net.WebUtility.HtmlEncode(m.Groups[1].Value)}</code>");
        }

        private string ApplyHighlight(string line)
        {
            return Regex.Replace(line, "==(.+?)==", m => $"<mark>{System.Net.WebUtility.HtmlEncode(m.Groups[1].Value)}</mark>");
        }

        private string ProcessInlineText(string text)
        {
            var processed = ProtectEscapedCharacters(text, out var escapes);
            processed = ApplyHighlight(processed);
            processed = ApplyBasicInlineFormatting(processed);
            return RestoreEscapedCharacters(processed, escapes);
        }

        private string ApplyBasicInlineFormatting(string text)
        {
            var processed = text;
            processed = Regex.Replace(processed, @"\*\*\*(.+?)\*\*\*", "<em><strong>$1</strong></em>");
            processed = Regex.Replace(processed, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
            processed = Regex.Replace(processed, @"\*(.+?)\*", "<em>$1</em>");
            processed = Regex.Replace(processed, @"\[([^\]]+)\]\(([^\)]+)\)", "<a href='$2' target='_blank'>$1</a>");
            processed = RenderInlineCode(processed);
            return processed;
        }

        private string RemoveMarkdownEscapes(string text)
        {
            return Regex.Replace(text, @"\\([\\`*_{}\[\]()#+\-.!|>])", "$1");
        }

        private bool IsTableLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            var trimmed = line.Trim();
            if (!trimmed.StartsWith("|", StringComparison.Ordinal))
            {
                return false;
            }

            return trimmed.Count(c => c == '|') >= 2;
        }

        private bool FlushTableBuffer(List<string> tableBuffer, List<string> htmlLines)
        {
            if (tableBuffer.Count == 0)
            {
                return false;
            }

            var bufferedLines = tableBuffer.ToList();
            tableBuffer.Clear();

            var tableHtml = TryRenderTable(bufferedLines);
            if (tableHtml != null)
            {
                htmlLines.Add(tableHtml);
                return true;
            }

            foreach (var fallbackLine in bufferedLines)
            {
                htmlLines.Add(ProcessInlineText(fallbackLine) + "<br>");
            }

            return bufferedLines.Count > 0;
        }

        private string? TryRenderTable(List<string> tableLines)
        {
            if (tableLines.Count < 2)
            {
                return null;
            }

            var headerCells = SplitTableRow(tableLines[0]);
            if (headerCells.Count == 0)
            {
                return null;
            }

            var alignments = ParseAlignmentRow(tableLines[1], headerCells.Count);
            if (alignments == null)
            {
                return null;
            }

            var columnCount = Math.Max(headerCells.Count, alignments.Count);
            while (headerCells.Count < columnCount)
            {
                headerCells.Add(string.Empty);
            }
            while (alignments.Count < columnCount)
            {
                alignments.Add(null);
            }

            var sb = new StringBuilder();
            sb.Append("<table>");
            sb.Append("<thead><tr>");
            for (int col = 0; col < columnCount; col++)
            {
                sb.Append("<th");
                var align = alignments[col];
                if (!string.IsNullOrEmpty(align) && !string.Equals(align, "left", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(" style='text-align:").Append(align).Append("'>");
                }
                else
                {
                    sb.Append(">");
                }

                sb.Append(ProcessInlineText(headerCells[col].Trim()));
                sb.Append("</th>");
            }
            sb.Append("</tr></thead>");

            if (tableLines.Count > 2)
            {
                sb.Append("<tbody>");
                for (int rowIndex = 2; rowIndex < tableLines.Count; rowIndex++)
                {
                    var rowCells = SplitTableRow(tableLines[rowIndex]);
                    sb.Append("<tr>");
                    for (int col = 0; col < columnCount; col++)
                    {
                        var cellText = col < rowCells.Count ? rowCells[col] : string.Empty;
                        sb.Append("<td");
                        var align = alignments[col];
                        if (!string.IsNullOrEmpty(align) && !string.Equals(align, "left", StringComparison.OrdinalIgnoreCase))
                        {
                            sb.Append(" style='text-align:").Append(align).Append("'>");
                        }
                        else
                        {
                            sb.Append(">");
                        }

                        sb.Append(ProcessInlineText(cellText.Trim()));
                        sb.Append("</td>");
                    }
                    sb.Append("</tr>");
                }
                sb.Append("</tbody>");
            }
            else
            {
                sb.Append("<tbody></tbody>");
            }

            sb.Append("</table>");
            return sb.ToString();
        }

        private List<string?>? ParseAlignmentRow(string line, int minimumColumns)
        {
            var separators = SplitTableRow(line);
            if (separators.All(string.IsNullOrWhiteSpace))
            {
                return null;
            }

            var alignments = new List<string?>();
            foreach (var separator in separators)
            {
                var trimmed = separator.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    alignments.Add(null);
                    continue;
                }

                if (!Regex.IsMatch(trimmed, @"^:?-+:?$"))
                {
                    return null;
                }

                bool startsColon = trimmed.StartsWith(":", StringComparison.Ordinal);
                bool endsColon = trimmed.EndsWith(":", StringComparison.Ordinal);

                if (startsColon && endsColon)
                {
                    alignments.Add("center");
                }
                else if (endsColon)
                {
                    alignments.Add("right");
                }
                else if (startsColon)
                {
                    alignments.Add("left");
                }
                else
                {
                    alignments.Add(null);
                }
            }

            while (alignments.Count < minimumColumns)
            {
                alignments.Add(null);
            }

            return alignments;
        }

        private List<string> SplitTableRow(string line)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("|", StringComparison.Ordinal))
            {
                trimmed = trimmed.Substring(1);
            }
            if (trimmed.EndsWith("|", StringComparison.Ordinal))
            {
                trimmed = trimmed.Substring(0, trimmed.Length - 1);
            }

            return trimmed.Split('|').Select(cell => cell).ToList();
        }

        private string ProtectEscapedCharacters(string text, out List<(string placeholder, string value)> replacements)
        {
            var list = new List<(string placeholder, string value)>();
            var result = Regex.Replace(
                text,
                @"\\([\\`*_{}\[\]()#+\-.!|>])",
                m =>
                {
                    var placeholder = $"__MISBAH_ESC_{list.Count}__";
                    list.Add((placeholder, m.Groups[1].Value));
                    return placeholder;
                });
            replacements = list;
            return result;
        }

        private string RestoreEscapedCharacters(string text, List<(string placeholder, string value)> replacements)
        {
            foreach (var replacement in replacements)
            {
                text = text.Replace(replacement.placeholder, replacement.value, StringComparison.Ordinal);
            }
            return text;
        }

        private bool TryRenderHeadingLine(string line, List<string> htmlLines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            var trimmedStart = line.TrimStart();
            if (!trimmedStart.StartsWith("#", StringComparison.Ordinal))
            {
                return false;
            }

            if (trimmedStart.StartsWith("\\#", StringComparison.Ordinal))
            {
                return false;
            }

            var match = Regex.Match(trimmedStart, @"^(#{1,6})\s+(.*)$");
            if (!match.Success)
            {
                return false;
            }

            var level = match.Groups[1].Length;
            var content = match.Groups[2].Value;
            var processedContent = ProcessInlineText(content.TrimEnd());
            htmlLines.Add($"<h{level}>{processedContent}</h{level}>");
            return true;
        }
    }
}
