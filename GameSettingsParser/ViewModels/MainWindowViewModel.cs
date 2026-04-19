using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using GameSettingsParser.Model;
using GameSettingsParser.Services.AnalysisExport;
using GameSettingsParser.Services.ImageAnalysis;
using GameSettingsParser.Services.Validation;
using GameSettingsParser.Settings;
using GameSettingsParser.Utility;
using GameSettingsParser.Views;
using Path = System.IO.Path;

namespace GameSettingsParser.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private ParsingProfileModel _parsingProfile;
        
        private const string ApplicationName = "Game Settings Parser";
        private const string WindowTitleFormat = "{0} - {1}";
        private string? _windowTitle;
        public string? WindowTitle
        {
            get => _windowTitle;
            set => SetProperty(ref _windowTitle, value);
        }
        
        public ObservableCollection<ImageModel> Images => _parsingProfile.Images;
        
        private ImageModel? _selectedImage;
        public ImageModel? SelectedImage
        {
            get => _selectedImage;
            set
            {
                SetProperty(ref _selectedImage, value);
                if (_selectedImage != null)
                {
                    var newSelectedImageInstance = _parsingProfile.ImageInstances.FirstOrDefault(instance => instance.Image == _selectedImage);
                    if (newSelectedImageInstance == null)
                    {
                        newSelectedImageInstance = new ImageInstanceModel() { Image = _selectedImage };
                        _parsingProfile.ImageInstances.Add(newSelectedImageInstance);
                    }
                    
                    if (_selectedImageInstance != null)
                        _selectedImageInstance.MarkupInstances.CollectionChanged -= OnMarkupInstancesOnCollectionChanged;
                    
                    SetProperty(ref _selectedImageInstance, newSelectedImageInstance, nameof(SelectedImageInstance));
                    
                    if (_selectedImageInstance != null)
                        _selectedImageInstance.MarkupInstances.CollectionChanged += OnMarkupInstancesOnCollectionChanged;
                }
                else
                {
                    SetProperty(ref _selectedImageInstance, null, nameof(SelectedImageInstance)); 
                }
            }
        }

        public bool HasSelectedImage => SelectedImage != null;

        private ImageInstanceModel? _selectedImageInstance;
        public ImageInstanceModel? SelectedImageInstance => _selectedImageInstance;
        

        public ObservableCollection<MarkupTypeModel> MarkupTypes => _parsingProfile.MarkupTypes;
        
        private MarkupTypeModel? _selectedMarkupType;
        public MarkupTypeModel? SelectedMarkupType
        {
            get => _selectedMarkupType;
            set => SetProperty(ref _selectedMarkupType, value);
        }
        public bool HasSelectedMarkupType => SelectedMarkupType != null;

        public int WordGapThreshold
        {
            get => _parsingProfile.WordGapThreshold;
            set
            {
                if (_parsingProfile.WordGapThreshold != value)
                {
                    _parsingProfile.WordGapThreshold = value;
                    RaisePropertyChanged();
                }
            }
        }

        public double MinimumDynamicComparisonConfidence
        {
            get => _parsingProfile.MinimumDynamicComparisonConfidence;
            set
            {
                _parsingProfile.MinimumDynamicComparisonConfidence = value;
                RaisePropertyChanged();
            }
        }
        
        public ICommand ClosingWindowCommand { get; }
        
        public ICommand FileNewCommand { get; }
        public ICommand FileOpenCommand { get; }
        public ICommand FileSaveCommand { get; }
        public ICommand FileSaveAsCommand { get; }
        public ICommand FileExportCommand { get; }
        public ICommand FileExitCommand { get; }

        public ICommand AddImageCommand { get; }
        public ICommand RemoveImageCommand { get; }
        public ICommand GoToNextImageCommand { get; }
        public ICommand GoToPreviousImageCommand { get; }
        
        public ICommand AddMarkupTypeCommand { get; }
        public ICommand RemoveMarkupTypeCommand { get; }
        public ICommand EditMarkupTypeCommand { get; }
        public ICommand ClearTypeInstancesCommand { get; }
        public ICommand ClearAllInstancesCommand { get; }
        
        public ICommand ParseToFileCommand { get; }
        public ICommand ParseToClipboardCommand { get; }
        
        public ICommand TestButtonCommand { get; }

        private string _currentProfileFilePath = string.Empty;
        
        private readonly IImageAnalysisService _imageAnalysisService;
        private readonly IAnalysisExportService _analysisExportService;
        private readonly IProfileValidationService _validationService;

        public MainWindowViewModel(IImageAnalysisService imageAnalysisService, IAnalysisExportService analysisExportService, IProfileValidationService validationService)
        {
            _imageAnalysisService = imageAnalysisService;
            _analysisExportService = analysisExportService;
            _validationService = validationService;
            
            ClosingWindowCommand = new DelegateCommand(OnCloseWindow);
            
            FileNewCommand = new DelegateCommand(OnFileNew);
            FileOpenCommand = new DelegateCommand(OnFileOpen);
            FileSaveCommand = new DelegateCommand(OnFileSave);
            FileSaveAsCommand = new DelegateCommand(OnFileSaveAs);
            FileExportCommand = new DelegateCommand(ExportProject);
            FileExitCommand = new DelegateCommand(OnFileExit);
            
            AddImageCommand = new DelegateCommand(OnAddImage);
            RemoveImageCommand = new DelegateCommand(OnRemoveImage, () => HasSelectedImage);
            GoToNextImageCommand = new DelegateCommand(GoToNextImage, CanTraverseImages);
            GoToPreviousImageCommand = new DelegateCommand(GoToPreviousImage, CanTraverseImages);
            
            AddMarkupTypeCommand = new DelegateCommand(AddMarkupType);
            RemoveMarkupTypeCommand = new DelegateCommand(RemoveMarkupType, () => HasSelectedMarkupType);
            EditMarkupTypeCommand = new DelegateCommand(EditMarkupType, () => HasSelectedMarkupType);
            ClearTypeInstancesCommand = new DelegateCommand(ClearCurrentTypeMarkupInstances, () => HasSelectedImage && HasSelectedMarkupType && SelectedImageInstance!.MarkupInstances.Count(instance => instance.Type == SelectedMarkupType) > 0);
            ClearAllInstancesCommand = new DelegateCommand(ClearAllMarkupInstances, () => HasSelectedImage && HasSelectedMarkupType && SelectedImageInstance!.MarkupInstances.Count > 0);
            
            ParseToFileCommand = new DelegateCommand(ParseToFile, () => CanGatherAndExport() && _analysisExportService.SupportsExportToFile);
            ParseToClipboardCommand = new DelegateCommand(ParseToClipboard, () => CanGatherAndExport() && _analysisExportService.SupportsExportToClipboard);

            // Debugging
            TestButtonCommand = new DelegateCommand(TestButton);

            var lastParsingProfilePath = UserSettings.Instance.LastParsingProfilePath;
            if (UserSettings.Instance.AutoOpenLastParsingProfile &&
                lastParsingProfilePath != null && File.Exists(lastParsingProfilePath))
            {
                var profile = ParsingProfileModel.Load(lastParsingProfilePath);
                if (profile != null)
                {
                    SetNewParsingProfile(profile);
                    _currentProfileFilePath = lastParsingProfilePath;
                    _parsingProfile.HasChanges = false;
                    
                    if(UserSettings.Instance.SelectedImageModel != null && Images.Any(image => image.Name == UserSettings.Instance.SelectedImageModel))
                        SelectedImage = Images.First(image => image.Name == UserSettings.Instance.SelectedImageModel);
                    
                    if(UserSettings.Instance.SelectedMarkupType != null && MarkupTypes.Any(type => type.Name == UserSettings.Instance.SelectedMarkupType))
                        SelectedMarkupType = MarkupTypes.First(type => type.Name == UserSettings.Instance.SelectedMarkupType);
                }
            }
            
            if (_parsingProfile == null)
            {
                var profile = new ParsingProfileModel();
                SetNewParsingProfile(profile);
                _parsingProfile.HasChanges = true;
            }
            
            ChangeTracker.OnChangeNotified += OnChangeTrackerChange;
            UpdateWindowTitle();
        }

        public void OnFileNew()
        {
            if (_parsingProfile.HasChanges)
            {
                var msgBoxResult = MessageBox.Show("You have unsaved changes. Do you want to save them?", "Unsaved Changes", MessageBoxButton.YesNoCancel);
                if (msgBoxResult == MessageBoxResult.Cancel)
                    return;

                if (msgBoxResult == MessageBoxResult.Yes)
                {
                    OnFileSave();
                }
            }
            
            SetNewParsingProfile(new ParsingProfileModel());
            _parsingProfile.HasChanges = true;
            _currentProfileFilePath = string.Empty;
            UpdateWindowTitle();
        }
        
        public void OnFileOpen()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Open Parsing Profile",
                Filter = "Parsing Profile Files (*.json)|*.json",
                Multiselect = false
            };
            
            var result = openFileDialog.ShowDialog();
            if (result != true) 
                return;
            
            var profile = ParsingProfileModel.Load(openFileDialog.FileName);
            if (profile == null)
                return;
            
            SetNewParsingProfile(profile);
            _parsingProfile.HasChanges = false;
            _currentProfileFilePath = openFileDialog.FileName;
            UpdateWindowTitle();
        }

        [MemberNotNull(nameof(_parsingProfile))]
        private void SetNewParsingProfile(ParsingProfileModel profile)
        {
            _parsingProfile = profile;
            
            SelectedImage = _parsingProfile.Images.FirstOrDefault();
            SelectedMarkupType = _parsingProfile.MarkupTypes.FirstOrDefault();
            WordGapThreshold = _parsingProfile.WordGapThreshold;
            MinimumDynamicComparisonConfidence = _parsingProfile.MinimumDynamicComparisonConfidence;
            RaiseProfilePropertiesChanged();
            RaiseCommandsCanExecuteChanged();
        }

        public void OnFileSave()
        {
            if (_currentProfileFilePath != null)
            {
                ParsingProfileModel.Save(_parsingProfile, _currentProfileFilePath);
                _parsingProfile.HasChanges = false;
                UpdateWindowTitle();
            }
            else
            {
                OnFileSaveAs();
            }
        }

        public void OnFileSaveAs()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Save Parsing Profile",
                Filter = "Parsing Profile Files (*.json)|*.json",
                DefaultExt = ".json",
                FileName = "Parsing Profile"
            };
            
            var result = saveFileDialog.ShowDialog();
            if (result != true)
                return;
            
            _currentProfileFilePath = saveFileDialog.FileName;
            _parsingProfile.Name = Path.GetFileNameWithoutExtension(_currentProfileFilePath);
            ParsingProfileModel.Save(_parsingProfile, _currentProfileFilePath);
            _parsingProfile.HasChanges = false;
            UpdateWindowTitle();
        }

        public void OnFileExit()
        {
            Application.Current.Shutdown();
        }

        private void OnChangeTrackerChange(ChangeTracker.ChangeType changeType)
        {
            if(changeType == ChangeTracker.ChangeType.Parsing)
                UpdateWindowTitle();
        }

        private void UpdateWindowTitle()
        {
            var title = string.Format(WindowTitleFormat, ApplicationName, _parsingProfile.Name);
            if (_parsingProfile.HasChanges)
                title += "*";
            WindowTitle = title;
        }

        private bool CanGatherAndExport()
        {
            return _parsingProfile.MarkupTypes.Count != 0
                   && _parsingProfile.Images.Count != 0
                   && _parsingProfile.ImageInstances.Count != 0
                   && _parsingProfile.ImageInstances.Any(instance => instance.MarkupInstances.Count != 0);
        }

        public void OnCloseWindow()
        {
            Save();
        }
        
        public void Save()
        {
            UserSettings.Instance.LastParsingProfilePath = !string.IsNullOrEmpty(_currentProfileFilePath) ? _currentProfileFilePath : string.Empty;
            UserSettings.Instance.SelectedImageModel = _selectedImage != null ? _selectedImage.Name : string.Empty;
            UserSettings.Instance.SelectedMarkupType = _selectedMarkupType != null ? _selectedMarkupType.Name : string.Empty;
        }

        public void OnAddImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Open Target Image",
                Filter = "Image Files (PNG, WEBP, GIF, TIFF, JPG, BMP)|*.png;*.webp;*.gif;*.tiff;*.jpg;*.bmp",
                Multiselect = true
            };

            ImageModel? firstAddedImage = null;
            
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                foreach (var file in openFileDialog.FileNames)
                {
                    if (Images.Any(image => image.Path.Equals(file))) continue;
                    
                    ImageModel image = new()
                    {
                        Name = Path.GetFileNameWithoutExtension(file),
                        Path = file,
                    };

                    Images.Add(image);
                    firstAddedImage ??= image;
                }
            }
            
            if(firstAddedImage != null)
                SelectedImage = firstAddedImage;

            RaiseCommandsCanExecuteChanged();
        }

        public void OnRemoveImage()
        {
            if (SelectedImage == null)
                return;

            var imageModelToRemove = SelectedImage;
            if (_parsingProfile.IsImageModelInUse(imageModelToRemove))
            {
                var result = MessageBox.Show("Are you sure you want to remove this image? It has markup data.", "Warning", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.Cancel)
                    return;
            }
            
            GoToPreviousImage();
            _parsingProfile.RemoveImageModel(imageModelToRemove);

            RaiseCommandsCanExecuteChanged();
        }
        
        public void GoToPreviousImage()
        {
            var model = (SelectedImage != null ? Images.GetPrevious(SelectedImage) : Images.First()) ?? Images.First();
            SelectedImage = model;
        }

        public void GoToNextImage()
        {
            var model = (SelectedImage != null ? Images.GetNext(SelectedImage) : Images.Last()) ?? Images.Last();
            SelectedImage = model;
        }

        public bool CanTraverseImages()
        {
            return Images.Count > 1;
        }

        public void AddMarkupType()
        {
            MarkupTypeDialogViewModel dialogViewModel = new(_parsingProfile);
            MarkupTypeDialog dialog = new MarkupTypeDialog(dialogViewModel);

            if (dialog.ShowDialog() == true)
            {
                if(!MarkupTypes.Contains(dialogViewModel.MarkupTypeModel))
                    MarkupTypes.Add(dialogViewModel.MarkupTypeModel);
                
                SelectedMarkupType = dialogViewModel.MarkupTypeModel;
            }
            
            RaiseCommandsCanExecuteChanged();
        }

        public void RemoveMarkupType()
        {
            if (SelectedMarkupType == null)
                return;
            
            var markupTypeToRemove = SelectedMarkupType;
            if (_parsingProfile.IsMarkupTypeInUse(markupTypeToRemove))
            {
                var result = MessageBox.Show("Are you sure you want to remove this markup type? It's still in use!", "Warning", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.Cancel)
                    return;
            }
            
            SelectedMarkupType = MarkupTypes.GetPrevious(SelectedMarkupType);
            _parsingProfile.RemoveMarkupType(markupTypeToRemove);
            
            RaiseCommandsCanExecuteChanged();
        }

        public void EditMarkupType()
        {
            if (SelectedMarkupType == null)
                return;
            
            MarkupTypeDialogViewModel dialogViewModel = new(_parsingProfile, SelectedMarkupType);
            MarkupTypeDialog dialog = new MarkupTypeDialog(dialogViewModel);
            dialog.ShowDialog();
        }

        public void ClearAllMarkupInstances()
        {
            SelectedImageInstance?.MarkupInstances.Clear();
            RaiseCommandsCanExecuteChanged();
        }
        
        private void ClearCurrentTypeMarkupInstances()
        {
            SelectedImageInstance?.MarkupInstances.RemoveAll(instance => instance.Type == SelectedMarkupType);
        }

        public void TestButton()
        {
        }
        
        private void ParseToFile()
        {
            var analysisResult = GatherExportResult();

            if (analysisResult is null)
                return;

            var saveFileDialog = new SaveFileDialog()
            {
                Title = "Save Exported Data",
                Filter = _analysisExportService.FileFilter,
                DefaultExt = _analysisExportService.FileExtension,
                FileName = "Exported Data"
            };

            if (saveFileDialog.ShowDialog() == false)
                return;
            
            _analysisExportService.ExportToFile(analysisResult, _parsingProfile, saveFileDialog.FileName);
        }

        private void ParseToClipboard()
        {
            var analysisResult = GatherExportResult();
            if (analysisResult != null)
                _analysisExportService.ExportToClipboard(analysisResult, _parsingProfile);
        }

        private ImageAnalysisResultModel? GatherExportResult()
        {
            var validationResult = _validationService.Validate(_parsingProfile);
            Console.WriteLine(validationResult);

            if (validationResult.Type == ProfileValidationResultType.Invalid)
            {
                MessageBox.Show("Validation failed. Please fix the following issues before continuing:\n\n" + string.Join('\n', validationResult.Errors), "Validation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Images To Gather Data From",
                Filter = "Image Files (PNG, WEBP, GIF, TIFF, JPG, BMP)|*.png;*.webp;*.gif;*.tiff;*.jpg;*.bmp",
                Multiselect = true
            };
            
            var dialogResult = openFileDialog.ShowDialog();

            if (dialogResult is null or false)
                return null;
            
            var imagePaths = openFileDialog.FileNames;

            return _imageAnalysisService.Analyse(_parsingProfile, imagePaths);
        }

        private void ExportProject()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Save Parsing Profile",
                Filter = "Parsing Profile Files (*.json)|*.json",
                DefaultExt = ".json",
                FileName = "Parsing Profile"
            };
            
            var result = saveFileDialog.ShowDialog();
            if (result is null or false)
                return;
            
            ParsingProfileModel.ExportToPath(_parsingProfile, saveFileDialog.FileName, _currentProfileFilePath);
        }
        
        private void OnMarkupInstancesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            RaiseCommandsCanExecuteChanged();
        }
        
        private void RaiseCommandsCanExecuteChanged()
        {
            ((DelegateCommand)RemoveImageCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)GoToNextImageCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)GoToPreviousImageCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)AddMarkupTypeCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)RemoveMarkupTypeCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)EditMarkupTypeCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)ClearTypeInstancesCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)ClearAllInstancesCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)TestButtonCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)ParseToFileCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)ParseToClipboardCommand).RaiseCanExecuteChanged();
        }
        
        private void RaiseProfilePropertiesChanged()
        {
            RaisePropertyChanged(nameof(WordGapThreshold));
            RaisePropertyChanged(nameof(MinimumDynamicComparisonConfidence));
            RaisePropertyChanged(nameof(Images));
            RaisePropertyChanged(nameof(SelectedImage));
            RaisePropertyChanged(nameof(MarkupTypes));
            RaisePropertyChanged(nameof(SelectedMarkupType));
        }
    }
}