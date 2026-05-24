using System.Windows;

namespace GameSettingsParser.Services.Windows
{
    public interface IWindowService
    {
        Window? GetMainWindow();
        bool? ShowDialog(Window dialog);
    }
}