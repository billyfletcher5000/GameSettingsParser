using System.Windows;

namespace GameSettingsParser.Services.Windows
{
    public class BasicWindowService : IWindowService
    {
        public Window? GetMainWindow() => Application.Current.MainWindow;

        public bool? ShowDialog(Window dialog)
        {
            dialog.Owner = GetMainWindow();
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Topmost = true;
            dialog.ShowInTaskbar = false;
            return dialog.ShowDialog();
        }
    }
}