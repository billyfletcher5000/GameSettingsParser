using System.Data;

namespace GameSettingsParser.Model
{
    public class SectionDataTableModel
    {
        public string Name = string.Empty;
        public readonly DataTable Settings = new();
    }
}