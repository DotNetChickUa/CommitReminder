using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace CommitReminder
{
    public class Options : DialogPage, INotifyPropertyChanged
    {
        private bool _enableReminder = true;
        private int _reminderIntervalMinutes = 60;

        [Category("Commit Reminder")]
        [DisplayName("Enable reminder")]
        [DefaultValue(true)]
        public bool EnableReminder
        {
            get => _enableReminder;
            set
            {
                if (_enableReminder != value)
                {
                    _enableReminder = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnableReminder)));
                }
            }
        }

        [Category("Commit Reminder")]
        [DisplayName("Reminder interval (minutes)")]
        [DefaultValue(60)]
        public int ReminderIntervalMinutes
        {
            get => _reminderIntervalMinutes;
            set
            {
                if (_reminderIntervalMinutes != value)
                {
                    _reminderIntervalMinutes = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReminderIntervalMinutes)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}