using NUnit.Framework;
using Misbah.Domain.Entities;
using Misbah.Domain.ValueObjects;
using Misbah.Domain.Events;
using System;
using System.Linq;

namespace Misbah.Application.Tests.Domain
{
    [TestFixture]
    public class NoteEntityTests
    {
        [Test]
        public void CreateNew_ValidParameters_CreatesNoteWithEvents()
        {
            // Arrange & Act
            var note = Note.CreateNew("Test Note", @"C:\docs", "# Test Content");

            // Assert
            Assert.That(note.Title, Is.EqualTo("Test Note"));
            Assert.That(note.Content.RawContent, Is.EqualTo("# Test Content"));
            Assert.That(note.FilePath, Does.StartWith(@"C:\docs"));
            Assert.That(note.FilePath, Does.EndWith(".md"));
            Assert.That(note.Id, Is.Not.Empty);
            Assert.That(note.Created, Is.EqualTo(note.Modified).Within(TimeSpan.FromSeconds(1)));
            
            // Check domain events
            Assert.That(note.DomainEvents.Count, Is.EqualTo(1));
            Assert.That(note.DomainEvents.First(), Is.TypeOf<NoteCreated>());
            
            var createdEvent = (NoteCreated)note.DomainEvents.First();
            Assert.That(createdEvent.NoteId, Is.EqualTo(note.Id));
            Assert.That(createdEvent.Title, Is.EqualTo("Test Note"));
            Assert.That(createdEvent.FilePath, Is.EqualTo(note.FilePath));
        }

        [Test]
        public void CreateNew_EmptyContent_CreatesNoteWithEmptyContent()
        {
            // Arrange & Act
            var note = Note.CreateNew("Test Note", @"C:\docs");

            // Assert
            Assert.That(note.Content.IsEmpty, Is.True);
            Assert.That(note.Content.RawContent, Is.EqualTo(""));
        }

        [Test]
        public void CreateNew_GeneratesUniqueIds()
        {
            // Arrange & Act
            var note1 = Note.CreateNew("Note 1", @"C:\docs");
            var note2 = Note.CreateNew("Note 2", @"C:\docs");

            // Assert
            Assert.That(note1.Id, Is.Not.EqualTo(note2.Id));
        }

        [Test]
        public void FromExisting_ValidParameters_CreatesNoteWithoutEvents()
        {
            // Arrange
            var id = "existing-id";
            var title = "Existing Note";
            var filePath = @"C:\docs\existing.md";
            var content = "# Existing Content";
            var created = DateTime.UtcNow.AddDays(-1);
            var modified = DateTime.UtcNow;

            // Act
            var note = Note.FromExisting(id, title, filePath, content, created, modified);

            // Assert
            Assert.That(note.Id, Is.EqualTo(id));
            Assert.That(note.Title, Is.EqualTo(title));
            Assert.That(note.FilePath, Is.EqualTo(filePath));
            Assert.That(note.Content.RawContent, Is.EqualTo(content));
            Assert.That(note.Created, Is.EqualTo(created));
            Assert.That(note.Modified, Is.EqualTo(modified));
            Assert.That(note.DomainEvents, Is.Empty);
        }

        [Test]
        public void UpdateContent_ValidContent_UpdatesContentAndTime()
        {
            // Arrange
            var note = Note.CreateNew("Test Note", @"C:\docs", "# Original");
            var originalModified = note.Modified;
            note.ClearDomainEvents(); // Clear creation events

            // Wait a bit to ensure different timestamp
            System.Threading.Thread.Sleep(10);

            // Act
            note.UpdateContent("# Updated Content");

            // Assert
            Assert.That(note.Content.RawContent, Is.EqualTo("# Updated Content"));
            Assert.That(note.Modified, Is.GreaterThan(originalModified));
            Assert.That(note.HasBeenModified, Is.True);
        }

        [Test]
        public void UpdateContent_ContentWithTitle_UpdatesTitleAndRaisesEvent()
        {
            // Arrange
            var note = Note.CreateNew("Original Title", @"C:\docs", "# Original");
            note.ClearDomainEvents();

            // Act
            note.UpdateContent("# New Title\nSome content");

            // Assert
            Assert.That(note.Title, Is.EqualTo("New Title"));
            Assert.That(note.DomainEvents.Count, Is.EqualTo(1));
            Assert.That(note.DomainEvents.First(), Is.TypeOf<NoteUpdated>());

            var updatedEvent = (NoteUpdated)note.DomainEvents.First();
            Assert.That(updatedEvent.PreviousTitle, Is.EqualTo("Original Title"));
            Assert.That(updatedEvent.NewTitle, Is.EqualTo("New Title"));
        }

        [Test]
        public void UpdateTitle_ValidTitle_UpdatesTitle()
        {
            // Arrange
            var note = Note.CreateNew("Original Title", @"C:\docs");
            note.ClearDomainEvents();

            // Act
            note.UpdateTitle("New Title");

            // Assert
            Assert.That(note.Title, Is.EqualTo("New Title"));
            Assert.That(note.DomainEvents.Count, Is.EqualTo(1));
            Assert.That(note.DomainEvents.First(), Is.TypeOf<NoteUpdated>());
        }

        [Test]
        public void UpdateTitle_EmptyTitle_ThrowsArgumentException()
        {
            // Arrange
            var note = Note.CreateNew("Original Title", @"C:\docs");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => note.UpdateTitle(""));
            Assert.Throws<ArgumentException>(() => note.UpdateTitle("   "));
        }

        [Test]
        public void ExtractedTags_ContentWithTags_ExtractsCorrectly()
        {
            // Arrange
            var note = Note.CreateNew("Note", @"C:\docs", "Content with #work and #personal tags");

            // Act
            var tags = note.ExtractedTags;

            // Assert
            Assert.That(tags.Count, Is.EqualTo(2));
            Assert.That(tags, Contains.Item("work"));
            Assert.That(tags, Contains.Item("personal"));
        }

        [Test]
        public void WikiLinks_ContentWithLinks_ExtractsCorrectly()
        {
            // Arrange
            var note = Note.CreateNew("Note", @"C:\docs", "Link to [[Page One]] and [[Page Two]]");

            // Act
            var links = note.WikiLinks;

            // Assert
            Assert.That(links.Count, Is.EqualTo(2));
            Assert.That(links, Contains.Item("Page One"));
            Assert.That(links, Contains.Item("Page Two"));
        }

        [Test]
        public void WordCount_ContentWithWords_CountsCorrectly()
        {
            // Arrange
            var note = Note.CreateNew("Note", @"C:\docs", "This has exactly five words");

            // Act
            var count = note.WordCount;

            // Assert
            Assert.That(count, Is.EqualTo(5));
        }

        [Test]
        public void IsEmpty_EmptyContent_ReturnsTrue()
        {
            // Arrange
            var note = Note.CreateNew("Note", @"C:\docs", "");

            // Act & Assert
            Assert.That(note.IsEmpty, Is.True);
        }

        [Test]
        public void IsEmpty_ContentWithText_ReturnsFalse()
        {
            // Arrange
            var note = Note.CreateNew("Note", @"C:\docs", "Some content");

            // Act & Assert
            Assert.That(note.IsEmpty, Is.False);
        }

        [Test]
        public void ClearDomainEvents_RemovesAllEvents()
        {
            // Arrange
            var note = Note.CreateNew("Test Note", @"C:\docs", "Content");
            Assert.That(note.DomainEvents.Count, Is.GreaterThan(0));

            // Act
            note.ClearDomainEvents();

            // Assert
            Assert.That(note.DomainEvents, Is.Empty);
        }

        [Test]
        public void HasBeenModified_NewNote_ReturnsFalse()
        {
            // Arrange & Act
            var note = Note.CreateNew("Test Note", @"C:\docs", "Content");

            // Assert
            Assert.That(note.HasBeenModified, Is.False);
        }

        [Test]
        public void HasBeenModified_AfterUpdate_ReturnsTrue()
        {
            // Arrange
            var note = Note.CreateNew("Test Note", @"C:\docs", "Content");

            // Act
            note.UpdateContent("Updated content");

            // Assert
            Assert.That(note.HasBeenModified, Is.True);
        }
    }
}
