using NUnit.Framework;
using NSubstitute;
using Microsoft.Extensions.DependencyInjection;
using Misbah.Application.Services;
using Misbah.Application.Common;
using Misbah.Application.Commands.Notes;
using Misbah.Application.Queries.Notes;
using Misbah.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Misbah.Application.Tests.Services
{
    [TestFixture]
    public class SimpleMediatorTests
    {
        private IServiceProvider _serviceProvider;
        private SimpleMediator _mediator;

        [SetUp]
        public void Setup()
        {
            _serviceProvider = Substitute.For<IServiceProvider>();
            _mediator = new SimpleMediator(_serviceProvider);
        }

        [Test]
        public async Task SendAsync_Command_CallsCorrectHandler()
        {
            // Arrange
            var command = new UpdateNoteContentCommand("test-id", "new content");
            var mockHandler = Substitute.For<ICommandHandler<UpdateNoteContentCommand>>();
            
            _serviceProvider.GetRequiredService(typeof(ICommandHandler<UpdateNoteContentCommand>))
                .Returns(mockHandler);

            // Act
            await _mediator.SendAsync(command);

            // Assert
            await mockHandler.Received(1).HandleAsync(command, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task SendAsync_CommandWithResult_CallsCorrectHandlerAndReturnsResult()
        {
            // Arrange
            var command = new CreateNoteCommand("Test Note", @"C:\docs", "Content");
            var expectedNote = Note.CreateNew("Test Note", @"C:\docs", "Content");
            var mockHandler = Substitute.For<ICommandHandler<CreateNoteCommand, Note>>();
            
            mockHandler.HandleAsync(command, Arg.Any<CancellationToken>()).Returns(expectedNote);
            _serviceProvider.GetRequiredService(typeof(ICommandHandler<CreateNoteCommand, Note>))
                .Returns(mockHandler);

            // Act
            var result = await _mediator.SendAsync(command);

            // Assert
            Assert.That(result, Is.EqualTo(expectedNote));
            await mockHandler.Received(1).HandleAsync(command, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task QueryAsync_Query_CallsCorrectHandlerAndReturnsResult()
        {
            // Arrange
            var query = new GetNoteByIdQuery("test-id");
            var expectedNote = Note.CreateNew("Test Note", @"C:\docs", "Content");
            var mockHandler = Substitute.For<IQueryHandler<GetNoteByIdQuery, Note?>>();
            
            mockHandler.HandleAsync(query, Arg.Any<CancellationToken>()).Returns(expectedNote);
            _serviceProvider.GetRequiredService(typeof(IQueryHandler<GetNoteByIdQuery, Note?>))
                .Returns(mockHandler);

            // Act
            var result = await _mediator.QueryAsync(query);

            // Assert
            Assert.That(result, Is.EqualTo(expectedNote));
            await mockHandler.Received(1).HandleAsync(query, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task QueryAsync_CollectionQuery_CallsCorrectHandlerAndReturnsCollection()
        {
            // Arrange
            var query = new GetAllNotesQuery();
            var expectedNotes = new List<Note> 
            { 
                Note.CreateNew("Note 1", @"C:\docs", "Content 1"),
                Note.CreateNew("Note 2", @"C:\docs", "Content 2")
            };
            var mockHandler = Substitute.For<IQueryHandler<GetAllNotesQuery, IEnumerable<Note>>>();
            
            mockHandler.HandleAsync(query, Arg.Any<CancellationToken>()).Returns(expectedNotes);
            _serviceProvider.GetRequiredService(typeof(IQueryHandler<GetAllNotesQuery, IEnumerable<Note>>))
                .Returns(mockHandler);

            // Act
            var result = await _mediator.QueryAsync(query);

            // Assert
            Assert.That(result, Is.EqualTo(expectedNotes));
            await mockHandler.Received(1).HandleAsync(query, Arg.Any<CancellationToken>());
        }

        [Test]
        public void SendAsync_HandlerNotRegistered_ThrowsInvalidOperationException()
        {
            // Arrange
            var command = new UpdateNoteContentCommand("test-id", "new content");
            
            _serviceProvider.When(x => x.GetRequiredService(typeof(ICommandHandler<UpdateNoteContentCommand>)))
                .Do(x => throw new InvalidOperationException("Service not registered"));

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => _mediator.SendAsync(command));
        }

        [Test]
        public void QueryAsync_HandlerNotRegistered_ThrowsInvalidOperationException()
        {
            // Arrange
            var query = new GetNoteByIdQuery("test-id");
            
            _serviceProvider.When(x => x.GetRequiredService(typeof(IQueryHandler<GetNoteByIdQuery, Note?>)))
                .Do(x => throw new InvalidOperationException("Service not registered"));

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => _mediator.QueryAsync(query));
        }

        [Test]
        public void SendAsync_HandlerThrowsException_PropagatesException()
        {
            // Arrange
            var command = new UpdateNoteContentCommand("test-id", "new content");
            var mockHandler = Substitute.For<ICommandHandler<UpdateNoteContentCommand>>();
            var expectedException = new ArgumentException("Invalid note ID");
            
            mockHandler.When(x => x.HandleAsync(command, Arg.Any<CancellationToken>())).Do(x => throw expectedException);
            _serviceProvider.GetRequiredService(typeof(ICommandHandler<UpdateNoteContentCommand>))
                .Returns(mockHandler);

            // Act & Assert
            var thrownException = Assert.ThrowsAsync<ArgumentException>(async () => await _mediator.SendAsync(command));
            Assert.That(thrownException.Message, Is.EqualTo("Invalid note ID"));
        }

        [Test]
        public void QueryAsync_HandlerThrowsException_PropagatesException()
        {
            // Arrange
            var query = new GetNoteByIdQuery("test-id");
            var mockHandler = Substitute.For<IQueryHandler<GetNoteByIdQuery, Note?>>();
            var expectedException = new ArgumentException("Invalid note ID");
            
            mockHandler.When(x => x.HandleAsync(query, Arg.Any<CancellationToken>())).Do(x => throw expectedException);
            _serviceProvider.GetRequiredService(typeof(IQueryHandler<GetNoteByIdQuery, Note?>))
                .Returns(mockHandler);

            // Act & Assert
            var thrownException = Assert.ThrowsAsync<ArgumentException>(async () => await _mediator.QueryAsync(query));
            Assert.That(thrownException.Message, Is.EqualTo("Invalid note ID"));
        }

        [Test]
        public async Task SendAsync_WithCancellationToken_PassesToHandler()
        {
            // Arrange
            var command = new UpdateNoteContentCommand("test-id", "new content");
            var mockHandler = Substitute.For<ICommandHandler<UpdateNoteContentCommand>>();
            var cancellationToken = new CancellationToken(true);
            
            _serviceProvider.GetRequiredService(typeof(ICommandHandler<UpdateNoteContentCommand>))
                .Returns(mockHandler);

            // Act
            await _mediator.SendAsync(command, cancellationToken);

            // Assert
            await mockHandler.Received(1).HandleAsync(command, cancellationToken);
        }
    }
}
