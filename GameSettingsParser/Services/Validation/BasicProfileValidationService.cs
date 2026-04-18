using System.IO;
using GameSettingsParser.Model;

namespace GameSettingsParser.Services.Validation
{
    public class BasicProfileValidationService : IProfileValidationService
    {
        public ProfileValidationResult Validate(ParsingProfileModel profile)
        {
            ProfileValidationResult result = new ProfileValidationResult();
            
            foreach (var markupType in profile.MarkupTypes)
            {
                if(markupType.IsDynamic && markupType.IsPositionedRelativeToOther)
                    result.Warnings.Add($"Markup type '{markupType.Name}' is dynamic and positioned relatively to another markup type. This is not supported.");
                
                if(markupType.IsSearchArea && markupType.IsPositionedRelativeToOther)
                    result.Warnings.Add($"Markup type '{markupType.Name}' is a search area and positioned relatively to another markup type. This is not supported.");
                
                if(profile.MarkupTypes.Count(item => item.Name.Equals(markupType.Name)) > 1)
                    result.Errors.Add($"Markup type '{markupType.Name}' is defined multiple times. This is not supported.");
                
                if(!markupType.IsDynamic && profile.ImageInstances.Count(item => item.MarkupInstances.Any(instance => instance.Type == markupType)) > 1)
                    result.Warnings.Add($"Markup type '{markupType.Name}' is used multiple times. The first instance found (in the order of training images) will be used.");
                
                if(markupType.IsSearchArea && profile.ImageInstances.Count(item => item.MarkupInstances.Any(instance => instance.Type == markupType)) > 1)
                    result.Warnings.Add($"Markup type '{markupType.Name}' is a search area and is used multiple times. The first instance found (in the order of training images) will be used.");
                
                if(markupType.IsPositionedRelativeToOther && !profile.MarkupTypes.Any(item => item.Name.Equals(markupType.PositionedRelativeTo)))
                    result.Errors.Add($"Markup type '{markupType.Name}' is positioned relatively to '{markupType.PositionedRelativeTo}', but this markup type does not exist.");
            }
            
            if(profile.MarkupTypes.Count(type => type.ExportSignificance == ExportSignificance.Section) > 1)
                result.Errors.Add("Multiple sections found in parsing profile. Only one section is supported.");
            
            var numKeys = profile.MarkupTypes.Count(type => type.IsExportRowKey);
            switch (numKeys)
            {
                case 0:
                    result.Warnings.Add("No row keys found in parsing profile, items will not condense by a single key.");
                    break;
                case > 1:
                    result.Errors.Add("Multiple row keys found in parsing profile, only one row key is supported.");
                    break;
            }

            if (profile.MarkupTypes.Count(type => type.ExportSignificance == ExportSignificance.ItemProperty) == 0)
            {
                if(numKeys == 1)
                    result.Warnings.Add("No item properties found in parsing profile.");
                else
                    result.Errors.Add("No item properties or item keys found in parsing profile.");
            }

            foreach (var imageInstance in profile.ImageInstances)
            {
                if(imageInstance.Image == null)
                    result.Errors.Add($"Image instance has null Image property.");
                else
                {
                    if(!File.Exists(imageInstance.Image.Path)) 
                        result.Errors.Add($"Image '{imageInstance.Image.Name}' does not exist at '{imageInstance.Image.Path}'.");
                
                    if(imageInstance.MarkupInstances.Count == 0)
                        result.Warnings.Add($"Image '{imageInstance.Image.Name}' has no markup instances.");
                }
            }
            
            return result;
        }
    }
}