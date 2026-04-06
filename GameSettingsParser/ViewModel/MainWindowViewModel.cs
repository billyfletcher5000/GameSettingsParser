using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using GameSettingsParser.Model;
using GameSettingsParser.Settings;
using GameSettingsParser.Utility;
using GameSettingsParser.Views;
using Path = System.IO.Path;

namespace GameSettingsParser.ViewModel
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
                        newSelectedImageInstance = new ParsingProfileModel.ImageInstance() { Image = _selectedImage.Value };
                        _parsingProfile.ImageInstances.Add(newSelectedImageInstance);
                    }

                    SetProperty(ref _selectedImageInstance, newSelectedImageInstance, nameof(SelectedImageInstance)); 
                }
            }
        }

        private ParsingProfileModel.ImageInstance? _selectedImageInstance;
        public ParsingProfileModel.ImageInstance? SelectedImageInstance => _selectedImageInstance;
        

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

        public MainWindowViewModel()
        {
            AddImageCommand = new DelegateCommand(OnAddImage);
            RemoveImageCommand = new DelegateCommand(OnRemoveImage, CanRemoveImage);
            GoToNextImageCommand = new DelegateCommand(GoToNextImage, CanTraverseImages);
            GoToPreviousImageCommand = new DelegateCommand(GoToPreviousImage, CanTraverseImages);
            
            AddMarkupTypeCommand = new DelegateCommand(AddMarkupType);
            RemoveMarkupTypeCommand = new DelegateCommand(RemoveMarkupType);

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

        public bool CanRemoveImage()
        {
            return SelectedImage.HasValue;
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
            MarkupTypeDialog dialog = new MarkupTypeDialog();

            if (dialog.ShowDialog() == true)
            {
                if(!MarkupTypes.Contains(dialog.MarkupTypeModel))
                    MarkupTypes.Add(dialog.MarkupTypeModel);
                
                SelectedMarkupType = dialog.MarkupTypeModel;
            }
        }

        public void RemoveMarkupType()
        {
            if (!SelectedMarkupType.HasValue)
                return;
            
            var markupTypeToRemove = SelectedMarkupType.Value;
            if (_parsingProfile.IsMarkupTypeInUse(markupTypeToRemove))
            {
                var result = MessageBox.Show("Are you sure you want to remove this markup type? It's still in use", "Warning", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.Cancel)
                    return;
            }
            
            SelectedMarkupType = MarkupTypes.GetPrevious(SelectedMarkupType.Value);
            _parsingProfile.RemoveMarkupType(markupTypeToRemove);
        }

        public bool CanRemoveMarkupType()
        {
            return SelectedMarkupType.HasValue;
        }
    }
}