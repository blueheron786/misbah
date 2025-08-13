using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Misbah.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing a file path in the note system
    /// </summary>
    public record NotePath
    {
        public string Value { get; }
        
        public NotePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be empty", nameof(path));
                
            if (!IsValidPath(path))
                throw new ArgumentException($"Invalid path format: {path}", nameof(path));
                
            Value = NormalizePath(path);
        }
        
        public string FileName => Path.GetFileName(Value);
        public string DirectoryPath => Path.GetDirectoryName(Value) ?? "";
        public string Extension => Path.GetExtension(Value);
        public bool IsMarkdownFile => Extension.Equals(".md", StringComparison.OrdinalIgnoreCase);
        
        private static bool IsValidPath(string path)
        {
            // Add validation logic for valid file paths
            return !string.IsNullOrEmpty(path) && !path.Contains("..");
        }
        
        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(path).Replace('\\', '/');
        }
        
        public static implicit operator string(NotePath notePath) => notePath.Value;
        public static implicit operator NotePath(string path) => new(path);
    }
}
