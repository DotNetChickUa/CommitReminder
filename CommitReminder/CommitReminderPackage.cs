using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace CommitReminder
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideAutoLoad(PackageGuidString, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(CommitReminderOptionsPage),
            "Commit Reminder", "General", 0, 0, true)]
    public sealed class CommitReminderPackage : AsyncPackage
    {
        /// <summary>
        /// CommitReminderPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "842c0375-2cc2-4224-81a3-cf2468fc7f46";


        private Timer _timer;

        /// <inheritdoc />
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);
            var options = (CommitReminderOptionsPage)GetDialogPage(typeof(CommitReminderOptionsPage));
            StartReminder(options.ReminderIntervalMinutes);
        }

        private void StartReminder(int minutes)
        {
            _timer?.Dispose();
            _timer = new Timer(ShowReminder, null, TimeSpan.Zero, TimeSpan.FromMinutes(minutes));
        }

        public void UpdateInterval(int minutes)
        {
            StartReminder(minutes);
        }

        private void ShowReminder(object state)
        {
            ThreadHelper.JoinableTaskFactory.Run( () =>
            {
                VsShellUtilities.ShowMessageBox(this,
                    "⏰ Reminder: Don't forget to commit your changes!",
                    "Commit Reminder",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK, 
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return Task.CompletedTask;
            });
        }
    }

}