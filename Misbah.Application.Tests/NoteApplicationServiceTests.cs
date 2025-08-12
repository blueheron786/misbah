using Misbah.Application.Services;
using Misbah.Domain.Entities;
using Misbah.Domain.Interfaces;
using NSubstitute;

namespace Misbah.Application.Tests;

[TestFixture]
public class NoteApplicationServiceTests
{
    private INoteRepository _noteRepository = null!;
    private NoteApplicationService _service = null!;

    [SetUp]
    public void Setup()
    {
        _noteRepository = Substitute.For<INoteRepository>();
        _service = new NoteApplicationService(_noteRepository);
    }

    [Test]
    public async Task LoadNoteAsync_ValidFilePath_ReturnsNote()
    {
        // Arrange
        var filePath = "test-note.md";
        var expectedNote = new Note 
        { 
            Id = "test-note", 
            Title = "Test Note", 
            Content = "# Test Content\n\nThis is a test note with **bold** text.",
            Modified = DateTime.UtcNow,
            FilePath = filePath
        };
        
        _noteRepository.GetNoteAsync(filePath).Returns(expectedNote);

        // Act
        var result = await _service.LoadNoteAsync(filePath);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo("test-note"));
        Assert.That(result.Title, Is.EqualTo("Test Note"));
        Assert.That(result.Content, Contains.Substring("**bold**"));
        Assert.That(result.FilePath, Is.EqualTo(filePath));
    }

    [Test]
    public void LoadNote_ValidFilePath_ReturnsNote()
    {
        // Arrange
        var filePath = "test-note.md";
        var expectedNote = new Note 
        { 
            Id = "test-note", 
            Title = "Test Note", 
            Content = "# Test Content",
            Modified = DateTime.UtcNow,
            FilePath = filePath
        };
        
        _noteRepository.GetNote(filePath).Returns(expectedNote);

        // Act
        var result = _service.LoadNote(filePath);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo("test-note"));
        Assert.That(result.Title, Is.EqualTo("Test Note"));
        Assert.That(result.Content, Is.EqualTo("# Test Content"));
    }

    [Test]
    public async Task SaveNoteAsync_ValidNote_CallsRepositorySave()
    {
        // Arrange
        var note = new Note 
        { 
            Id = "test-note", 
            Title = "Test Note", 
            Content = "Updated content",
            Modified = DateTime.UtcNow,
            FilePath = "test-note.md"
        };

        // Act
        await _service.SaveNoteAsync(note);

        // Assert
        await _noteRepository.Received(1).SaveNoteAsync(note);
    }

    [Test]
    public async Task CreateNoteAsync_ValidParameters_ReturnsNewNote()
    {
        // Arrange
        var folderPath = "/notes";
        var title = "New Note";
        var expectedNote = new Note 
        { 
            Id = "new-note", 
            Title = title, 
            Content = "",
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            FilePath = "/notes/new-note.md"
        };
        
        _noteRepository.CreateNoteAsync(folderPath, title).Returns(expectedNote);

        // Act
        var result = await _service.CreateNoteAsync(folderPath, title);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo(title));
        Assert.That(result.FilePath, Contains.Substring(folderPath));
    }

    [Test]
    public void ExtractTags_ContentWithTags_ReturnsTagList()
    {
        // Arrange
        var content = "This is content with #tag1 and #tag2 and #anothertag";

        // Act
        var result = _service.ExtractTags(content);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Contains.Item("tag1"));
        Assert.That(result, Contains.Item("tag2"));  
        Assert.That(result, Contains.Item("anothertag"));
    }

    [Test]
    public void ExtractTags_TagsWithHyphens_OnlyExtractsWordCharacters()
    {
        // Arrange - this tests the actual regex behavior
        var content = "Content with #word-tag and #under_score";

        // Act
        var result = _service.ExtractTags(content);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Contains.Item("word")); // hyphen stops the match
        Assert.That(result, Contains.Item("under_score")); // underscore is allowed in \w+
    }

    [Test]
    public void GetAllNotes_ReturnsAllNotes()
    {
        // Arrange
        var expectedNotes = new List<Note>
        {
            new() { Id = "note1", Title = "Note 1", Content = "Content 1" },
            new() { Id = "note2", Title = "Note 2", Content = "Content 2" }
        };
        
        _noteRepository.GetAllNotes().Returns(expectedNotes);

        // Act
        var results = _service.GetAllNotes();

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count(), Is.EqualTo(2));
    }
}
