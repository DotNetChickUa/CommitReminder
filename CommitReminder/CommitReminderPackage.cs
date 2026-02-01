using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CommitReminder
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(
        "Commit Reminder",
        "Reminds you to commit uncommitted changes",
        "1.0")]
    [ProvideAutoLoad(
        UIContextGuids80.SolutionExistsAndNotBuildingAndNotDebugging,
        PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(
        typeof(Options),
        "Commit Reminder",
        "General",
        0,
        0,
        supportsAutomation: true)]
    [Guid(PackageGuidString)]
    public sealed class CommitReminderPackage : AsyncPackage
    {
        public const string PackageGuidString = "A1B2C3D4-7E8F-4B9A-9D77-112233445566";

        private Timer _timer;
        private IVsInfoBarUIElement _infoBar;
        private Options _options;

        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            _options = (Options)GetDialogPage(typeof(Options));
            _options.PropertyChanged += Options_PropertyChanged;

            StartTimer();
        }

        private void Options_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Hot-reload: restart timer immediately when settings change
            StartTimer();
        }

        private void StartTimer()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_options == null || !_options.EnableReminder)
            {
                _timer?.Dispose();
                _timer = null;
                ClearNotification();
                return;
            }

            var interval = TimeSpan.FromSeconds(Math.Max(10, _options.ReminderIntervalSeconds));

            _timer?.Dispose();
            _timer = new Timer(_ => CheckAndNotify(), null, interval, interval);
        }

        private void CheckAndNotify()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await JoinableTaskFactory.SwitchToMainThreadAsync();

                if (!HasUncommittedChanges())
                {
                    ClearNotification();
                    return;
                }

                ShowNotification();
            });
        }

        private bool HasUncommittedChanges()
        {
            try
            {
                var gitExt = GetService(typeof(IGitExt)) as IGitExt;
                var repo = gitExt?.ActiveRepositories.FirstOrDefault();
                return repo?.HasUncommittedChanges == true;
            }
            catch
            {
                return false;
            }
        }

        private void ShowNotification()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_infoBar != null) return;

            var factory = GetService(typeof(SVsInfoBarUIFactory)) as IVsInfoBarUIFactory;
            var shell = GetService(typeof(SVsShell)) as IVsShell;
            if (factory == null || shell == null) return;

            var model = new InfoBarModel(
                new[] { new InfoBarTextSpan("Commit Reminder: you have uncommitted changes") },
                KnownMonikers.Git,
                isCloseButtonVisible: true);

            _infoBar = factory.CreateInfoBar(model);
            _infoBar.Advise(new InfoBarEvents(this), out _);

            shell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out var hostObj);
            (hostObj as IVsInfoBarHost)?.AddInfoBar(_infoBar);
        }

        private void ClearNotification()
        {
            _infoBar?.Close();
            _infoBar = null;
        }

        protected override void Dispose(bool disposing)
        {
            _timer?.Dispose();
            ClearNotification();
            if (_options != null) _options.PropertyChanged -= Options_PropertyChanged;
            base.Dispose(disposing);
        }

        [ClassInterface(ClassInterfaceType.AutoDual)]
        [ComVisible(true)]
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

        private sealed class InfoBarEvents : IVsInfoBarUIEvents
        {
            private readonly CommitReminderPackage _package;
            public InfoBarEvents(CommitReminderPackage package) => _package = package;
            public void OnActionItemClicked(IVsInfoBarUIElement element, IVsInfoBarActionItem actionItem) { }
            public void OnClosed(IVsInfoBarUIElement element) => _package._infoBar = null;
        }
    }
}
