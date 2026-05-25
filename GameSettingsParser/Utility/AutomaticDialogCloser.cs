using System.Windows;

namespace GameSettingsParser.Utility
{
    // Cribbed from https://github.com/rdingwall/wpf-mvvm-task-progress-dialog/blob/master/src/ProgressDialogEx/ProgressDialog/ProgressDialogWindow.xaml
    
    public static class AutomaticDialogCloser
    {
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached(
                "DialogResult",
                typeof(bool?),
                typeof(AutomaticDialogCloser),
                new PropertyMetadata(DialogResultChanged));

        private static void DialogResultChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window && (bool?) e.NewValue == true)
                window.Close();
        }
        
        public static void SetDialogResult(Window target, bool? value)
        {
            target.SetValue(DialogResultProperty, value);
        }
    }
}