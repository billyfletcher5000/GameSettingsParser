using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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
        private ParsingProfileModel _parsingProfile;
        
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
                        newSelectedImageInstance = new ImageInstanceModel() { Image = _selectedImage.Value };
                        _parsingProfile.ImageInstances.Add(newSelectedImageInstance);
                    }

                    SetProperty(ref _selectedImageInstance, newSelectedImageInstance, nameof(SelectedImageInstance)); 
                }
            }
        }

        private ImageInstanceModel? _selectedImageInstance;
        public ImageInstanceModel? SelectedImageInstance => _selectedImageInstance;
        

        public ObservableCollection<MarkupTypeModel> MarkupTypes => _parsingProfile.MarkupTypes;
        
        private MarkupTypeModel? _selectedMarkupType;
        public MarkupTypeModel? SelectedMarkupType
        {
            get => _selectedMarkupType;
            set => SetProperty(ref _selectedMarkupType, value);
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
        
        public ICommand GenerateTesseractDataCommand { get; }
        public ICommand GatherAndExportSettingsCommand { get; }
        
        private readonly IImageAnalysisService _imageAnalysisService;
        private readonly IDataExportService _dataExportService;
        private readonly IProfileValidationService _validationService;

        public MainWindowViewModel(IImageAnalysisService imageAnalysisService, IDataExportService dataExportService, IProfileValidationService validationService)
        {
            _imageAnalysisService = imageAnalysisService;
            _dataExportService = dataExportService;
            _validationService = validationService;
            
            AddImageCommand = new DelegateCommand(OnAddImage);
            RemoveImageCommand = new DelegateCommand(OnRemoveImage, () => SelectedImage.HasValue);
            GoToNextImageCommand = new DelegateCommand(GoToNextImage, CanTraverseImages);
            GoToPreviousImageCommand = new DelegateCommand(GoToPreviousImage, CanTraverseImages);
            
            AddMarkupTypeCommand = new DelegateCommand(AddMarkupType);
            RemoveMarkupTypeCommand = new DelegateCommand(RemoveMarkupType, () => SelectedMarkupType.HasValue);
            EditMarkupTypeCommand = new DelegateCommand(EditMarkupType, () => SelectedMarkupType.HasValue);
            ClearTypeInstancesCommand = new DelegateCommand(ClearCurrentTypeMarkupInstances, () => SelectedImage.HasValue && SelectedMarkupType.HasValue);
            ClearAllInstancesCommand = new DelegateCommand(ClearAllMarkupInstances, () => SelectedImage.HasValue);
            
            GenerateTesseractDataCommand = new DelegateCommand(GenerateTesseractData, () => SelectedMarkupType.HasValue && SelectedImage.HasValue);
            GatherAndExportSettingsCommand = new DelegateCommand(GatherAndExportSettings);

            _parsingProfile = UserSettings.Instance.ParsingProfile;
            SelectedImage = !string.IsNullOrEmpty(UserSettings.Instance.SelectedImageModel) 
                ? Images.FirstOrDefault(image => image.Name == UserSettings.Instance.SelectedImageModel) 
                : Images.FirstOrDefault();
            SelectedMarkupType = !string.IsNullOrEmpty(UserSettings.Instance.SelectedMarkupType) 
                ? MarkupTypes.FirstOrDefault(type => type.Name == UserSettings.Instance.SelectedMarkupType) 
                : MarkupTypes.FirstOrDefault();
        }

        public void Save()
        {
            UserSettings.Instance.ParsingProfile = _parsingProfile;
            UserSettings.Instance.SelectedImageModel = _selectedImage.HasValue ? _selectedImage.Value.Name : string.Empty;
            UserSettings.Instance.SelectedMarkupType = _selectedMarkupType.HasValue ? _selectedMarkupType.Value.Name : string.Empty;
        }

        public void OnAddImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Open Target Image";
            openFileDialog.Filter = "Image Files (PNG, WEBP, GIF, TIFF, JPG, BMP)|*.png;*.webp;*.gif;*.tiff;*.jpg;*.bmp";
            openFileDialog.Multiselect = true;
            
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                foreach (var file in openFileDialog.FileNames)
                {
                    ImageModel image = new()
                    {
                        Name = Path.GetFileNameWithoutExtension(file),
                        Path = file,
                    };
                    
                    if(!Images.Contains(image))
                        Images.Add(image);
                }
                
                if(SelectedImage == null)
                    SelectedImage = Images.First();
            }
        }

        public void OnRemoveImage()
        {
            if (!SelectedImage.HasValue)
                return;

            var imageModelToRemove = SelectedImage.Value;
            if (_parsingProfile.IsImageModelInUse(imageModelToRemove))
            {
                var result = MessageBox.Show("Are you sure you want to remove this image? It has markup data.", "Warning", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.Cancel)
                    return;
            }
            
            GoToPreviousImage();
            _parsingProfile.RemoveImageModel(imageModelToRemove);
        }
        
        public void GoToPreviousImage()
        {
            SelectedImage = SelectedImage.HasValue ? Images.GetPrevious(SelectedImage.Value) : Images.Last();
        }

        public void GoToNextImage()
        {
            SelectedImage = SelectedImage.HasValue ? Images.GetNext(SelectedImage.Value) : Images.First();
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
        }

        public void RemoveMarkupType()
        {
            if (!SelectedMarkupType.HasValue)
                return;
            
            var markupTypeToRemove = SelectedMarkupType.Value;
            if (_parsingProfile.IsMarkupTypeInUse(markupTypeToRemove))
            {
                var result = MessageBox.Show("Are you sure you want to remove this markup type? It's still in use!", "Warning", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.Cancel)
                    return;
            }
            
            SelectedMarkupType = MarkupTypes.GetPrevious(SelectedMarkupType.Value);
            _parsingProfile.RemoveMarkupType(markupTypeToRemove);
        }

        public void EditMarkupType()
        {
            if (!SelectedMarkupType.HasValue)
                return;
            
            MarkupTypeDialogViewModel dialogViewModel = new(_parsingProfile, SelectedMarkupType.Value);
            MarkupTypeDialog dialog = new MarkupTypeDialog(dialogViewModel);
            
            if (dialog.ShowDialog() == true)
            {
                SelectedMarkupType = dialogViewModel.MarkupTypeModel;
            }
        }

        public bool CanRemoveMarkupType()
        {
            return SelectedMarkupType.HasValue;
        }

        public void ClearAllMarkupInstances()
        {
            SelectedImageInstance?.MarkupInstances.Clear();
        }
        
        private void ClearCurrentTypeMarkupInstances()
        {
            SelectedImageInstance?.MarkupInstances.RemoveAll(instance => instance.Type == SelectedMarkupType);
        }

        public void GenerateTesseractData()
        {
            if (SelectedImageInstance == null || SelectedMarkupType == null)
                return;
            
            try
            {
                using (var engine = new TesseractEngine(@"./TesseractData", "eng", EngineMode.Default))
                {
                    BitmapImage bitmapImage =  new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(SelectedImageInstance.Image.Path);
                    bitmapImage.EndInit();
                    
                    using (var img = PixConverter.ToPix(BitmapImage2Bitmap(bitmapImage)))
                    {
                        using (var page = engine.Process(img))
                        {
                            var text = page.GetText();
                            Console.WriteLine("Mean confidence: {0}", page.GetMeanConfidence());
                            Console.WriteLine("Text (GetText): \r\n{0}", text);

                            var regions = page.GetSegmentedRegions(PageIteratorLevel.Word);
                            foreach (var rectangle in regions)
                            {
                                SelectedImageInstance?.MarkupInstances.Add(new MarkupInstanceModel()
                                {
                                    Rect = new Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height),
                                    Type = SelectedMarkupType!.Value
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using(MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        private void GatherAndExportSettings()
        {
            var validationResult = _validationService.Validate(_parsingProfile);
            Console.WriteLine(validationResult);

            if (validationResult.Type == ProfileValidationResultType.Invalid)
            {
                MessageBox.Show("Validation failed. Please fix the following issues before continuing:\n\n" + string.Join('\n', validationResult.Errors), "Validation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select Images To Gather Data From";
            openFileDialog.Filter = "Image Files (PNG, WEBP, GIF, TIFF, JPG, BMP)|*.png;*.webp;*.gif;*.tiff;*.jpg;*.bmp";
            openFileDialog.Multiselect = true;
            bool? dialogResult = openFileDialog.ShowDialog();

            if (dialogResult == false)
                return;
            
            var imagePaths = openFileDialog.FileNames;

            var analysisResult = _imageAnalysisService.Analyse(_parsingProfile, imagePaths);
            
            if(analysisResult != null)
                _dataExportService.Export(analysisResult, null);
        }
    }
}