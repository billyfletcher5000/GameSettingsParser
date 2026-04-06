namespace GameSettingsParser.Model;

public record struct ImageModel
{
    public string Name { get; set; }
    public string Path { get; set; }
    
    public override string ToString() => Name;
}