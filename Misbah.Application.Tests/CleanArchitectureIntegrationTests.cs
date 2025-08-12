using Microsoft.Extensions.DependencyInjection;
using Misbah.Application.Interfaces;
using Misbah.Application.Services;
using Misbah.Core.Services;
using Misbah.Domain.Interfaces;
using Misbah.Infrastructure.Repositories;
using NSubstitute;

namespace Misbah.Application.Tests;

[TestFixture]
public class CleanArchitectureIntegrationTests
{
    [Test]
    public void ServiceRegistration_ShouldResolveCleanArchitectureServices()
    {
        // Arrange - Set up DI container just like in the real app
        var services = new ServiceCollection();
        
        // Register Core services (legacy)
        services.AddSingleton<INoteService>(sp => Substitute.For<INoteService>());
        
        // Register Clean Architecture services (new)
        services.AddScoped<INoteRepository, NoteRepositoryAdapter>();
        services.AddScoped<INoteApplicationService, NoteApplicationService>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Act & Assert - Verify all services can be resolved
        Assert.DoesNotThrow(() => serviceProvider.GetRequiredService<INoteService>(), 
            "Legacy INoteService should be resolvable");
        
        Assert.DoesNotThrow(() => serviceProvider.GetRequiredService<INoteRepository>(), 
            "INoteRepository should be resolvable");
        
        Assert.DoesNotThrow(() => serviceProvider.GetRequiredService<INoteApplicationService>(), 
            "INoteApplicationService should be resolvable");
    }
    
    [Test]
    public void CleanArchitectureServices_ShouldWorkTogether()
    {
        // Arrange
        var mockLegacyService = Substitute.For<INoteService>();
        var coreNote = new Misbah.Core.Models.Note
        {
            Id = "test-note",
            Title = "Test Note",
            Content = "# Test Content",
            Modified = DateTime.Now,
            FilePath = "test.md"
        };
        
        mockLegacyService.GetAllNotes().Returns([coreNote]);
        
        var repository = new NoteRepositoryAdapter(mockLegacyService);
        var applicationService = new NoteApplicationService(repository);
        
        // Act
        var notes = applicationService.GetAllNotes();
        var notesList = notes.ToList();
        
        // Assert
        Assert.That(notesList, Has.Count.EqualTo(1));
        Assert.That(notesList[0].Id, Is.EqualTo("test-note"));
        Assert.That(notesList[0].Title, Is.EqualTo("Test Note"));
        Assert.That(notesList[0].Content, Is.EqualTo("# Test Content"));
    }
    
    [Test]
    public async Task CleanArchitectureServices_AsyncMethods_ShouldWork()
    {
        // Arrange
        var mockLegacyService = Substitute.For<INoteService>();
        var coreNote = new Misbah.Core.Models.Note
        {
            Id = "async-test",
            Title = "Async Test Note",
            Content = "# Async Content",
            Modified = DateTime.Now,
            FilePath = "async-test.md"
        };
        
        mockLegacyService.LoadNote("async-test.md").Returns(coreNote);
        mockLegacyService.GetAllNotes().Returns([coreNote]);
        
        var repository = new NoteRepositoryAdapter(mockLegacyService);
        var applicationService = new NoteApplicationService(repository);
        
        // Act & Assert - Test async methods
        var loadedNote = await applicationService.LoadNoteAsync("async-test.md");
        Assert.That(loadedNote.Id, Is.EqualTo("async-test"));
        
        var allNotes = await applicationService.GetAllNotesAsync();
        Assert.That(allNotes.Count(), Is.EqualTo(1));
    }
    
    [Test]
    public void AdapterPattern_ShouldTransformDataCorrectly()
    {
        // Arrange
        var mockLegacyService = Substitute.For<INoteService>();
        var coreNote = new Misbah.Core.Models.Note
        {
            Id = "transform-test",
            Title = "Transform Test",
            Content = "Content with #tag1 and #tag2",
            Modified = new DateTime(2023, 1, 1),
            FilePath = "transform.md"
        };
        
        mockLegacyService.LoadNote("transform.md").Returns(coreNote);
        
        var adapter = new NoteRepositoryAdapter(mockLegacyService);
        
        // Act
        var domainNote = adapter.GetNote("transform.md");
        
        // Assert - Verify transformation from Core.Note to Domain.Note
        Assert.That(domainNote.Id, Is.EqualTo(coreNote.Id));
        Assert.That(domainNote.Title, Is.EqualTo(coreNote.Title));
        Assert.That(domainNote.Content, Is.EqualTo(coreNote.Content));
        Assert.That(domainNote.Modified, Is.EqualTo(coreNote.Modified));
        Assert.That(domainNote.FilePath, Is.EqualTo(coreNote.FilePath));
        
        // Verify tags are extracted
        Assert.That(domainNote.Tags, Has.Count.EqualTo(2));
        Assert.That(domainNote.Tags, Contains.Item("tag1"));
        Assert.That(domainNote.Tags, Contains.Item("tag2"));
    }
    
    [Test]
    public void BusinessLogic_TagExtraction_ShouldWorkInApplicationLayer()
    {
        // Arrange
        var mockRepository = Substitute.For<INoteRepository>();
        var applicationService = new NoteApplicationService(mockRepository);
        
        var content = "This content has #important and #urgent tags in it.";
        
        // Act
        var tags = applicationService.ExtractTags(content);
        
        // Assert
        Assert.That(tags, Has.Count.EqualTo(2));
        Assert.That(tags, Contains.Item("important"));
        Assert.That(tags, Contains.Item("urgent"));
    }
}
