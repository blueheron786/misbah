using NUnit.Framework;
using NSubstitute;
using System.Linq;
using System.Threading.Tasks;
using Misbah.Core.Services;
using Misbah.Domain.Entities;
using Misbah.Infrastructure.Repositories;

namespace Misbah.Infrastructure.Tests
{
    [TestFixture]
    public class FolderRepositoryAdapterTests
    {
        private IFolderService _mockLegacyFolderService;
        private FolderRepositoryAdapter _folderRepositoryAdapter;

        [SetUp]
        public void SetUp()
        {
            _mockLegacyFolderService = Substitute.For<IFolderService>();
            _folderRepositoryAdapter = new FolderRepositoryAdapter(_mockLegacyFolderService);
        }

        [Test]
        public void GetFolderTree_CallsLegacyService()
        {
            // Arrange
            var rootPath = @"C:\TestRoot";
            var legacyFolderNode = new Misbah.Core.Models.FolderNode { Name = "Root", Path = rootPath };
            _mockLegacyFolderService.LoadFolderTree(rootPath).Returns(legacyFolderNode);

            // Act
            var result = _folderRepositoryAdapter.GetFolderTree(rootPath);

            // Assert
            Assert.That(result.Name, Is.EqualTo("Root"));
            Assert.That(result.Path, Is.EqualTo(rootPath));
            _mockLegacyFolderService.Received(1).LoadFolderTree(rootPath);
        }

        [Test]
        public async Task GetFolderTreeAsync_CallsLegacyServiceAsynchronously()
        {
            // Arrange
            var rootPath = @"C:\TestRoot";
            var legacyFolderNode = new Misbah.Core.Models.FolderNode { Name = "Root", Path = rootPath };
            _mockLegacyFolderService.LoadFolderTree(rootPath).Returns(legacyFolderNode);

            // Act
            var result = await _folderRepositoryAdapter.GetFolderTreeAsync(rootPath);

            // Assert
            Assert.That(result.Name, Is.EqualTo("Root"));
            Assert.That(result.Path, Is.EqualTo(rootPath));
            _mockLegacyFolderService.Received(1).LoadFolderTree(rootPath);
        }

        [Test]
        public void GetFolder_FindsSpecificFolderInTree()
        {
            // Arrange
            var targetPath = @"C:\TestRoot\SubFolder";
            var legacyFolderNode = new Misbah.Core.Models.FolderNode
            {
                Name = "SubFolder",
                Path = targetPath
            };

            _mockLegacyFolderService.LoadFolderTree(targetPath).Returns(legacyFolderNode);

            // Act
            var result = _folderRepositoryAdapter.GetFolder(targetPath);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("SubFolder"));
            Assert.That(result.Path, Is.EqualTo(targetPath));
        }

        [Test]
        public void CreateFolder_CallsLegacyService()
        {
            // Arrange
            var folderPath = @"C:\TestRoot\NewFolder";

            // Act
            _folderRepositoryAdapter.CreateFolder(folderPath);

            // Assert
            _mockLegacyFolderService.Received(1).CreateFolder(@"C:\TestRoot", "NewFolder");
        }

        [Test]
        public void DeleteFolder_CallsLegacyService()
        {
            // Arrange
            var folderPath = @"C:\TestRoot\FolderToDelete";

            // Act
            _folderRepositoryAdapter.DeleteFolder(folderPath);

            // Assert
            _mockLegacyFolderService.Received(1).DeleteFolder(folderPath);
        }

        [Test]
        public void FolderExists_UsesSystemIoDirectoryExists()
        {
            // Note: This test depends on the actual file system
            var result = _folderRepositoryAdapter.FolderExists(@"C:\");
            
            Assert.That(result, Is.True);
        }
    }
}
