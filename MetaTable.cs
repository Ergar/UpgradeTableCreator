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

        [XmlArray("Fields")]
        [XmlArrayItem("Field")]
        public List<Field> Fields { get; init; } = new();

        [XmlArray("Keys")]
        [XmlArrayItem("Key")]
        public List<Key> Keys { get; init; } = new();

        [XmlIgnore]
        public Dictionary<int, Field> PrimaryKeyFields { get; set; } = new();

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
    }
}