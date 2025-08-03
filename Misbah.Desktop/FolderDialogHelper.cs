using System.Windows.Forms;

namespace Misbah.UI
{
    public static class FolderDialogHelper
    {
        public static string? ShowFolderDialog()
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
            return null;
        }
    }
}
