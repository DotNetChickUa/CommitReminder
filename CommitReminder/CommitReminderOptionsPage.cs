using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CommitReminder
{
    [Guid("842c0375-2cc2-4224-81a3-cf2468fc7f47")]
    public class CommitReminderOptionsPage : DialogPage
    {
        private int _reminderIntervalMinutes = 60;

        [Category("Commit Reminder")]
        [DisplayName("Reminder Interval (minutes)")]
        [Description("How often to show commit reminder notifications.")]
        public int ReminderIntervalMinutes
        {
            get => _reminderIntervalMinutes;
            set => _reminderIntervalMinutes = value < 1 ? 1 : value;
        }
    }
}