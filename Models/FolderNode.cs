using System.Collections.Generic;

namespace Misbah.Models
{
    public class FolderNode
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public List<FolderNode> Folders { get; set; } = new List<FolderNode>();
        public List<Note> Notes { get; set; } = new List<Note>();
    }
}
