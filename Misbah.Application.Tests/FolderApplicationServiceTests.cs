using NUnit.Framework;
using NSubstitute;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Misbah.Application.Services;
using Misbah.Domain.Entities;
using Misbah.Domain.Interfaces;

namespace Misbah.Application.Tests
{
    [TestFixture]
    public class FolderApplicationServiceTests
    {
        private IFolderRepository _mockFolderRepository;
        private FolderApplicationService _folderApplicationService;

        [SetUp]
        public void SetUp()
        {
            _mockFolderRepository = Substitute.For<IFolderRepository>();
            _folderApplicationService = new FolderApplicationService(_mockFolderRepository);
        }

        [Test]
        public void LoadFolderTree_WithValidPath_ReturnsFilteredTree()
        {
            // Arrange - Use a real directory that exists on Windows
            var rootPath = @"C:\"; // This should exist on Windows systems
            var testTree = new FolderNode
            {
                Name = "Root",
                Path = rootPath,
                Folders = new[]
                {
                    new FolderNode { Name = "Visible", Path = @"C:\Visible" },
                    new FolderNode { Name = ".hidden", Path = @"C:\.hidden" }
                }.ToList()
            };

            _mockFolderRepository.GetFolderTree(rootPath).Returns(testTree);

            // Act
            var result = _folderApplicationService.LoadFolderTree(rootPath);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Folders.Count, Is.EqualTo(1));
            Assert.That(result.Folders.First().Name, Is.EqualTo("Visible"));
        }

        [Test]
        public void LoadFolderTree_WithInvalidPath_ThrowsDirectoryNotFoundException()
        {
            // Arrange
            var invalidPath = @"C:\NonExistent";
            
            // Act & Assert
            Assert.Throws<DirectoryNotFoundException>(() => 
                _folderApplicationService.LoadFolderTree(invalidPath));
        }

        [Test]
        public async Task LoadFolderTreeAsync_WithValidPath_ReturnsFilteredTree()
        {
            // Arrange - Use a real directory that exists on Windows
            var rootPath = @"C:\"; // This should exist on Windows systems
            var testTree = new FolderNode
            {
                Name = "Root",
                Path = rootPath,
                Folders = new[]
                {
                    new FolderNode { Name = "Visible", Path = @"C:\Visible" },
                    new FolderNode { Name = ".hidden", Path = @"C:\.hidden" }
                }.ToList()
            };

            _mockFolderRepository.GetFolderTreeAsync(rootPath).Returns(testTree);

            // Act
            var result = await _folderApplicationService.LoadFolderTreeAsync(rootPath);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Folders.Count, Is.EqualTo(1));
            Assert.That(result.Folders.First().Name, Is.EqualTo("Visible"));
        }

        [Test]
        public void GetVisibleFolders_FiltersHiddenFolders()
        {
            // Arrange
            var testPath = @"C:\TestPath";
            var folders = new[]
            {
                new FolderNode { Name = "Visible1", Path = @"C:\TestPath\Visible1" },
                new FolderNode { Name = ".hidden", Path = @"C:\TestPath\.hidden" },
                new FolderNode { Name = "Visible2", Path = @"C:\TestPath\Visible2" }
            };

            _mockFolderRepository.GetFolders(testPath).Returns(folders);

            // Act
            var result = _folderApplicationService.GetVisibleFolders(testPath);

            // Assert
            var visibleFolders = result.ToList();
            Assert.That(visibleFolders.Count, Is.EqualTo(2));
            Assert.That(visibleFolders.Any(f => f.Name.StartsWith(".")), Is.False);
        }

        [Test]
        public void CreateFolder_WithValidPath_CallsRepository()
        {
            // Arrange
            var testPath = @"C:\TestPath\NewFolder";

            // Act
            _folderApplicationService.CreateFolder(testPath);

            // Assert
            _mockFolderRepository.Received(1).CreateFolder(testPath);
        }

        [Test]
        public void CreateFolder_WithHiddenPath_ThrowsArgumentException()
        {
            // Arrange
            var hiddenPath = @"C:\TestPath\.hidden";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _folderApplicationService.CreateFolder(hiddenPath));
        }

        [Test]
        public void CreateFolder_WithEmptyPath_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _folderApplicationService.CreateFolder(""));
        }

        [Test]
        public void FilterHiddenFolders_RemovesHiddenFoldersRecursively()
        {
            // Arrange
            var testTree = new FolderNode
            {
                Name = "Root",
                Path = @"C:\Root",
                Folders = new[]
                {
                    new FolderNode 
                    { 
                        Name = "Visible", 
                        Path = @"C:\Root\Visible",
                        Folders = new[]
                        {
                            new FolderNode { Name = "SubVisible", Path = @"C:\Root\Visible\SubVisible" },
                            new FolderNode { Name = ".subhidden", Path = @"C:\Root\Visible\.subhidden" }
                        }.ToList()
                    },
                    new FolderNode { Name = ".hidden", Path = @"C:\Root\.hidden" }
                }.ToList()
            };

            // Act
            var result = _folderApplicationService.FilterHiddenFolders(testTree);

            // Assert
            Assert.That(result.Folders.Count, Is.EqualTo(1));
            Assert.That(result.Folders.First().Name, Is.EqualTo("Visible"));
            Assert.That(result.Folders.First().Folders.Count, Is.EqualTo(1));
            Assert.That(result.Folders.First().Folders.First().Name, Is.EqualTo("SubVisible"));
        }

        [Test]
        public void DeleteFolder_WithRootPath_ThrowsInvalidOperationException()
        {
            // Arrange
            var rootPath = @"C:\Root";
            _mockFolderRepository.GetRootPath().Returns(rootPath);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                _folderApplicationService.DeleteFolder(rootPath));
        }

        [Test]
        public void DeleteFolder_WithNonExistentPath_ThrowsDirectoryNotFoundException()
        {
            // Arrange
            var testPath = @"C:\NonExistent";
            _mockFolderRepository.GetRootPath().Returns(@"C:\Root");
            _mockFolderRepository.FolderExists(testPath).Returns(false);

            // Act & Assert
            Assert.Throws<DirectoryNotFoundException>(() => 
                _folderApplicationService.DeleteFolder(testPath));
        }

        [Test]
        public void IsValidFolderPath_WithValidPath_ReturnsTrue()
        {
            // Note: This test depends on the actual file system
            // In a real scenario, we'd mock Directory.Exists
            var result = _folderApplicationService.IsValidFolderPath(@"C:\");
            
            // C:\ should exist on Windows systems
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValidFolderPath_WithInvalidPath_ReturnsFalse()
        {
            var result = _folderApplicationService.IsValidFolderPath(@"Z:\NonExistentPath");
            
            Assert.That(result, Is.False);
        }
    }
}
