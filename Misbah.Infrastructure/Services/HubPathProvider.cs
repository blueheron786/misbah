namespace Misbah.Infrastructure.Services
{
    public interface IHubPathProvider
    {
        string GetCurrentHubPath();
        void SetCurrentHubPath(string path);
    }

    public class HubPathProvider : IHubPathProvider
    {
        private static string? _currentHubPath;

        public string GetCurrentHubPath()
        {
            return _currentHubPath ?? string.Empty;
        }

        public void SetCurrentHubPath(string path)
        {
            _currentHubPath = path;
        }
    }
}
