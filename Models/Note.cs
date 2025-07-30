using System;
using System.Collections.Generic;

namespace Misbah.Models
{
    public class Note
    {
        public string Id { get; set; } // Unique identifier, e.g. file path
        public string Title { get; set; }
        public string Content { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string FilePath { get; set; }
    }
}
