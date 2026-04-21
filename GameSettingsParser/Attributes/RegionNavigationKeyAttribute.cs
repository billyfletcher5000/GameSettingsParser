namespace GameSettingsParser.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class RegionNavigationKeyAttribute(string key) : Attribute
    {
        public string Key { get; } = key;
    }
}