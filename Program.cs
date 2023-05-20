using UpgradeTableCreator;

class Program
{
    private static void Main(string[] args)
    {
        var options = new SqlMetaTableReaderOption(args);

        options.ExportToAL = true;

        List<MetaTable> tables = new();
        if (string.IsNullOrEmpty(options.FromTxtFile))
        {
            var sqlMetaTableReader = new SqlMetaTableReader(options);
            tables = sqlMetaTableReader.GetMetaTables();
        }
        else
        {
            var txtReader = new TextTableReader(options.FromTxtFile);
            tables = txtReader.GetTables();
        }

        ITableExporter exporter;
        if (!options.ExportToAL)
            exporter = new TableToTextExporter(tables, options);
        else
            exporter = new TableToALExporter(tables, options);
        exporter.Export();

        var sqlGen = new SqlQueryGenerator(options, tables);
        sqlGen.WriteToFile("SqlQueries.txt");
    }
}