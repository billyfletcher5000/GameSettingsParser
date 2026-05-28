namespace GameSettingsParser.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class SwitchableServiceAttribute : Attribute
    {
        public SwitchableServiceAttribute(string serviceId, string? displayName = null, bool isDefault = false)
        {
            ServiceId = serviceId;
            DisplayName = displayName ?? serviceId;
            IsDefault = isDefault;
        }

        public string ServiceId { get; set; }
        public string DisplayName { get; set; }
        public bool IsDefault { get; set; }
    }
}