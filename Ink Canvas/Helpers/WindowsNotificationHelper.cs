using Ink_Canvas.Models;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Ink_Canvas.Helpers
{
    internal static class WindowsNotificationHelper
    {
        public static void ShowToast(NotificationMessage message)
        {
            try
            {
                if (message == null) return;
                ShowToastForModernWindows(message);
            }
            catch
            {
            }
        }

        private static void ShowToastForModernWindows(NotificationMessage message)
        {
            var builder = new ToastContentBuilder()
                .AddText(string.IsNullOrWhiteSpace(message.Title) ? "InkCanvasForClass CE" : message.Title);

            if (!string.IsNullOrWhiteSpace(message.Summary)) builder.AddText(message.Summary);
            else if (!string.IsNullOrWhiteSpace(message.Content)) builder.AddText(message.Content);

            builder.Show();
        }
    }
}
