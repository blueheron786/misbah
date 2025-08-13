using System.Collections.Generic;

namespace Misbah.Core.Models
{
    public class FolderNode
    {
        public required string Name { get; set; }
        public required string Path { get; set; }
        public List<FolderNode> Folders { get; set; } = new List<FolderNode>();
        public List<Note> Notes { get; set; } = new List<Note>();
    }
}
