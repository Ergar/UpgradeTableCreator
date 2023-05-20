using System.Xml.Serialization;

namespace UpgradeTableCreator
{
    [Serializable]
    public class MetaTable
    {
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
            var primaryKey = Keys[0];
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