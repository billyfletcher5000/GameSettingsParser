using System.Data;
using GameSettingsParser.Model;

namespace GameSettingsParser.Utility
{
    public static class SectionDataTableHelper
    {
        public const string UnknownSectionName = "Unknown";
        public const string ScreenshotColumnName = "Screenshots";
        public const string ListSeparator = ", ";
        
        public static SectionDataTableModel[] ConvertAnalysisResult(ImageAnalysisResultModel imageAnalysisResult, ParsingProfileModel parsingProfile)
        {
            // TODO: Re-evaluate this logic and associated properties on MarkupTypeModel, there's likely a more robust template w/ rules based approach
            MarkupTypeModel? sectionMarkupType = null;
            MarkupTypeModel? keyItemMarkupType = null;
            List<MarkupTypeModel> itemPropertyMarkupTypes = new();
            foreach (var parsingProfileMarkupType in parsingProfile.MarkupTypes)
            {
                if (parsingProfileMarkupType.IsSearchArea)
                    continue;

                if (parsingProfileMarkupType.ExportSignificance == ExportSignificance.Section)
                {
                    sectionMarkupType ??= parsingProfileMarkupType;
                    continue;
                }

                if (parsingProfileMarkupType.ExportSignificance != ExportSignificance.ItemProperty)
                    continue;
                
                if (parsingProfileMarkupType.IsExportRowKey)
                    keyItemMarkupType ??= parsingProfileMarkupType;
                
                itemPropertyMarkupTypes.Add(parsingProfileMarkupType);
            }
            
            if(keyItemMarkupType == null)
                throw new Exception("No key item markup type found in parsing profile.");
            
            itemPropertyMarkupTypes.Sort((itemA, itemB) =>
            {
                if(itemA.IsExportRowKey)
                    return -1;
                if(itemB.IsExportRowKey)
                    return 1;
                
                return itemA.ExportPropertyOrder < itemB.ExportPropertyOrder ? -1 : 1;
            });
            
            Dictionary<string, SectionColumnData> sectionToColumnData = new();
            
            if(sectionMarkupType == null)
                sectionMarkupType = new MarkupTypeModel() { Name = "Section" };
            
            DataColumn? keyColumn = null;
            DataColumn? screenshotColumn = null;
            Dictionary<string, SectionDataTableModel> result = new();

            foreach (var processedImage in imageAnalysisResult.ProcessedImages)
            {
                SectionDataTableModel? currentSection;
                string? section = null;
                if (processedImage.MarkupTypeToValues.Any(pair => pair.Key == sectionMarkupType))
                {
                    var kvp = processedImage.MarkupTypeToValues.First(pair => pair.Key == sectionMarkupType);
                    section = kvp.Value.First();
                }
                else
                {
                    section = UnknownSectionName;
                }

                if (!result.TryGetValue(section, out var value))
                {
                    currentSection = CreateSectionData(section, result, itemPropertyMarkupTypes, keyItemMarkupType, sectionToColumnData, out screenshotColumn, out keyColumn);
                }
                else
                {
                    currentSection = value;
                    
                    var columnData = sectionToColumnData[section];
                    keyColumn = columnData.KeyColumn;
                    screenshotColumn = columnData.ScreenshotColumn;
                }

                var keyValue = string.Join(ListSeparator, processedImage.MarkupTypeToValues.First(pair => pair.Key == keyItemMarkupType).Value);
                
                var row = currentSection.Settings.Rows.Find(keyValue);
                if(row == null)
                {    
                    row = currentSection.Settings.NewRow();
                    row[keyColumn!] = keyValue;
                    currentSection.Settings.Rows.Add(row);
                }
                
                foreach (var settingMarkupTypeToValue in processedImage.MarkupTypeToValues)
                {
                    if (settingMarkupTypeToValue.Key == sectionMarkupType)
                        continue;
                    
                    var column = currentSection.Settings.Columns.Cast<DataColumn>().First(column => column.ColumnName == settingMarkupTypeToValue.Key.Name);
                    if (column == keyColumn)
                        continue;
                    
                    SetCellValue(row, column, settingMarkupTypeToValue.Value);
                }
                
                SetCellValue(row, screenshotColumn!, [processedImage.ScreenshotPath]);
            }

            return result.Values.ToArray();
        }

        private static SectionDataTableModel CreateSectionData(
            string section, 
            Dictionary<string, SectionDataTableModel> result, 
            List<MarkupTypeModel> itemPropertyMarkupTypes,
            MarkupTypeModel keyItemMarkupType, 
            Dictionary<string, SectionColumnData> sectionToColumnData, 
            out DataColumn? screenshotColumn,
            out DataColumn? keyColumn)
        {
            SectionDataTableModel currentSection;
            currentSection = new SectionDataTableModel() { Name = section };
            result.Add(currentSection.Name, currentSection);
                        
            List<DataColumn> columnsList = [];
            foreach (var itemPropertyMarkupType in itemPropertyMarkupTypes)
                columnsList.Add(new DataColumn(itemPropertyMarkupType.Name, typeof(string)));
                        
            screenshotColumn = new DataColumn(ScreenshotColumnName, typeof(string));
            columnsList.Add(screenshotColumn);
                        
            keyColumn = columnsList.First(column => column.ColumnName == keyItemMarkupType.Name);
                        
            sectionToColumnData.Add(section, new SectionColumnData() { KeyColumn = keyColumn, ScreenshotColumn = screenshotColumn });
                        
            currentSection.Settings.Columns.AddRange(columnsList.ToArray());
            if(keyColumn != null)
                currentSection.Settings.PrimaryKey = [keyColumn];
            
            return currentSection;
        }

        private struct SectionColumnData
        {
            public DataColumn KeyColumn;
            public DataColumn ScreenshotColumn;
        }

        private static void SetCellValue(DataRow row, DataColumn column, List<string> values)
        {
            string previousValue;
            if (row[column] == DBNull.Value)
                previousValue = string.Empty;
            else
                previousValue = $"{row[column] as string}{ListSeparator}";
                        
            previousValue += string.Join(ListSeparator, values);
            row[column] = previousValue;
        }
    }
}