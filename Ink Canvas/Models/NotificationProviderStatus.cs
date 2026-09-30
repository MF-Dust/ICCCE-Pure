using System;

namespace Ink_Canvas.Models
{
    public class NotificationProviderStatus
    {
        public string ProviderId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public bool IsRunning { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? LastUpdatedAt { get; set; }
    }
}
