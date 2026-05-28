using GameSettingsParser.Model.Configuration.TextComparison;
using GameSettingsParser.Services.TextComparison;

namespace GameSettingsParser.ViewModels.Configuration.TextComparison
{
    public abstract class BasicTextComparisonConfigurationViewModel : BindableBase, IConfigurationViewModel
    {
        private BasicTextComparisonConfigurationModel? _basicConfigModel;
        public BasicTextComparisonConfigurationModel? BasicConfigModel
        {
            get => _basicConfigModel;
            set
            {
                if (SetProperty(ref _basicConfigModel, value))
                {
                    RaisePropertyChanged(nameof(MinimumConfidence));
                }
            }
        }

        public float MinimumConfidence
        {
            get => BasicConfigModel?.MinimumConfidence ?? 0.0f;
            set
            {
                if (BasicConfigModel != null) 
                    BasicConfigModel.MinimumConfidence = value;
            }
        }

        public abstract Type ViewType { get; }
    }
}