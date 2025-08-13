using NUnit.Framework;
using NSubstitute;
using Misbah.Application.Handlers.Queries;
using Misbah.Application.Queries.Notes;
using Misbah.Domain.Entities;
using Misbah.Domain.Interfaces;
using Misbah.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Misbah.Application.Tests.Handlers.Queries
{
    [TestFixture]
    public class GetAllNotesQueryHandlerTests
    {
        private INoteRepository _mockRepository;
        private GetAllNotesQueryHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockRepository = Substitute.For<INoteRepository>();
            _handler = new GetAllNotesQueryHandler(_mockRepository);
        }

        [Test]
        public async Task HandleAsync_NoFilter_ReturnsAllNotes()
        {
            // Arrange
            var notes = new[]
            {
                Note.CreateNew("Note 1", @"C:\test", "Content 1"),
                Note.CreateNew("Note 2", @"C:\test", "Content 2")
            };
            
            _mockRepository.GetAllNotes().Returns(notes);
            var query = new GetAllNotesQuery();

            // Act
            var result = await _handler.HandleAsync(query);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().Title, Is.EqualTo("Note 1"));
        }

        [Test]
        public async Task HandleAsync_WithTagFilter_ReturnsFilteredNotes()
        {
            // Arrange
            var notes = new[]
            {
                Note.CreateNew("Note 1", @"C:\test", "Content with #work tag"),
                Note.CreateNew("Note 2", @"C:\test", "Content with #personal tag")
            };
            
            _mockRepository.GetAllNotes().Returns(notes);
            var query = new GetAllNotesQuery(tagFilter: "work");

            // Act
            var result = await _handler.HandleAsync(query);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Title, Is.EqualTo("Note 1"));
        }
    }

    [TestFixture]
    public class GetNoteByIdQueryHandlerTests
    {
        private INoteRepository _mockRepository;
        private GetNoteByIdQueryHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockRepository = Substitute.For<INoteRepository>();
            _handler = new GetNoteByIdQueryHandler(_mockRepository);
        }

        [Test]
        public async Task HandleAsync_ExistingNote_ReturnsNote()
        {
            // Arrange
            var noteId = "test-note-id";
            var note = Note.CreateNew("Test Note", @"C:\test", "Content");
            
            _mockRepository.GetNoteByIdAsync(noteId).Returns(note);
            var query = new GetNoteByIdQuery(noteId);

            // Act
            var result = await _handler.HandleAsync(query);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Title, Is.EqualTo("Test Note"));
        }

        [Test]
        public async Task HandleAsync_NonExistentNote_ReturnsNull()
        {
            // Arrange
            var noteId = "non-existent";
            _mockRepository.GetNoteByIdAsync(noteId).Returns((Note?)null);
            var query = new GetNoteByIdQuery(noteId);

            // Act
            var result = await _handler.HandleAsync(query);

            // Assert
            Assert.That(result, Is.Null);
        }
    }

    [TestFixture]
    public class SearchNotesQueryHandlerTests
    {
        private INoteRepository _mockRepository;
        private SearchNotesQueryHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockRepository = Substitute.For<INoteRepository>();
            _handler = new SearchNotesQueryHandler(_mockRepository);
        }

        [Test]
        public async Task HandleAsync_ValidSearchTerm_ReturnsMatchingNotes()
        {
            // Arrange
            var notes = new[]
            {
                Note.CreateNew("Important Note", @"C:\test", "This is important content"),
                Note.CreateNew("Other Note", @"C:\test", "This is other content")
            };

            _mockRepository.GetAllNotes().Returns(notes);
            
            var query = new SearchNotesQuery("important");

            // Act
            var result = await _handler.HandleAsync(query);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Title, Is.EqualTo("Important Note"));
        }

        [Test]
        public async Task HandleAsync_EmptySearchTerm_ReturnsEmptyCollection()
        {
            // Arrange
            var query = new SearchNotesQuery("");

            // Act
            var result = await _handler.HandleAsync(query);

            // Assert
            Assert.That(result, Is.Empty);
        }
    }

    [TestFixture]
    public class GetNotesByTagQueryHandlerTests
    {
        private INoteRepository _mockRepository;
        private GetNotesByTagQueryHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockRepository = Substitute.For<INoteRepository>();
            _handler = new GetNotesByTagQueryHandler(_mockRepository);
        }

        [Test]
        public async Task HandleAsync_ValidTag_ReturnsNotesWithTag()
        {
            // Arrange
            var notes = new[]
            {
                Note.CreateNew("Note 1", @"C:\test", "Content with #work"),
                Note.CreateNew("Note 2", @"C:\test", "Content with #personal")
            };

            _mockRepository.GetAllNotes().Returns(notes);
            
            var query = new GetNotesByTagQuery("work");

            // Act
            var result = await _handler.HandleAsync(query);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().ExtractedTags, Contains.Item("work"));
        }
    }

    [TestFixture]
    public class GetAllTagsQueryHandlerTests
    {
        private INoteRepository _mockRepository;
        private GetAllTagsQueryHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockRepository = Substitute.For<INoteRepository>();
            _handler = new GetAllTagsQueryHandler(_mockRepository);
        }

        [Test]
        public async Task HandleAsync_NotesExist_ReturnsAllTags()
        {
            // Arrange
            var notes = new[]
            {
                Note.CreateNew("Note 1", @"C:\test", "Content with #work and #project"),
                Note.CreateNew("Note 2", @"C:\test", "Content with #personal")
            };
            
            _mockRepository.GetAllNotes().Returns(notes);
            
            var query = new GetAllTagsQuery();

            // Act
            var result = await _handler.HandleAsync(query);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(3));
            Assert.That(result, Contains.Item("work"));
            Assert.That(result, Contains.Item("personal"));
            Assert.That(result, Contains.Item("project"));
        }

        [Test]
        public async Task HandleAsync_NoNotes_ReturnsEmptyTags()
        {
            // Arrange
            _mockRepository.GetAllNotes().Returns(Array.Empty<Note>());
            var query = new GetAllTagsQuery();

            // Act
            var result = await _handler.HandleAsync(query);

            // Assert
            Assert.That(result, Is.Empty);
        }
    }
}
