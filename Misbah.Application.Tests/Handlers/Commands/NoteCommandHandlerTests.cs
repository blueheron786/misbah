using NUnit.Framework;
using NSubstitute;
using Misbah.Application.Handlers.Commands;
using Misbah.Application.Commands.Notes;
using Misbah.Domain.Entities;
using Misbah.Domain.Interfaces;
using Misbah.Domain.ValueObjects;
using Misbah.Domain.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Misbah.Application.Tests.Handlers.Commands
{
    [TestFixture]
    public class CreateNoteCommandHandlerTests
    {
        private INoteRepository _mockRepository;
        private IDomainEventDispatcher _mockEventDispatcher;
        private CreateNoteCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockRepository = Substitute.For<INoteRepository>();
            _mockEventDispatcher = Substitute.For<IDomainEventDispatcher>();
            _handler = new CreateNoteCommandHandler(_mockRepository, _mockEventDispatcher);
        }

        [Test]
        public async Task HandleAsync_ValidCommand_CreatesNoteSuccessfully()
        {
            // Arrange
            var command = new CreateNoteCommand(
                "Test Note",
                @"C:\test",
                "# Test Content"
            );

            // Act
            var result = await _handler.HandleAsync(command);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Title, Is.EqualTo("Test Note"));
            Assert.That(result.Content.RawContent, Is.EqualTo("# Test Content"));
            
            await _mockRepository.Received(1).SaveNoteAsync(Arg.Any<Note>());
            await _mockEventDispatcher.Received(1).DispatchAsync(Arg.Any<IReadOnlyList<DomainEvent>>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void HandleAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var command = new CreateNoteCommand(
                "Test Note", 
                @"C:\test",
                "# Test Content"
            );

            _mockRepository.When(x => x.SaveNoteAsync(Arg.Any<Note>())).Do(x => throw new InvalidOperationException("Repository error"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.HandleAsync(command));
            Assert.That(ex.Message, Is.EqualTo("Repository error"));
        }

        [Test]
        public async Task HandleAsync_ValidCommand_DispatchesDomainEvents()
        {
            // Arrange
            var command = new CreateNoteCommand(
                "Test Note",
                @"C:\test",
                "# Test Content"
            );

            // Act
            await _handler.HandleAsync(command);

            // Assert
            await _mockEventDispatcher.Received(1).DispatchAsync(
                Arg.Is<IReadOnlyList<DomainEvent>>(events => 
                    events.Any(e => e is NoteCreated)), 
                Arg.Any<CancellationToken>()
            );
        }
    }

    [TestFixture]
    public class UpdateNoteContentCommandHandlerTests
    {
        private INoteRepository _mockRepository;
        private IDomainEventDispatcher _mockEventDispatcher;
        private UpdateNoteContentCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockRepository = Substitute.For<INoteRepository>();
            _mockEventDispatcher = Substitute.For<IDomainEventDispatcher>();
            _handler = new UpdateNoteContentCommandHandler(_mockRepository, _mockEventDispatcher);
        }

        [Test]
        public async Task HandleAsync_ExistingNote_UpdatesContentSuccessfully()
        {
            // Arrange
            var noteId = "test-note-id";
            var command = new UpdateNoteContentCommand(noteId, "# Updated Content");

            var existingNote = Note.CreateNew(
                "Test Note",
                @"C:\test",
                "# Original Content"
            );

            _mockRepository.GetNoteByIdAsync(noteId).Returns(existingNote);

            // Act
            await _handler.HandleAsync(command);

            // Assert
            await _mockRepository.Received(1).SaveNoteAsync(Arg.Is<Note>(n => 
                n.Content.RawContent == "# Updated Content"));
            await _mockEventDispatcher.Received(1).DispatchAsync(Arg.Any<IReadOnlyList<DomainEvent>>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void HandleAsync_NonExistentNote_ThrowsInvalidOperationException()
        {
            // Arrange
            var noteId = "non-existent";
            var command = new UpdateNoteContentCommand(noteId, "# Updated Content");

            _mockRepository.GetNoteByIdAsync(noteId).Returns((Note?)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.HandleAsync(command));
            Assert.That(ex.Message, Contains.Substring("not found"));
        }
    }

    [TestFixture]
    public class DeleteNoteCommandHandlerTests
    {
        private INoteRepository _mockRepository;
        private IDomainEventDispatcher _mockEventDispatcher;
        private DeleteNoteCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockRepository = Substitute.For<INoteRepository>();
            _mockEventDispatcher = Substitute.For<IDomainEventDispatcher>();
            _handler = new DeleteNoteCommandHandler(_mockRepository, _mockEventDispatcher);
        }

        [Test]
        public async Task HandleAsync_ExistingNote_DeletesSuccessfully()
        {
            // Arrange
            var noteId = "test-note-id";
            var command = new DeleteNoteCommand(noteId);

            var existingNote = Note.CreateNew(
                "Test Note",
                @"C:\test",
                "# Content"
            );

            _mockRepository.GetNoteByIdAsync(noteId).Returns(existingNote);

            // Act
            await _handler.HandleAsync(command);

            // Assert
            await _mockRepository.Received(1).DeleteNoteAsync(noteId);
            await _mockEventDispatcher.Received(1).DispatchAsync(
                Arg.Is<IReadOnlyList<DomainEvent>>(events => 
                    events.Any(e => e is NoteDeleted)), 
                Arg.Any<CancellationToken>()
            );
        }

        [Test]
        public void HandleAsync_NonExistentNote_ThrowsInvalidOperationException()
        {
            // Arrange
            var noteId = "non-existent";
            var command = new DeleteNoteCommand(noteId);

            _mockRepository.GetNoteByIdAsync(noteId).Returns((Note?)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.HandleAsync(command));
            Assert.That(ex.Message, Contains.Substring("not found"));
        }
    }
}
