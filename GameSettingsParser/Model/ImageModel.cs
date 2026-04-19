using GameSettingsParser.Utility;
using Newtonsoft.Json;

namespace GameSettingsParser.Model;

public class ImageModel
{
    private string _name = string.Empty;
    private string _path = string.Empty;
    private string _relativePath = string.Empty;
    private bool _hasChanges = false;

    public string Name
    {
        get => _name;
        set
        {
            if (!string.Equals(_name, value))
            {
                _name = value;
                HasChanges = true;
            }
        }
    }

    // We support both absolute and relative paths where possible, so projects can be exported as a whole, while also
    // allowing the ability to target images anywhere on a user's PC
    public string Path
    {
        get => _path;
        set
        {
            if (!string.Equals(_path, value))
            {
                _path = value;
                HasChanges = true;
            }
        }
    }

    // We support both absolute and relative paths where possible, so projects can be exported as a whole, while also
    // allowing the ability to target images anywhere on a user's PC
    public string RelativePath
    {
        get => _relativePath;
        set
        {
            if (!string.Equals(_relativePath, value))
            {
                _relativePath = value;
                HasChanges = true;
            }
        }
    }

    [JsonIgnore]
    public bool HasChanges
    {
        get => _hasChanges;
        set
        {
            _hasChanges = value;
            if(value)
                ChangeTracker.NotifyChange(ChangeTracker.ChangeType.Parsing);
        }
    }

    public override string ToString() => Name;
}