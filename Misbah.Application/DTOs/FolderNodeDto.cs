using System.Collections.Generic;

namespace Misbah.Application.DTOs
{
    public class FolderNodeDto
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public List<FolderNodeDto> Folders { get; set; } = new List<FolderNodeDto>();
        public List<NoteDto> Notes { get; set; } = new List<NoteDto>();
    }
}
