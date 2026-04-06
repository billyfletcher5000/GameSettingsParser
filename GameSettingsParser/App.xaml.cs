using System.Configuration;
using System.Data;
using System.Windows;
using GameSettingsParser.ViewModel;

namespace GameSettingsParser;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        MainWindow window = new MainWindow();
        window.DataContext = new MainWindowViewModel();
        window.Show();
    }
}