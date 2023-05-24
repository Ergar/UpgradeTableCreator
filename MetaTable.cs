using System.Net.NetworkInformation;
using System.Xml.Serialization;

namespace UpgradeTableCreator
{
    public enum TableSource
    {
        Xml,
        Txt
    }

    [Serializable]
    public class MetaTable
    {
        [XmlIgnore]
        public TableSource TableSource { get; set; }

        [XmlAttribute(AttributeName = "ID")]
        public int Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlIgnore]
        public string NewName { get; set; }

        [XmlAttribute]
        public bool DataPerCompany { get; set; }

        [XmlArray("Fields")]
        [XmlArrayItem("Field")]
        public List<Field> Fields { get; init; } = new();

        [XmlArray("Keys")]
        [XmlArrayItem("Key")]
        public List<Key> Keys { get; init; } = new();

        [XmlIgnore]
        public Dictionary<int, Field> PrimaryKeyFields { get; set; } = new();

        [XmlIgnore]
        public bool IsCustomerTable => Id >= 50000;

        public MetaTable()
        {
        }

        public MetaTable(string name)
        {
            Name = name;
        }

        public void CalcPrimaryKeys()
        {
            if (TableSource == TableSource.Xml)
                CalcPrimaryKeysXml();
            else if (TableSource == TableSource.Txt)
                CalcPrimaryKeysTxt();
        }

        private void CalcPrimaryKeysXml()
        {
            var primaryKey = Keys[0];
            primaryKey.IsPrimary = true;
            var keyFieldIds = primaryKey.GetFieldIds();

            foreach (var field in Fields)
            {
                if (keyFieldIds.Any(m => m == field.Id))
                {
                    field.IsInPrimaryKeys = true;
                    PrimaryKeyFields.Add(field.Id, field);
                }
            }
        }
        private void CalcPrimaryKeysTxt()
        {
            var primaryKey = Keys[0];
            primaryKey.IsPrimary = true;

            foreach (var field in Fields)
                if (primaryKey.Fields.Contains(field.Name))
                {
                    field.IsInPrimaryKeys = true;
                    PrimaryKeyFields.Add(field.Id, field);
                }
        }

        public List<Field> GetFilteredFields(SqlMetaTableReaderOption options)
        {
            var fields = new List<Field>();

            foreach (var field in Fields)
            {
                if (!field.Enabled || field.FieldClass == "FlowField")
                    continue;

                if ((field.Id < options.FromFieldId || (options.ToFieldId != 0 && field.Id > options.ToFieldId)) && !field.IsInPrimaryKeys)
                    continue;

                fields.Add(field);
            }

            return fields;
        }

        public List<Field> GetFilteredFieldsForAL(SqlMetaTableReaderOption options)
        {
            var fields = new List<Field>();

            foreach (var field in Fields)
            {
                if (!field.Enabled)
                    continue;

                if (!IsCustomerTable && (field.Id < options.FromFieldId || (options.ToFieldId != 0 && field.Id > options.ToFieldId)))
                    continue;

                fields.Add(field);
            }

            return fields;
        }

        internal string GetALFileName()
        {
            if (!IsCustomerTable)
                return $"{Name.Trim()}.TableExt.al";
            return $"{Name.Trim()}.al";
        }
    }
}