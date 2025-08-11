using Xunit;
using Misbah.Infrastructure.Persistence.Repositories;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace Misbah.Infrastructure.Tests
{
    public class NoteRepositoryTests
    {
        [Fact]
        public async Task GetAllAsync_ShouldReturnJustFilenames()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), "MisbahTest", Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                // Create test markdown files
                var file1 = Path.Combine(tempDir, "test-note.md");
                var file2 = Path.Combine(tempDir, "another-note.md");
                var file3 = Path.Combine(tempDir, "note-with-spaces.md");
                
                await File.WriteAllTextAsync(file1, "# Test Note\n\nThis is a test.");
                await File.WriteAllTextAsync(file2, "# Another Note\n\nAnother test.");
                await File.WriteAllTextAsync(file3, "# Note with Spaces\n\nSpaces test.");

                var repository = new NoteRepository(tempDir);

                // Act
                var notes = await repository.GetAllAsync();

                // Assert
                Assert.Equal(3, notes.Count());
                
                var notesList = notes.ToList();
                
                // Verify IDs are just the filename without path or extension
                Assert.Contains(notesList, n => n.Id == "test-note");
                Assert.Contains(notesList, n => n.Id == "another-note");
                Assert.Contains(notesList, n => n.Id == "note-with-spaces");
                
                // Verify titles come from markdown headers
                Assert.Contains(notesList, n => n.Title == "Test Note");
                Assert.Contains(notesList, n => n.Title == "Another Note");
                Assert.Contains(notesList, n => n.Title == "Note with Spaces");
                
                // Verify no full paths in IDs or titles
                Assert.All(notesList, note => 
                {
                    Assert.DoesNotContain(tempDir, note.Id);
                    Assert.DoesNotContain(".md", note.Id);
                    Assert.DoesNotContain("\\", note.Id);
                    Assert.DoesNotContain("/", note.Id);
                    Assert.DoesNotContain(tempDir, note.Title);
                    Assert.DoesNotContain(".md", note.Title);
                });
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }
    }
}
