﻿using System.Xml.Serialization;

namespace UpgradeTableCreator
{
    class XmlTableReader : ITableReader
    {
        private string _fileName;
        private bool _usingFile;

        public string Xml { get; set; }

        public XmlTableReader()
        {
        }

        public XmlTableReader(string fileName)
        {
            _fileName = fileName;
            _usingFile = true;
        }

        public List<MetaTable> GetTables()
        {
            var tables = new List<MetaTable>();

            var xmlRoot = new XmlRootAttribute("MetaTable")
            {
                Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects"
            };
            var serializer = new XmlSerializer(typeof(MetaTable), xmlRoot);

            MetaTable table;
            if (_usingFile)
            {
                using var sr = new StreamReader(_fileName);
                table = (MetaTable)serializer.Deserialize(sr);
            }
            else
            {
                using var sr = new StringReader(Xml);
                table = (MetaTable)serializer.Deserialize(sr);
            }

            table.TableSource = TableSource.Xml;
            table.CalcPrimaryKeys();
            tables.Add(table);

            return tables;
        }
    }
}
