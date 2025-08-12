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
    public async Task GetNoteAsync_ValidId_ReturnsNote()
    {
        // Arrange
        var noteId = "test-note";
        var expectedNote = new Note 
        { 
            Id = noteId, 
            Title = "Test Note", 
            Content = "# Test Content\n\nThis is a test note with **bold** text.",
            LastModified = DateTime.UtcNow
        };
        
        _noteRepository.GetByIdAsync(noteId).Returns(expectedNote);

        // Act
        var result = await _service.GetNoteAsync(noteId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(noteId));
        Assert.That(result.Title, Is.EqualTo("Test Note"));
        Assert.That(result.Content, Contains.Substring("**bold**"));
    }

    [Test]
    public async Task GetNoteAsync_NonExistentId_ReturnsNull()
    {
        // Arrange
        var noteId = "non-existent";
        _noteRepository.GetByIdAsync(noteId).Returns((Note?)null);

        // Act
        var result = await _service.GetNoteAsync(noteId);

        // Assert
        Assert.That(result, Is.Null);
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
            LastModified = DateTime.UtcNow
        };

        // Act
        await _service.SaveNoteAsync(note);

        // Assert
        await _noteRepository.Received(1).SaveAsync(note);
    }

    [Test]
    public async Task SearchNotesAsync_ValidQuery_ReturnsMatchingNotes()
    {
        // Arrange
        var query = "markdown";
        var expectedNotes = new List<Note>
        {
            new Note { Id = "note1", Title = "Markdown Guide", Content = "# Markdown basics" },
            new Note { Id = "note2", Title = "Other", Content = "This mentions markdown too" }
        };
        
        _noteRepository.SearchAsync(query).Returns(expectedNotes);

        // Act
        var results = await _service.SearchNotesAsync(query);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count(), Is.EqualTo(2));
        Assert.That(results.First().Title, Is.EqualTo("Markdown Guide"));
    }

    [Test]
    public async Task SearchNotesAsync_EmptyQuery_ReturnsEmptyResults()
    {
        // Arrange
        var query = "";
        _noteRepository.SearchAsync(query).Returns(new List<Note>());

        // Act
        var results = await _service.SearchNotesAsync(query);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count(), Is.EqualTo(0));
    }
}
