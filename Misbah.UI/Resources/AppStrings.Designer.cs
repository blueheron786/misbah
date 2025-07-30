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

        public static string GitHubRepo => ResourceManager.GetString("GitHubRepo", resourceCulture);
        public static string WelcomeToMisbah => ResourceManager.GetString("WelcomeToMisbah", resourceCulture);
        public static string NoHubLoaded => ResourceManager.GetString("NoHubLoaded", resourceCulture);
        public static string LoadHub => ResourceManager.GetString("LoadHub", resourceCulture);
        public static string HideFolders => ResourceManager.GetString("HideFolders", resourceCulture);
        public static string ShowFolders => ResourceManager.GetString("ShowFolders", resourceCulture);
        public static string Notes => ResourceManager.GetString("Notes", resourceCulture);
        public static string NoNotesFound => ResourceManager.GetString("NoNotesFound", resourceCulture);
        public static string Preview => ResourceManager.GetString("Preview", resourceCulture);
        public static string Edit => ResourceManager.GetString("Edit", resourceCulture);
        public static string Save => ResourceManager.GetString("Save", resourceCulture);
        public static string SelectNoteToViewOrEdit => ResourceManager.GetString("SelectNoteToViewOrEdit", resourceCulture);
        public static string NoFoldersFound => ResourceManager.GetString("NoFoldersFound", resourceCulture);
        public static string Cancel => ResourceManager.GetString("Cancel", resourceCulture);
    }
}
