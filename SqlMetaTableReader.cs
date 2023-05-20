using System.Data.SqlClient;
using System.Diagnostics;
using System.IO.Compression;

namespace UpgradeTableCreator
{
    public class SqlMetaTableReader
    {
        private readonly SqlMetaTableReaderOption _options;

        public SqlMetaTableReader(SqlMetaTableReaderOption options)
        {
            _options = options;
        }

        public List<MetaTable> GetMetaTables()
        {
            var tables = new List<MetaTable>();
            var compressedData = new Dictionary<int, byte[]>();

            var connectionString = _options.GetConnectionString();
            Console.WriteLine("Using connectionstring: " + connectionString);

            var sw = Stopwatch.StartNew();
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    var command = conn.CreateCommand();
                    command.CommandText = "SELECT Metadata, [Object ID] FROM [dbo].[Object Metadata] WHERE [Object Type] = 1 AND ([Object ID] >= @FromTableId AND [Object ID] <= @ToTableId)";
                    command.Parameters.AddWithValue("@FromTableId", _options.FromTableId);
                    command.Parameters.AddWithValue("@ToTableId", (_options.ToTableId <= 0 ? _options.FromTableId : _options.ToTableId));

                    Console.Write($"Reading metadata of tables from database..");
                    var dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        var output = (byte[])dataReader[0];
                        compressedData.Add(dataReader.GetInt32(1), output);
                    }
                    sw.Stop();
                    Console.WriteLine($"FINISHED ({sw.ElapsedMilliseconds}ms)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    conn.Close();

                    return new();
                }
            }

            Console.Write("Process metadata..");
            sw.Restart();

            var tableReader = new XmlTableReader();
            var resultMs = new MemoryStream();
            var sr = new StreamReader(resultMs);

            foreach (var data in compressedData)
            {
                using (var ms = new MemoryStream(data.Value, 4, data.Value.Length - 4))
                using (var df = new DeflateStream(ms, CompressionMode.Decompress))
                {
                    resultMs.SetLength(0);
                    df.CopyTo(resultMs);
                    resultMs.Position = 0;
                    sr.DiscardBufferedData();
                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
                    tableReader.Xml = sr.ReadToEnd();
                }
                tables.AddRange(tableReader.GetTables());

                WriteXMLToFile(data.Key, tableReader.Xml);
            }
            sw.Stop();
            Console.WriteLine($"FINISHED ({sw.ElapsedMilliseconds}ms)");

            return tables;
        }

        private void WriteXMLToFile(int tableId, string xmlContent)
        {
            var path = $"Table_{tableId}.txt";
            using var sw = File.CreateText(path);
            sw.Write(xmlContent);
        }
    }
}
