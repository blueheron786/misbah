using Misbah.Core.Services;
using Misbah.Infrastructure.Repositories;
using NSubstitute;
using CoreNote = Misbah.Core.Models.Note;
using DomainNote = Misbah.Domain.Entities.Note;

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
    public async Task GetNoteAsync_ValidFilePath_ReturnsNote()
    {
        // Arrange
        var filePath = "test-note.md";
        var coreNote = new CoreNote 
        { 
            Id = "test-note", 
            Title = "Test Note", 
            Content = "# Test Content",
            Modified = DateTime.UtcNow,
            FilePath = filePath
        };
        
        _noteService.LoadNote(filePath).Returns(coreNote);

        // Act
        var result = await _adapter.GetNoteAsync(filePath);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo("test-note"));
        Assert.That(result.Title, Is.EqualTo("Test Note"));
        Assert.That(result.Content, Is.EqualTo("# Test Content"));
        Assert.That(result.FilePath, Is.EqualTo(filePath));
    }

    [Test]
    public void GetNote_ValidFilePath_ReturnsNote()
    {
        // Arrange
        var filePath = "test-note.md";
        var coreNote = new CoreNote 
        { 
            Id = "test-note", 
            Title = "Test Note", 
            Content = "# Test Content",
            FilePath = filePath,
            Modified = DateTime.UtcNow
        };
        
        _noteService.LoadNote(filePath).Returns(coreNote);

        // Act
        var result = _adapter.GetNote(filePath);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo("test-note"));
        Assert.That(result.Title, Is.EqualTo("Test Note"));
    }

    [Test]
    public async Task SaveNoteAsync_ValidNote_CallsCoreServiceSave()
    {
        // Arrange
        var domainNote = new DomainNote 
        { 
            Id = "test-note", 
            Title = "Test Note", 
            Content = "Updated content",
            Modified = DateTime.UtcNow
        };

        // Act
        await _adapter.SaveNoteAsync(domainNote);

        // Assert
        await _noteService.Received(1).SaveNoteAsync(Arg.Is<CoreNote>(n => 
            n.Id == domainNote.Id && 
            n.Title == domainNote.Title && 
            n.Content == domainNote.Content));
    }

    [Test]
    public void GetAllNotes_ReturnsAllNotes()
    {
        // Arrange
        var coreNotes = new List<CoreNote>
        {
            new() { Id = "note1", Title = "Note 1", Content = "Content 1", FilePath = "note1.md" },
            new() { Id = "note2", Title = "Note 2", Content = "Content 2", FilePath = "note2.md" },
            new() { Id = "note3", Title = "Note 3", Content = "Content 3", FilePath = "note3.md" }
        };
        
        _noteService.GetAllNotes().Returns(coreNotes);

        // Act
        var results = _adapter.GetAllNotes();

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count(), Is.EqualTo(3));
        
        var resultList = results.ToList();
        Assert.That(resultList[0].Id, Is.EqualTo("note1"));
        Assert.That(resultList[1].Id, Is.EqualTo("note2"));
        Assert.That(resultList[2].Id, Is.EqualTo("note3"));
    }

    [Test]
    public async Task CreateNoteAsync_ValidParameters_CallsCoreService()
    {
        // Arrange
        var folderPath = "/notes";
        var title = "New Note";
        var expectedCoreNote = new CoreNote
        {
            Id = "new-note",
            Title = title,
            Content = "",
            FilePath = "/notes/new-note.md",
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow
        };
        
        _noteService.CreateNoteAsync(folderPath, title).Returns(expectedCoreNote);

        // Act
        var result = await _adapter.CreateNoteAsync(folderPath, title);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo(title));
        await _noteService.Received(1).CreateNoteAsync(folderPath, title);
    }
}
