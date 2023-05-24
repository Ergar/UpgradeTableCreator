using System.Xml.Serialization;

namespace UpgradeTableCreator
{
    [Serializable]
    public class Key
    {
        [XmlAttribute(AttributeName = "Key")]
        public string Fields { get; set; }

        [XmlAttribute]
        public bool Enabled { get; set; }

        [XmlAttribute]
        public bool Clustered { get; set; }

        [XmlIgnore]
        public bool IsPrimary { get; set; }

        public Key()
        {
        }

        public string[] GetFields()
        {
            return Fields.Split(',');
        }

        public List<int> GetFieldIds()
        {
            var fields = GetFields();
            var ids = new List<int>();

            foreach (var field in fields)
            {
                var id = field.Substring(5, field.Length - 5);
                ids.Add(int.Parse(id));
            }

            return ids;
        }
    }
}
