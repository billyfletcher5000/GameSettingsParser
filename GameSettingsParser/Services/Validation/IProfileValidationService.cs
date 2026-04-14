using System.Text;
using GameSettingsParser.Model;

namespace GameSettingsParser.Services.Validation
{
    public enum ProfileValidationResultType
    {
        Valid,
        ValidWithWarnings,
        Invalid
    }

    public struct ProfileValidationResult
    {
        public ProfileValidationResultType Type =>
            Errors.Count > 0 
                ? ProfileValidationResultType.Invalid 
                : Warnings.Count > 0 
                    ? ProfileValidationResultType.ValidWithWarnings 
                    : ProfileValidationResultType.Valid;

        public List<string> Warnings { get; } = [];
        public List<string> Errors { get; } = [];

        public ProfileValidationResult()
        {
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"Validation Result: {Type}");
            sb.AppendLine($"\tWarnings:");
            foreach (var warning in Warnings)
            {
                sb.AppendLine($"\t\t{warning}");
            }
            
            sb.AppendLine($"\tErrors:");
            foreach (var error in Errors)
            {
                sb.AppendLine($"\t\t{error}");
            }

            return sb.ToString();
        }
    }
    
    public interface IProfileValidationService
    {
        ProfileValidationResult Validate(ParsingProfileModel profile);
    }
}