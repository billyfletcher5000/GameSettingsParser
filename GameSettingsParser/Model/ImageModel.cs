using GameSettingsParser.Utility;
using Newtonsoft.Json;

namespace GameSettingsParser.Model;

public class ImageModel
{
    private string _name = string.Empty;
    private string _path = string.Empty;
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