using System.Windows;
using System.Windows.Input;
using GameSettingsParser.Services.Progress;

namespace GameSettingsParser.ViewModels.Progress
{
    // Slightly modified version of https://github.com/rdingwall/wpf-mvvm-task-progress-dialog
    
    public class ProgressDialogWindowViewModel : BindableBase
    {
        private string _windowTitle = string.Empty;
        private string _label = string.Empty;
        private string _subLabel = string.Empty;
        private bool _isClosed = false;
        private double _progressValue = 0.0;

        public string WindowTitle
        {
            get => _windowTitle;
            private set
            {
                _windowTitle = value;
                RaisePropertyChanged();
            }
        }

        public string Label
        {
            get => _label;
            private set
            {
                _label = value;
                RaisePropertyChanged();
            }
        }

        public string SubLabel
        {
            get => _subLabel;
            private set
            {
                _subLabel = value;
                RaisePropertyChanged();
            }
        }

        public bool IsClosed
        {
            get => _isClosed;
            set
            {
                _isClosed = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCancellable { get; private set; }

        public IProgress<string> ProgressText { get; private set; }
        public IProgress<double> ProgressPercentage { get; private set; }

        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value;
                RaisePropertyChanged();
            }
        }

        public bool IsProgressBarIndeterminate { get; set; }
        
        public ICommand CancelCommand { get; }

        public ProgressDialogWindowViewModel(
            ProgressDialogOptions options,
            CancellationToken? cancellationToken = null,
            bool isCancellable = false,
            bool hasProgressPercentage = false)
        {
            ArgumentNullException.ThrowIfNull(options);
            Label = options.Label;
            IsCancellable = isCancellable;
            cancellationToken?.Register(OnCancelled);
            ProgressText = new Progress<string>(OnProgressTextUpdated);
            ProgressPercentage = new Progress<double>(OnProgressPercentageUpdated);
            CancelCommand = new DelegateCommand(() => IsClosed = true);
            IsProgressBarIndeterminate = !hasProgressPercentage;
        }

        private void OnCancelled()
        {
            // Cancellation may come from a background thread.
            if (Application.Current.Dispatcher != null)
                Application.Current.Dispatcher.Invoke(() => IsClosed = true);
            else
                IsClosed = true;
        }

        private void OnProgressTextUpdated(string value)
        {
            // Progress will probably come from a background thread.
            if (Application.Current.Dispatcher != null)
                Application.Current.Dispatcher.Invoke(() => SubLabel = value);
            else
                SubLabel = value;
        }
        
        private void OnProgressPercentageUpdated(double value)
        {
            // Progress will probably come from a background thread.
            if (Application.Current.Dispatcher != null)
                Application.Current.Dispatcher.Invoke(() => ProgressValue = value);
            else
                ProgressValue = value;
        }
    }
}