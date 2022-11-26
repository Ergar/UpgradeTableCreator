using System.Text;

namespace UpgradeTableCreator
{
    class TableToTextExporter
    {
        private readonly SqlMetaTableReaderOption _options;
        private readonly List<MetaTable> _tables;

        private int _currentTableId;

        public TableToTextExporter(List<MetaTable> tables, SqlMetaTableReaderOption options)
        {
            _tables = tables;
            _options = options;
        }

        public string GetText()
        {
            var sb = new StringBuilder();

            _currentTableId = _options.StartNewTableId;
            foreach (var table in _tables)
            {
                var tableText = TableToText(table);
                if (!string.IsNullOrEmpty(tableText))
                    sb.AppendLine(tableText);
            }

            return sb.ToString();
        }

        private string TableToText(MetaTable table)
        {
            var sb = new StringBuilder();

            _currentTableId++;
            var tableName = $"{_options.TablePrefix}{table.Name}";
            if (tableName.Length > 30)
            {
                tableName = table.Name;
                Console.WriteLine(" ---Table name could not be changed! (30 character limit)");
            }
            Console.WriteLine($"Write table {table.Id} \"{table.Name}\" -> {_currentTableId} \"{tableName}\"");

            if (!HasFieldsInOptionRange(table))
            {
                Console.WriteLine("SKIPPED (No fields founds)");
                return string.Empty;
            }

            sb.AppendLine($"OBJECT Table {_currentTableId} \"{tableName}\"");
            sb.AppendLine("{");
            sb.Append(GetObjectProperties(table));
            sb.Append(GetFields(table));
            sb.Append(GetKeys(table));
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GetObjectProperties(MetaTable table)
        {
            var sb = new StringBuilder();
            sb.AppendLine("OBJECT-PROPERTIES");
            sb.AppendLine("{");
            sb.AppendLine($"Date={DateTime.Today.ToString("dd.MM.yy")};");
            sb.AppendLine($"Time={DateTime.Now.ToString("HH:mm:ss")};");
            sb.AppendLine($"Version List={_options.VersionList}");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string GetFields(MetaTable table)
        {
            var sb = new StringBuilder();
            sb.AppendLine("FIELDS");
            sb.AppendLine("{");

            var fieldProps = "{{{0};;{1};{2};{3}}}";

            foreach (var field in GetFilteredFields(table))
            {
                var fieldLength = string.IsNullOrEmpty(field.DataLength) ? string.Empty : field.DataLength;
                var optionString = $"OptionString={field.OptionString};";
                if (field.Datatype != "Option")
                    optionString = string.Empty;

                var fieldName = $"{_options.FieldPrefix}{field.Name}";

                if (field.IsInPrimaryKeys)
                    fieldName = field.Name;

                if (fieldName.Length > 30)
                {
                    fieldName = field.Name;
                    Console.WriteLine($" -----Field \"{field.Name}\" could not be renamed! (30 character limit)");
                }

                sb.AppendLine(string.Format(fieldProps, field.Id, fieldName, $"{field.Datatype}{fieldLength}", optionString));
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private string GetKeys(MetaTable table)
        {
            if (table.Keys.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine("KEYS");
            sb.AppendLine("{");

            var primaryKey = table.Keys[0];
            var clusteredText = primaryKey.Clustered ? "Clustered=Yes" : string.Empty;
            var fieldNames = string.Empty;

            foreach (var field in table.PrimaryKeyFields)
            {
                if (fieldNames.Length > 0)
                    fieldNames += ",";
                fieldNames += field.Value.Name;
            }

            var keyProps = "{{;{0};{1}}}";
            sb.AppendLine(string.Format(keyProps, fieldNames, clusteredText));

            sb.AppendLine("}");
            return sb.ToString();
        }

        private List<Field> GetFilteredFields(MetaTable table)
        {
            var fields = new List<Field>();

            foreach (var field in table.Fields)
            {
                if (!field.Enabled || field.FieldClass == "FlowField")
                    continue;

                if ((field.Id < _options.FromFieldId || (_options.ToFieldId != 0 && field.Id > _options.ToFieldId)) && !field.IsInPrimaryKeys)
                    continue;

                fields.Add(field);
            }

            return fields;
        }

        private bool HasFieldsInOptionRange(MetaTable table)
        {
            return table.Fields.Any(m => m.Enabled && m.FieldClass != "FlowField" && m.Id >= _options.FromFieldId && (m.Id <= _options.ToFieldId || _options.ToFieldId == 0));
        }
    }
}
