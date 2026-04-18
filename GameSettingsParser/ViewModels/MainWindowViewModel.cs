using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Microsoft.Win32;
using GameSettingsParser.Model;
using GameSettingsParser.Services.DataExport;
using GameSettingsParser.Services.ImageAnalysis;
using GameSettingsParser.Services.Validation;
using GameSettingsParser.Settings;
using GameSettingsParser.Utility;
using GameSettingsParser.Views;
using Tesseract;
using Path = System.IO.Path;
using Rect = System.Windows.Rect;

namespace GameSettingsParser.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly ParsingProfileModel _parsingProfile;
        
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

        public ICommand AddImageCommand { get; }
        public ICommand RemoveImageCommand { get; }
        public ICommand GoToNextImageCommand { get; }
        public ICommand GoToPreviousImageCommand { get; }
        
        public ICommand AddMarkupTypeCommand { get; }
        public ICommand RemoveMarkupTypeCommand { get; }
        public ICommand EditMarkupTypeCommand { get; }
        public ICommand ClearTypeInstancesCommand { get; }
        public ICommand ClearAllInstancesCommand { get; }
        
        public ICommand ExportToFileCommand { get; }
        public ICommand ExportToClipboardCommand { get; }
        
        public ICommand TestButtonCommand { get; }
        
        private readonly IImageAnalysisService _imageAnalysisService;
        private readonly IDataExportService _dataExportService;
        private readonly IProfileValidationService _validationService;

        public MainWindowViewModel(IImageAnalysisService imageAnalysisService, IDataExportService dataExportService, IProfileValidationService validationService)
        {
            _imageAnalysisService = imageAnalysisService;
            _dataExportService = dataExportService;
            _validationService = validationService;
            
            AddImageCommand = new DelegateCommand(OnAddImage);
            RemoveImageCommand = new DelegateCommand(OnRemoveImage, () => HasSelectedImage);
            GoToNextImageCommand = new DelegateCommand(GoToNextImage, CanTraverseImages);
            GoToPreviousImageCommand = new DelegateCommand(GoToPreviousImage, CanTraverseImages);
            
            AddMarkupTypeCommand = new DelegateCommand(AddMarkupType);
            RemoveMarkupTypeCommand = new DelegateCommand(RemoveMarkupType, () => HasSelectedMarkupType);
            EditMarkupTypeCommand = new DelegateCommand(EditMarkupType, () => HasSelectedMarkupType);
            ClearTypeInstancesCommand = new DelegateCommand(ClearCurrentTypeMarkupInstances, () => HasSelectedImage && HasSelectedMarkupType && SelectedImageInstance!.MarkupInstances.Count(instance => instance.Type == SelectedMarkupType) > 0);
            ClearAllInstancesCommand = new DelegateCommand(ClearAllMarkupInstances, () => HasSelectedImage && HasSelectedMarkupType && SelectedImageInstance!.MarkupInstances.Count > 0);
            
            ExportToFileCommand = new DelegateCommand(ExportToFile, () => CanGatherAndExport() && _dataExportService.SupportsExportToFile);
            ExportToClipboardCommand = new DelegateCommand(ExportToClipboard, () => CanGatherAndExport() && _dataExportService.SupportsExportToClipboard);

            // Debugging
            TestButtonCommand = new DelegateCommand(TestButton);
            
            _parsingProfile = UserSettings.Instance.ParsingProfile;
            SelectedImage = !string.IsNullOrEmpty(UserSettings.Instance.SelectedImageModel) 
                ? Images.FirstOrDefault(image => image.Name == UserSettings.Instance.SelectedImageModel) 
                : Images.FirstOrDefault();
            SelectedMarkupType = !string.IsNullOrEmpty(UserSettings.Instance.SelectedMarkupType) 
                ? MarkupTypes.FirstOrDefault(type => type.Name == UserSettings.Instance.SelectedMarkupType) 
                : MarkupTypes.FirstOrDefault();
        }

        private bool CanGatherAndExport()
        {
            return _parsingProfile.MarkupTypes.Count != 0
                   && _parsingProfile.Images.Count != 0
                   && _parsingProfile.ImageInstances.Count != 0
                   && _parsingProfile.ImageInstances.Any(instance => instance.MarkupInstances.Count != 0);
        }

        public void Save()
        {
            UserSettings.Instance.ParsingProfile = _parsingProfile;
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
        
        private void ExportToFile()
        {
            var analysisResult = GatherExportResult();

            var saveFileDialog = new SaveFileDialog()
            {
                Title = "Save Exported Data",
                Filter = _dataExportService.FileFilter,
                DefaultExt = _dataExportService.FileExtension,
                FileName = "Exported Data"
            };

            if (saveFileDialog.ShowDialog() == false)
                return;
            
            if (analysisResult != null)
                _dataExportService.ExportToFile(analysisResult, _parsingProfile, saveFileDialog.FileName);
        }

        private void ExportToClipboard()
        {
            var analysisResult = GatherExportResult();
            if (analysisResult != null)
                _dataExportService.ExportToClipboard(analysisResult, _parsingProfile);
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

            if (dialogResult == false)
                return null;
            
            var imagePaths = openFileDialog.FileNames;

            return _imageAnalysisService.Analyse(_parsingProfile, imagePaths);
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
            ((DelegateCommand)ExportToFileCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)ExportToClipboardCommand).RaiseCanExecuteChanged();
        }
    }
}