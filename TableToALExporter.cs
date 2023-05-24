using System.Text;

namespace UpgradeTableCreator
{
    class TableToALExporter : ITableExporter
    {
        private readonly SqlMetaTableReaderOption _options;
        private readonly List<MetaTable> _tables;

        private int _currentTableId;
        private int _currentTableExtId;

        public TableToALExporter(List<MetaTable> tables, SqlMetaTableReaderOption options)
        {
            _tables = tables;
            _options = options;
        }

        public void Export()
        {
            var sb = new StringBuilder();

            _currentTableId = _options.StartNewTableId;
            _currentTableExtId = _options.StartNewTableId;

            foreach (var table in _tables)
            {
                Console.WriteLine($"Processing table {table.Name}");
                var tableText = TableToText(table);
                if (!string.IsNullOrEmpty(tableText))
                    sb.AppendLine(tableText);

                if (_options.SplitFile)
                    WriteToFile(table.GetALFileName(), tableText);
            }
            if (!_options.SplitFile)
                WriteToFile("ALTables.al", sb.ToString());
        }

        public void WriteToFile(string fileName, string text)
        {
            const string folderName = "AL Files";
            if (!Directory.Exists(folderName))
                Directory.CreateDirectory(folderName);

            using var fs = File.Create(Path.Combine(folderName, fileName));
            using var sw = new StreamWriter(fs);
            sw.Write(text);
            Console.WriteLine($"File \"{fs.Name}\" created.");
        }

        private string TableToText(MetaTable table)
        {
            var sb = new StringBuilder();

            var tableId = table.Id;
            if (_options.KeepTableId)
                _currentTableId = table.Id;

            if (table.IsCustomerTable)
                tableId = _currentTableId++;
            else
                tableId = _currentTableExtId++;

            var tableName = $"{_options.TablePrefix}{table.Name}";
            if (tableName.Length > 30)
                Console.WriteLine(" ---Table name exceeding 30 character limit!");

            table.NewName = tableName;
            Console.WriteLine($"Write table {table.Id} \"{table.Name}\" -> {tableId} \"{table.NewName}\"");

            if (table.IsCustomerTable)
                sb.AppendLine($"table {tableId} \"{table.NewName}\"");
            else
                sb.AppendLine($"tableextension {tableId} \"{table.NewName}\" extends \"{table.Name}\"");

            sb.AppendLine("{");
            sb.AppendLine(GetFields(table));
            sb.AppendLine(GetKeys(table));
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GetFields(MetaTable table)
        {
            var sb = new StringBuilder();

            sb.AppendLine("fields");
            sb.AppendLine("{");

            foreach (var field in table.Fields)
                field.NewName = field.Name;

            foreach (var field in table.GetFilteredFieldsForAL(_options))
            {
                var fieldName = $"{_options.FieldPrefix}{field.Name}";

                if (fieldName.Length > 30)
                {
                    fieldName = field.Name;
                    Console.WriteLine($" -----Field \"{field.Name}\" could not be renamed! (30 character limit)");
                }

                var fieldLength = string.IsNullOrEmpty(field.DataLength) ? string.Empty : $"[{field.DataLength}]";
                var fieldFormat = "field({0};\"{1}\";{2}{3})";

                field.NewName = fieldName;
                sb.AppendLine(string.Format(fieldFormat, field.Id, field.NewName, field.Datatype, fieldLength));
                sb.AppendLine("{");
                sb.AppendLine($"Caption = '{field.Name}';");
                if (field.Datatype == "Option")
                    sb.AppendLine($"OptionMembers = {TransformOptionStringToAL(field.OptionString)};");
                if (field.FieldClass == "FlowField")
                {
                    sb.AppendLine("FieldClass = FlowField;");
                    sb.AppendLine($"CalcFormula = {field.CalcFormula};");
                }
                if (!string.IsNullOrEmpty(field.TableRelation))
                    sb.AppendLine($"TableRelation={field.TableRelation};");
                sb.AppendLine("}");

                field.NewName = fieldName;
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private string GetKeys(MetaTable table)
        {
            if (!table.IsCustomerTable)
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine("keys");
            sb.AppendLine("{");

            for (var i = 0; i < table.Keys.Count; i++)
            {
                var key = table.Keys[i];
                if (!key.Enabled)
                    continue;

                var keyName = key.IsPrimary ? "PK" : $"Key{i + 1}";
                var fields = string.Empty;
                for (var j = 0; j < key.GetFields().Count(); j++)
                {
                    if (fields.Length > 0)
                        fields += ", ";
                    fields += $"\"{key.GetFields()[j]}\"";
                }
                sb.AppendLine($"key({keyName}; {fields})");
                sb.AppendLine("{");

                if (key.Clustered)
                    sb.AppendLine("Clustered = true;");

                sb.AppendLine("}");
            }
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string TransformOptionStringToAL(string optionString)
        {
            var options = optionString.Split(',');
            var alString = string.Empty;
            for (var i = 0; i < options.Length; i++)
            {
                alString += $"\"{options[i]}\"";
                if (i < options.Length - 1)
                    alString += ",";
            }
            return alString;
        }
    }
}
