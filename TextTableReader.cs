using System.Diagnostics;
using System.Text.RegularExpressions;

namespace UpgradeTableCreator
{
    public enum LineType
    {
        Table,
        ObjectProperties,
        TableProperties,
        Fields,
        Field,
        Keys,
        Key,
        Fieldgroups,
        Code,
        Open,
        Close
    }

    class TextTableReader : ITableReader
    {
        private string _fromTextFile;

        public TextTableReader(string fromTextFile)
        {
            _fromTextFile = fromTextFile;
        }

        public List<MetaTable> GetTables()
        {
            //@ Not finished!

            //var sourceName = "TAB_13_18.txt";
            var targetName = "TAB_13_18_UPG.txt";

            var tables = new List<MetaTable>();

            MetaTable table = null;
            Field field = null;
            Key key = null;

            var regex = new Regex(@"(?!.*[0-9]).*");

            var lineStack = new Stack<Tuple<LineType, int>>();
            var lineNo = 0;

            var readingFields = false;
            var readingField = false;
            var readingKeys = false;
            var readingKey = false;
            var readingTableRelation = false;

            foreach (var line in File.ReadLines(Path.Combine(".\\", _fromTextFile)))
            {
                lineNo++;
                var l = line.Trim();

                if (l.ToUpperInvariant().StartsWith("OBJECT TABLE"))
                {
                    lineStack.Push(Tuple.Create(LineType.Table, lineNo));
                    var match = regex.Match(line);
                    table = new MetaTable(match.Groups[0].Value);
                    Console.Write($"Reading table {table.Name}...");
                }
                else if (l.StartsWith("OBJECT-PROPERTIES"))
                    lineStack.Push(Tuple.Create(LineType.ObjectProperties, lineNo));
                else if (l.StartsWith("PROPERTIES"))
                    lineStack.Push(Tuple.Create(LineType.TableProperties, lineNo));
                else if (l.StartsWith("FIELDS"))
                {
                    lineStack.Push(Tuple.Create(LineType.Fields, lineNo));
                    readingFields = true;
                }
                else if (l.StartsWith("CODE"))
                    lineStack.Push(Tuple.Create(LineType.Code, lineNo));
                else if (l.StartsWith("KEYS"))
                {
                    lineStack.Push(Tuple.Create(LineType.Keys, lineNo));
                    readingKeys = true;
                }
                else if (l.StartsWith("{"))
                {
                    if (readingFields && lineStack.Peek().Item1 == LineType.Open)
                    {
                        lineStack.Push(Tuple.Create(LineType.Field, lineNo));
                        readingField = true;

                        var splitted = l.Split(';');

                        var id = 0;
                        var idMatch = Regex.Match(splitted[0], @"\d+");
                        if (idMatch.Success)
                            id = int.Parse(idMatch.Value);

                        var fieldType = splitted[3].Trim();
                        var size = string.Empty;
                        var sizeRegex = new Regex(@"[^a-zA-Z].*");
                        if (sizeRegex.IsMatch(fieldType))
                        {
                            size = sizeRegex.Match(fieldType).Value;
                            fieldType = fieldType.Replace(size, string.Empty);
                        }
                        field = new Field(id, splitted[2].Trim(), fieldType)
                        {
                            DataLength = size,
                            Enabled = true
                        };

                        if (splitted[4].Contains("FieldClass"))
                        {
                            var fcSplit = splitted[4].Trim().Split("=");
                            field.FieldClass = fcSplit[1];
                        }
                        else if (splitted[4].Contains("TableRelation"))
                        {
                            var trValue = splitted[4].Remove(0, splitted[4].IndexOf("=") + 1);
                            field.TableRelation = trValue;

                            if (splitted.Length <= 5)
                                readingTableRelation = true;
                        }
                    }
                    else if (readingKeys && lineStack.Peek().Item1 == LineType.Open)
                    {
                        lineStack.Push(Tuple.Create(LineType.Key, lineNo));
                        readingKey = true;

                        var splitted = l.Trim('{', '}').Split(';');
                        key = new Key
                        {
                            Enabled = !splitted[0].Contains("No"),
                            Fields = splitted[1].Trim()
                        };

                        if (splitted.Length > 2 && splitted.Contains("Clustered"))
                            key.Clustered = true;
                    }
                    else
                        lineStack.Push(Tuple.Create(LineType.Open, lineNo));
                }
                else if (readingField)
                {
                    if (l.StartsWith("FieldClass"))
                        field.FieldClass = l.Split("=")[1].TrimEnd(';');
                    else if (l.StartsWith("OptionString"))
                        field.OptionString = l.Split("=")[1].TrimEnd(';', '}', ' ').Trim('[', ']');

                    if (readingTableRelation)
                    {
                        if (l.EndsWith(';'))
                            readingTableRelation = false;

                        field.TableRelation += " " + l.TrimEnd(';');
                    }
                }
                else if (readingKey)
                {
                    if (l.Contains("Clustered"))
                        key.Clustered = true;
                }

                if (l.StartsWith("}") || l.EndsWith("}"))
                {
                    if (lineStack.Count == 0)
                        continue;

                    if (lineStack.Peek().Item1 == LineType.Open)
                        lineStack.Pop();

                    var peek = lineStack.Peek().Item1;
                    switch (peek)
                    {
                        case LineType.Table:
                            tables.Add(table);
                            lineStack.Pop();
                            Console.WriteLine($" {table.Fields.Count} fields.. FINISHED");
                            break;
                        case LineType.ObjectProperties:
                        case LineType.TableProperties:
                        case LineType.Code:
                            lineStack.Pop();
                            break;
                        case LineType.Fields:
                            readingFields = false;
                            lineStack.Pop();
                            break;
                        case LineType.Field:
                            table.Fields.Add(field);
                            readingField = false;
                            lineStack.Pop();
                            break;
                        case LineType.Keys:
                            readingKeys = false;
                            lineStack.Pop();
                            break;
                        case LineType.Key:
                            table.Keys.Add(key);
                            readingKey = false;
                            lineStack.Pop();
                            break;
                        case LineType.Fieldgroups:
                            break;
                        case LineType.Open:
                            break;
                        case LineType.Close:
                            break;
                    }
                }
            }

            return tables;
        }
    }
}
