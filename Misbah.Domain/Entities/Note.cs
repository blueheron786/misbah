using System;
using System.Collections.Generic;

namespace Misbah.Domain.Entities
{
    public class Note
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }
}
