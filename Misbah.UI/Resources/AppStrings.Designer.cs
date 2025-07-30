// Auto-generated for localization
namespace Misbah.UI.Resources {
    using System;
    using System.Reflection;
    using System.Resources;
    using System.Globalization;

    public class AppStrings {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    resourceMan = new ResourceManager("Misbah.UI.Resources.AppStrings", typeof(AppStrings).Assembly);
                }
                return resourceMan;
            }
        }

        public static CultureInfo Culture {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }

        public static string NoLibraryLoaded => ResourceManager.GetString("NoLibraryLoaded", resourceCulture);
        public static string LoadLibrary => ResourceManager.GetString("LoadLibrary", resourceCulture);
        public static string GitHubRepo => ResourceManager.GetString("GitHubRepo", resourceCulture);
    }
}
