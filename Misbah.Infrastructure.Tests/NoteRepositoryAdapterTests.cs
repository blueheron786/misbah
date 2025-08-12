using Misbah.Core.Services;
using Misbah.Domain.Entities;
using Misbah.Infrastructure.Repositories;
using NSubstitute;

namespace Misbah.Infrastructure.Tests;

[TestFixture]
public class NoteRepositoryAdapterTests
{
    private INoteService _noteService = null!;
    private NoteRepositoryAdapter _adapter = null!;

    [SetUp]
    public void Setup()
    {
        _noteService = Substitute.For<INoteService>();
        _adapter = new NoteRepositoryAdapter(_noteService);
    }

    [Test]
    public async Task GetByIdAsync_ValidId_ReturnsNote()
    {
        // Arrange
        var noteId = "test-note";
        var coreNote = new Misbah.Core.Models.Note 
        { 
            Id = noteId, 
            Title = "Test Note", 
            Content = "# Test Content",
            LastModified = DateTime.UtcNow
        };
        
        _noteService.GetNote(noteId).Returns(coreNote);

        // Act
        var result = await _adapter.GetByIdAsync(noteId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(noteId));
        Assert.That(result.Title, Is.EqualTo("Test Note"));
        Assert.That(result.Content, Is.EqualTo("# Test Content"));
    }

    [Test]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Arrange
        var noteId = "non-existent";
        _noteService.GetNote(noteId).Returns((Misbah.Core.Models.Note?)null);

        // Act
        var result = await _adapter.GetByIdAsync(noteId);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task SaveAsync_ValidNote_CallsCoreServiceSave()
    {
        // Arrange
        var domainNote = new Note 
        { 
            Id = "test-note", 
            Title = "Test Note", 
            Content = "Updated content",
            LastModified = DateTime.UtcNow
        };

        // Act
        await _adapter.SaveAsync(domainNote);

        // Assert
        _noteService.Received(1).SaveNote(Arg.Is<Misbah.Core.Models.Note>(n => 
            n.Id == domainNote.Id && 
            n.Title == domainNote.Title && 
            n.Content == domainNote.Content));
    }

    [Test]
    public async Task SearchAsync_ValidQuery_ReturnsMatchingNotes()
    {
        // Arrange
        var query = "markdown";
        var coreNotes = new List<Misbah.Core.Models.Note>
        {
            new() { Id = "note1", Title = "Markdown Guide", Content = "# Markdown basics" },
            new() { Id = "note2", Title = "Other", Content = "This mentions markdown too" }
        };
        
        _noteService.SearchNotes(query).Returns(coreNotes);

        // Act
        var results = await _adapter.SearchAsync(query);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count(), Is.EqualTo(2));
        Assert.That(results.First().Title, Is.EqualTo("Markdown Guide"));
        Assert.That(results.Last().Content, Contains.Substring("markdown"));
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllNotes()
    {
        // Arrange
        var coreNotes = new List<Misbah.Core.Models.Note>
        {
            new() { Id = "note1", Title = "Note 1", Content = "Content 1" },
            new() { Id = "note2", Title = "Note 2", Content = "Content 2" },
            new() { Id = "note3", Title = "Note 3", Content = "Content 3" }
        };
        
        _noteService.GetAllNotes().Returns(coreNotes);

        // Act
        var results = await _adapter.GetAllAsync();

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count(), Is.EqualTo(3));
        
        var resultList = results.ToList();
        Assert.That(resultList[0].Id, Is.EqualTo("note1"));
        Assert.That(resultList[1].Id, Is.EqualTo("note2"));
        Assert.That(resultList[2].Id, Is.EqualTo("note3"));
    }
}
