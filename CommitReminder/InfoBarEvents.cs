using Microsoft.VisualStudio.Shell.Interop;

namespace CommitReminder
{
    internal sealed class InfoBarEvents : IVsInfoBarUIEvents
    {
        private readonly CommitReminderPackage _package;
        public InfoBarEvents(CommitReminderPackage package) => _package = package;
        public void OnActionItemClicked(IVsInfoBarUIElement element, IVsInfoBarActionItem actionItem) { }
        public void OnClosed(IVsInfoBarUIElement element) => _package.ResetNotification();
    }
}