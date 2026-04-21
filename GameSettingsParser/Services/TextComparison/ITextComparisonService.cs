
using System.Drawing;
using System.Windows.Controls;
using GameSettingsParser.Model;
using GameSettingsParser.Model.TextComparisonConfiguration;

namespace GameSettingsParser.Services.TextComparison
{
    public interface ITextComparisonService
    {
        public ITextComparisonConfigurationModel? Configuration { get; set; }
        public double GetConfidenceInterval(Bitmap imageA, Bitmap imageB, ParsingProfileModel parsingProfile);
    }

    public static class TextComparisonServiceConstants
    {
        public const string NavigationParameterKey = "TextComparisonService";
    }
}