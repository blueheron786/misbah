using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Misbah.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing markdown content with parsing capabilities
    /// </summary>
    public record MarkdownContent
    {
        private static readonly Regex TagPattern = new(@"#(\w+)", RegexOptions.Compiled);
        private static readonly Regex LinkPattern = new(@"\[\[([^\]]+)\]\]", RegexOptions.Compiled);
        
        public string RawContent { get; }
        
        public MarkdownContent(string content)
        {
            RawContent = content ?? string.Empty;
        }
        
        public IReadOnlyList<string> ExtractTags()
        {
            return TagPattern.Matches(RawContent)
                .Select(m => m.Groups[1].Value.ToLowerInvariant())
                .Distinct()
                .OrderBy(tag => tag)
                .ToList();
        }
        
        public IReadOnlyList<string> ExtractWikiLinks()
        {
            return LinkPattern.Matches(RawContent)
                .Select(m => m.Groups[1].Value.Trim())
                .Distinct()
                .ToList();
        }
        
        public string ExtractTitle()
        {
            var lines = RawContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var firstLine = lines.FirstOrDefault()?.Trim() ?? "";
            
            // Check for markdown heading
            if (firstLine.StartsWith("# "))
                return firstLine.Substring(2).Trim();
                
            // Check for YAML frontmatter title
            if (RawContent.StartsWith("---"))
            {
                var frontMatterEnd = RawContent.IndexOf("\n---", StringComparison.Ordinal);
                if (frontMatterEnd > 0)
                {
                    var frontMatter = RawContent.Substring(0, frontMatterEnd);
                    var titleMatch = Regex.Match(frontMatter, @"title:\s*(.+)", RegexOptions.IgnoreCase);
                    if (titleMatch.Success)
                        return titleMatch.Groups[1].Value.Trim().Trim('"', '\'');
                }
            }
            
            return "";
        }
        
        public int WordCount => RawContent.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).Length;
        public int CharacterCount => RawContent.Length;
        public bool IsEmpty => string.IsNullOrWhiteSpace(RawContent);
        
        public static implicit operator string(MarkdownContent content) => content.RawContent;
        public static implicit operator MarkdownContent(string content) => new(content);
    }
}
