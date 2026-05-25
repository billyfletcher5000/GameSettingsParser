using GameSettingsParser.Services.Windows;
using GameSettingsParser.ViewModels.Progress;
using GameSettingsParser.Views.Progress;

namespace GameSettingsParser.Services.Progress
{
    // Slightly modified version of https://github.com/rdingwall/wpf-mvvm-task-progress-dialog
    
    public class ProgressDialogService : IProgressDialogService
    {
        private readonly TaskFactory _taskFactory;
        private readonly IWindowService _windowService;

        public ProgressDialogService(IWindowService windowService)
        {
            _windowService = windowService;
            _taskFactory = Task.Factory;
        }

        public void Execute(Action action, ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            ExecuteInternal((token, progressText, progressPercentage) => action(), options,
                isCancellable: false, hasProgressPercentage: false);
        }

        public void Execute(Action<CancellationToken> action, ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            ExecuteInternal((token, progressText, progressPercentage) => action(token), options, hasProgressPercentage: false);
        }

        public void Execute(Action<IProgress<string>> action, ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            ExecuteInternal((token, progressText, progressPercentage) => action(progressText), options, isCancellable: false, hasProgressPercentage: false);
        }
        
        public void Execute(Action<IProgress<double>> action, ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            ExecuteInternal((token, progressText, progressPercentage) => action(progressPercentage), options, isCancellable: false);
        }

        public void Execute(Action<IProgress<string>, IProgress<double>> action, ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            ExecuteInternal((token, progressText, progressPercentage) => action(progressText, progressPercentage), options, isCancellable: false);
        }

        public void Execute(
            Action<CancellationToken, IProgress<string>> action, ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            ExecuteInternal((token, progressText, progressPercentage) => action(token, progressText), options, hasProgressPercentage: false);
        }
        
        public void Execute(
            Action<CancellationToken, IProgress<double>> action, ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            ExecuteInternal((token, progressText, progressPercentage) => action(token, progressPercentage), options);
        }
        
        public void Execute(
            Action<CancellationToken, IProgress<string>, IProgress<double>> action, ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            ExecuteInternal(action, options);
        }

        public bool TryExecute<T>(Func<T> action, ProgressDialogOptions options, out T result)
        {
            ArgumentNullException.ThrowIfNull(action);
            return TryExecuteInternal((token, progressText, progressPercentage) => action(), options, out result,
                isCancellable: false, hasProgressPercentage: false);
        }

        public bool TryExecute<T>(Func<CancellationToken, T> action, ProgressDialogOptions options, out T result)
        {
            ArgumentNullException.ThrowIfNull(action);
            return TryExecuteInternal((token, progressText, progressPercentage) => action(token), options, out result, hasProgressPercentage: false);
        }

        public bool TryExecute<T>(Func<IProgress<string>, T> action, ProgressDialogOptions options, out T result)
        {
            ArgumentNullException.ThrowIfNull(action);
            return TryExecuteInternal((token, progressText, progressPercentage) => action(progressText), options, out result,
                isCancellable: false, hasProgressPercentage: false);
        }
        
        public bool TryExecute<T>(Func<IProgress<double>, T> action, ProgressDialogOptions options, out T result)
        {
            ArgumentNullException.ThrowIfNull(action);
            return TryExecuteInternal((token, progressText, progressPercentage) => action(progressPercentage), options, out result,
                isCancellable: false);
        }

        public bool TryExecute<T>(Func<IProgress<string>, IProgress<double>, T> action, ProgressDialogOptions options, out T result)
        {
            ArgumentNullException.ThrowIfNull(action);
            return TryExecuteInternal((token, progressText, progressPercentage) => action(progressText, progressPercentage), options, out result,
                isCancellable: false);
        }

        public bool TryExecute<T>(Func<CancellationToken, IProgress<string>, T> action,
            ProgressDialogOptions options, out T result)
        {
            ArgumentNullException.ThrowIfNull(action);
            return TryExecuteInternal((token, progressText, progressPercentage) => action(token, progressText), options, out result, hasProgressPercentage: false);
        }
        
        public bool TryExecute<T>(Func<CancellationToken, IProgress<double>, T> action,
            ProgressDialogOptions options, out T result)
        {
            ArgumentNullException.ThrowIfNull(action);
            return TryExecuteInternal((token, progressText, progressPercentage) => action(token, progressPercentage), options, out result);
        }
        
        public bool TryExecute<T>(Func<CancellationToken, IProgress<string>, IProgress<double>, T> action,
            ProgressDialogOptions options, out T result)
        {
            ArgumentNullException.ThrowIfNull(action);
            return TryExecuteInternal(action, options, out result);
        }

        public async Task ExecuteAsync(Func<Task> action, ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            await ExecuteAsyncInternal((token, progressText, progressPercentage) => action(), options, isCancellable: false, hasProgressPercentage: false);
        }

        public async Task ExecuteAsync(Func<CancellationToken, Task> action, ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            await ExecuteAsyncInternal((token, progressText, progressPercentage) => action(token), options, hasProgressPercentage: false);
        }

        public async Task ExecuteAsync(Func<IProgress<string>, Task> action, ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            await ExecuteAsyncInternal((token, progressText, progressPercentage) => action(progressText), options,
                isCancellable: false, hasProgressPercentage: false);
        }
        
        public async Task ExecuteAsync(Func<IProgress<double>, Task> action, ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            await ExecuteAsyncInternal((token, progressText, progressPercentage) => action(progressPercentage), options,
                isCancellable: false);
        }

        public async Task ExecuteAsync(Func<IProgress<string>, IProgress<double>, Task> action, ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            await ExecuteAsyncInternal((token, progressText, progressPercentage) => action(progressText, progressPercentage), options,
                isCancellable: false);
        }

        public async Task ExecuteAsync(Func<CancellationToken, IProgress<string>, Task> action,
            ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            await ExecuteAsyncInternal((token, progressText, progressPercentage) => action(token, progressText), options, hasProgressPercentage: false);
        }
        
        public async Task ExecuteAsync(Func<CancellationToken, IProgress<double>, Task> action,
            ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            await ExecuteAsyncInternal((token, progressText, progressPercentage) => action(token, progressPercentage), options);
        }
        
        public async Task ExecuteAsync(Func<CancellationToken, IProgress<string>, IProgress<double>, Task> action,
            ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            await ExecuteAsyncInternal(action, options);
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            return await ExecuteAsyncInternal((token, progressText, progressPercentage) => action(), options, isCancellable: false, hasProgressPercentage: false);
        }

        public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            return await ExecuteAsyncInternal((token, progressText, progressPercentage) => action(token), options, hasProgressPercentage: false);
        }

        public async Task<T> ExecuteAsync<T>(Func<IProgress<string>, Task<T>> action, ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            return await ExecuteAsyncInternal((token, progressText, progressPercentage) => action(progressText), options,
                isCancellable: false, hasProgressPercentage: false);
        }

        public async Task<T> ExecuteAsync<T>(Func<IProgress<double>, Task<T>> action, ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            return await ExecuteAsyncInternal((token, progressText, progressPercentage) => action(progressPercentage), options,
                isCancellable: false);
        }

        public async Task<T> ExecuteAsync<T>(Func<IProgress<string>, IProgress<double>, Task<T>> action, ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            return await ExecuteAsyncInternal((token, progressText, progressPercentage) => action(progressText, progressPercentage), options,
                isCancellable: false);
        }

        public async Task<T> ExecuteAsync<T>(Func<CancellationToken, IProgress<string>, Task<T>> action,
            ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            return await ExecuteAsyncInternal((token, progressText, progressPercentage) => action(token, progressText), options, hasProgressPercentage: false);
        }
        
        public async Task<T> ExecuteAsync<T>(Func<CancellationToken, IProgress<double>, Task<T>> action,
            ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            return await ExecuteAsyncInternal((token, progressText, progressPercentage) => action(token, progressPercentage), options);
        }

        public async Task<T> ExecuteAsync<T>(Func<CancellationToken, IProgress<string>, IProgress<double>, Task<T>> action,
            ProgressDialogOptions options)
        {
            ArgumentNullException.ThrowIfNull(action);
            return await ExecuteAsyncInternal(action, options);
        }

        private void ExecuteInternal(Action<CancellationToken, IProgress<string>, IProgress<double>> action,
            ProgressDialogOptions options, bool isCancellable = true, bool hasProgressPercentage = true)
        {
            ArgumentNullException.ThrowIfNull(action);
            ArgumentNullException.ThrowIfNull(options);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                CancellationToken cancellationToken = cancellationTokenSource.Token;

                var viewModel = new ProgressDialogWindowViewModel(
                    options, cancellationToken, isCancellable, hasProgressPercentage);

                var window = new ProgressDialogWindow
                                 {
                                     DataContext = viewModel
                                 };

                var task = _taskFactory
                    .StartNew(() => action(cancellationToken, viewModel.ProgressText, viewModel.ProgressPercentage),
                              cancellationToken);

                task.ContinueWith(_ => viewModel.IsClosed = true);

                _windowService.ShowDialog(window);
            }
        }

        private bool TryExecuteInternal<T>(
            Func<CancellationToken, IProgress<string>, IProgress<double>, T> action,
            ProgressDialogOptions options, out T result, bool isCancellable = true, bool hasProgressPercentage = true)
        {
            ArgumentNullException.ThrowIfNull(action);
            ArgumentNullException.ThrowIfNull(options);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var cancellationToken = cancellationTokenSource.Token;

                var viewModel = new ProgressDialogWindowViewModel(
                    options, cancellationToken, isCancellable, hasProgressPercentage);

                var window = new ProgressDialogWindow
                {
                    DataContext = viewModel
                };

                var task = _taskFactory
                    .StartNew(() => action(cancellationToken, viewModel.ProgressText, viewModel.ProgressPercentage),
                              cancellationToken);

                task.ContinueWith(_ => viewModel.IsClosed = true);

                _windowService.ShowDialog(window);

                if (task.IsCanceled)
                {
                    result = default(T)!;
                    return false;
                }

                if (task.IsCompleted)
                {
                    result = task.Result;
                    return true;
                }

                result = default(T)!;
                return false;
            }
        }

        private async Task<T> ExecuteAsyncInternal<T>(
            Func<CancellationToken, IProgress<string>, IProgress<double>, Task<T>> action,
            ProgressDialogOptions options, bool isCancellable = true, bool hasProgressPercentage = true)
        {
            ArgumentNullException.ThrowIfNull(action);
            ArgumentNullException.ThrowIfNull(options);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                CancellationToken cancellationToken = cancellationTokenSource.Token;

                var viewModel = new ProgressDialogWindowViewModel(
                    options, cancellationToken, isCancellable, hasProgressPercentage);

                var window = new ProgressDialogWindow
                {
                    DataContext = viewModel
                };

                var task = action(cancellationToken, viewModel.ProgressText, viewModel.ProgressPercentage);

                _ = task.ContinueWith(_ => viewModel.IsClosed = true);

                _windowService.ShowDialog(window);

                return await task;
            }
        }

        private async Task ExecuteAsyncInternal(
            Func<CancellationToken, IProgress<string>, IProgress<double>, Task> action,
            ProgressDialogOptions options, bool isCancellable = true, bool hasProgressPercentage = true)
        {
            ArgumentNullException.ThrowIfNull(action);
            ArgumentNullException.ThrowIfNull(options);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var cancellationToken = cancellationTokenSource.Token;

                var viewModel = new ProgressDialogWindowViewModel(
                    options, cancellationToken, isCancellable, hasProgressPercentage);

                var window = new ProgressDialogWindow
                {
                    DataContext = viewModel
                };

                var task = action(cancellationToken, viewModel.ProgressText, viewModel.ProgressPercentage);

                _ = task.ContinueWith(_ => viewModel.IsClosed = true, cancellationToken);

                _windowService.ShowDialog(window);

                await task;
            }
        }
    }
}