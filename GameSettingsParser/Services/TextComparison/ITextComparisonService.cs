
using System.Drawing;
using GameSettingsParser.Model;
using GameSettingsParser.Model.Configuration;

namespace GameSettingsParser.Services.TextComparison
{
    public interface ITextComparisonService
    {
        public IConfigurationModel? Configuration { get; set; }
        public double GetConfidenceInterval(Bitmap imageA, Bitmap imageB, ParsingProfileModel parsingProfile);
    }

    public static class TextComparisonServiceConstants
    {
        public const string NavigationParameterKey = "TextComparisonService";
    }
}