using GameSettingsParser.Services.TextComparison;

namespace GameSettingsParser.ServiceProviders.TextComparison
{
    public interface ITextComparisonServiceProvider
    {
        public ITextComparisonService Current { get; }
        public event Action<ITextComparisonService>? CurrentChanged;
    }
}