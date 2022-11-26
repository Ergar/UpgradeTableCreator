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
            var compressedData = new List<byte[]>();

            var connectionString = @$"Server={_options.Server};Database={_options.Database};Trusted_Connection=True;";
            Console.WriteLine("Using connectionstring: " + connectionString);
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    var command = conn.CreateCommand();
                    command.CommandText = "SELECT Metadata, [Object ID] FROM [dbo].[Object Metadata] WHERE [Object Type] = 1 AND ([Object ID] >= @FromTableId AND [Object ID] <= @ToTableId)";
                    command.Parameters.AddWithValue("@FromTableId", _options.FromTableId);
                    command.Parameters.AddWithValue("@ToTableId", (_options.ToTableId == 0 ? _options.FromTableId : _options.ToTableId));

                    var dataReader = command.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Console.WriteLine($"Reading metadata of table {dataReader[1]}");
                        var output = (byte[])dataReader[0];
                        compressedData.Add(output);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    conn.Close();

                    return new();
                }
            }

            Console.Write("Process metadata..");
            var sw = Stopwatch.StartNew();
            var tableReader = new XmlTableReader();

            var resultMs = new MemoryStream();
            var sr = new StreamReader(resultMs);

            foreach (var data in compressedData)
            {
                using (var ms = new MemoryStream(data, 4, data.Length - 4))
                using (var df = new DeflateStream(ms, CompressionMode.Decompress))
                {
                    resultMs.SetLength(0);
                    df.CopyTo(resultMs);
                    resultMs.Position = 0;
                    sr.DiscardBufferedData();
                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
                    tableReader.SetXml(sr.ReadToEnd());
                }
                tables.AddRange(tableReader.GetTables());
            }
            sw.Stop();
            Console.WriteLine($"FINISHED ({sw.ElapsedMilliseconds}ms)");

            return tables;
        }
    }
}
