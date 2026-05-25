using System.Windows;
using GameSettingsParser.ViewModels;

namespace GameSettingsParser.Views.AnalysisExport
{
    public partial class ConfluenceExportDialog : Window
    {
        public ConfluenceExportDialogViewModel ViewModel { get; init; }
        
        public ConfluenceExportDialog(ConfluenceExportDialogViewModel viewModel)
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
}