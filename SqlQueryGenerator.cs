using System.Data.SqlClient;
using System.Text;

namespace UpgradeTableCreator
{
    public class SqlQueryGenerator
    {
        private readonly SqlMetaTableReaderOption _options;
        private readonly List<MetaTable> _tables = new();
        private readonly List<string> _companies = new();

        public SqlQueryGenerator(SqlMetaTableReaderOption options, List<MetaTable> tables)
        {
            _options = options;
            _tables = tables;
        }

        public void WriteToFile(string fileName)
        {
            using var fs = File.Create(fileName);
            using var sw = new StreamWriter(fs);
            sw.Write(GetQueries());
            Console.WriteLine($"File \"{fs.Name}\" created.");
        }

        private void GetCompanies()
        {
            using var conn = new SqlConnection(_options.GetConnectionString());
            try
            {
                conn.Open();

                var command = conn.CreateCommand();
                command.CommandText = "SELECT Name FROM [dbo].[Company]";
                var dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    var name = dataReader.GetString(0);
                    _companies.Add(name.GetSqlString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                conn.Close();
            }
        }

        private string GetQueries()
        {
            GetCompanies();

            var sb = new StringBuilder();
            foreach (var table in _tables)
            {
                var tableText = GetQueryForTable(table);
                if (!string.IsNullOrEmpty(tableText))
                    sb.AppendLine(tableText);
            }

            return sb.ToString();
        }

        private string GetQueryForTable(MetaTable table)
        {
            var query = string.Empty;

            var toFields = string.Empty;
            var fromFields = string.Empty;

            foreach (var field in table.GetFilteredFields(_options))
            {
                if (toFields.Length > 0)
                    toFields += ",";
                toFields += $"[{field.NewName.GetSqlString()}]";

                if (fromFields.Length > 0)
                    fromFields += ",";
                fromFields += $"[{field.Name.GetSqlString()}]";
            }

            if (!table.DataPerCompany)
            {
                var fromTableName = $"[{table.Name.GetSqlString()}]";
                var toTableName = $"[{table.NewName.GetSqlString()}]";

                query = $"INSERT INTO [dbo].{toTableName} ({toFields}) SELECT {fromFields} FROM [dbo].{fromTableName}";
                return query;
            }

            foreach (var company in _companies)
            {
                var fromTableName = $"[{company}${table.Name.GetSqlString()}]";
                var toTableName = $"[{company}${table.NewName.GetSqlString()}]";

                query = $"INSERT INTO [dbo].{toTableName} ({toFields}) SELECT {fromFields} FROM [dbo].{fromTableName}";
            }

            return query;
        }
    }
}
