using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace CommitReminder
{
    public class Options : DialogPage, INotifyPropertyChanged
    {
        private bool _enableReminder = true;
        private int _reminderIntervalSeconds = 600;

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
        [DisplayName("Reminder interval (seconds)")]
        [DefaultValue(600)]
        public int ReminderIntervalSeconds
        {
            get => _reminderIntervalSeconds;
            set
            {
                if (_reminderIntervalSeconds != value)
                {
                    _reminderIntervalSeconds = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReminderIntervalSeconds)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}