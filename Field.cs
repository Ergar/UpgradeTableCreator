using System.Xml.Serialization;

namespace UpgradeTableCreator
{
    [Serializable]
    public class Field
    {
        [XmlAttribute(AttributeName = "ID")]
        public int Id { get; init; }
        [XmlAttribute]
        public string Name { get; init; }
        [XmlIgnore]
        public string NewName { get; set; }
        [XmlAttribute]
        public string Datatype { get; init; }
        [XmlAttribute]
        public string FieldClass { get; set; }
        [XmlAttribute]
        public string DataLength { get; set; }
        [XmlAttribute]
        public bool Enabled { get; set; }
        [XmlAttribute]
        public string OptionString { get; set; }

        [XmlIgnore]
        public bool IsInPrimaryKeys { get; set; }

        public Field()
        {
        }

        public Field(int id, string name, string type)
        {
            Id = id;
            Name = name;
            Datatype = type;
        }
    }
}
