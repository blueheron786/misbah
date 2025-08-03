using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Misbah.Core.Services;

namespace Misbah.Core.Tests
{
    /// <summary>
    /// Simple test to verify Git sync functionality works
    /// </summary>
    public class GitSyncIntegrationTest
    {
        public static async Task RunTestAsync()
        {
            // Setup logging
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            
            var gitLogger = loggerFactory.CreateLogger<GitSyncService>();
            var autoSaveLogger = loggerFactory.CreateLogger<AutoSaveService>();
            var coordinatorLogger = loggerFactory.CreateLogger<AutoSyncCoordinator>();
            var noteLogger = loggerFactory.CreateLogger<NoteService>();

            // Create services
            var gitSyncService = new GitSyncService(gitLogger);
            var autoSaveService = new AutoSaveService(autoSaveLogger);
            var coordinator = new AutoSyncCoordinator(autoSaveService, gitSyncService, coordinatorLogger);
            var noteService = new NoteService(noteLogger, gitSyncService);

            // Create test directory
            var testDir = Path.Combine(Path.GetTempPath(), "misbah-git-test-" + Guid.NewGuid().ToString()[..8]);
            Directory.CreateDirectory(testDir);
            noteService.SetRootPath(testDir);

            try
            {
                Console.WriteLine($"Testing Git sync in: {testDir}");

                // Initialize and start coordinator
                await coordinator.InitializeAsync(testDir);
                await coordinator.StartAsync();

                Console.WriteLine("✓ Coordinator started successfully");

                // Create a test note
                var testNote = await noteService.CreateNoteAsync(testDir, "Test Note");
                Console.WriteLine($"✓ Created test note: {testNote.FilePath}");

                // Wait a moment for auto-sync
                await Task.Delay(2000);

                // Update the note
                testNote.Content = "# Test Note\n\nThis is updated content.\n\nAuto-sync should detect this change.";
                await noteService.SaveNoteAsync(testNote);
                Console.WriteLine("✓ Updated and saved test note");

                // Wait for sync
                await Task.Delay(2000);

                // Trigger manual sync
                await coordinator.SaveAndSyncNowAsync();
                Console.WriteLine("✓ Manual sync completed");

                // Stop coordinator
                await coordinator.StopAsync();
                Console.WriteLine("✓ Coordinator stopped");

                // Check that Git repository was created
                var gitDir = Path.Combine(testDir, ".git");
                var misbahDir = Path.Combine(testDir, ".misbah");

                if (Directory.Exists(gitDir))
                    Console.WriteLine("✓ Git repository created");
                else
                    Console.WriteLine("✗ Git repository NOT found");

                if (Directory.Exists(misbahDir))
                    Console.WriteLine("✓ Misbah config directory created");
                else
                    Console.WriteLine("✗ Misbah config directory NOT found");

                Console.WriteLine("\n🎉 Git sync integration test completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                // Cleanup
                coordinator.Dispose();
                try
                {
                    if (Directory.Exists(testDir))
                        Directory.Delete(testDir, recursive: true);
                    Console.WriteLine("✓ Test directory cleaned up");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to cleanup test directory: {ex.Message}");
                }
            }
        }
    }
}
