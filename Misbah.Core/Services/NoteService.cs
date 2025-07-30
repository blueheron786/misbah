using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Misbah.Core.Models;

namespace Misbah.Core.Services
{
    public class NoteService : INoteService
    {
        private readonly string _rootPath;
        public NoteService(string rootPath)
        {
            _rootPath = rootPath;
        }

        public IEnumerable<Note> GetAllNotes()
        {
            var notes = new List<Note>();
            foreach (var file in Directory.EnumerateFiles(_rootPath, "*.md", SearchOption.AllDirectories))
            {
                notes.Add(LoadNote(file));
            }
            return notes;
        }

        public Note LoadNote(string filePath)
        {
            var content = File.ReadAllText(filePath);
            var title = Path.GetFileNameWithoutExtension(filePath);
            var tags = ExtractTags(content);
            var info = new FileInfo(filePath);
            return new Note
            {
                Id = filePath,
                Title = title,
                Content = content,
                Tags = tags,
                Created = info.CreationTime,
                Modified = info.LastWriteTime,
                FilePath = filePath
            };
        }

        public void SaveNote(Note note)
        {
            File.WriteAllText(note.FilePath, note.Content);
        }

        public Note CreateNote(string folderPath, string title)
        {
            var filePath = Path.Combine(folderPath, title + ".md");
            var note = new Note
            {
                Id = filePath,
                Title = title,
                Content = "# " + title + "\n",
                Tags = new List<string>(),
                Created = DateTime.Now,
                Modified = DateTime.Now,
                FilePath = filePath
            };
            SaveNote(note);
            return note;
        }

        public void DeleteNote(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public List<string> ExtractTags(string content)
        {
            // Simple: #tag or tags in YAML frontmatter
            var tags = new HashSet<string>();
            var regex = new Regex(@"#(\w+)");
            foreach (Match match in regex.Matches(content))
            {
                tags.Add(match.Groups[1].Value);
            }
            // TODO: Add YAML frontmatter parsing if needed
            return tags.ToList();
        }
    }
}
