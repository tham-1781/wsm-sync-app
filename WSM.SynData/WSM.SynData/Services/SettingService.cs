using System.Windows;

namespace WSM.SynData.Services
{
    public class SettingService
    {
        /// <summary>
        /// Save changes and reload by called Save() and Reload() methods
        /// </summary>
        public static void Commit(bool showAlert = true)
        {
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
            if (showAlert) Success();
        }

        /// <summary>
        /// Show dialog with error message
        /// </summary>
        /// <param name="message">Error message content</param>
        public static void Error(string message = "")
        {
            var errorMessage = string.IsNullOrEmpty(message) ? "Something was went wrong while proceed your request" : message;
            MessageBox.Show(errorMessage, "WSM", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Show dialog with success message
        /// </summary>
        public static void Success()
        {
            MessageBox.Show("The data was successfully saved", "WSM", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
