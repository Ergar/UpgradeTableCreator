using UpgradeTableCreator;

class Program
{
    private static void Main(string[] args)
    {
        var options = new SqlMetaTableReaderOption(args);
        var sqlMetaTableReader = new SqlMetaTableReader(options);
        var tables = sqlMetaTableReader.GetMetaTables();

        var textExporter = new TableToTextExporter(tables, options);
        var text = textExporter.GetText();

        using var fs = File.Create("GeneratedTables.txt");
        using var sw = new StreamWriter(fs);
        sw.Write(text);

        Console.WriteLine($"File \"{fs.Name}\" created.");
    }
}