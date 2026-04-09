using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using GameSettingsParser.Model;
using GameSettingsParser.ViewModel;

namespace GameSettingsParser.Views;

public partial class MarkupTypeDialog : Window
{
    public MarkupTypeDialogViewModel ViewModel { get; init; }
    public MarkupTypeDialog(MarkupTypeDialogViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }
    
    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}