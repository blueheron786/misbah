using System;
using System.Collections.Generic;

namespace Misbah.Core.Models
{
    public class Note
    {
        public required string Id { get; set; } // Unique identifier, e.g. file path
        public required string Title { get; set; }
        public required string Content { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public required string FilePath { get; set; }
    }
}
