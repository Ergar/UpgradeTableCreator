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
        Fieldgroups,
        Code,
        Open,
        Close
    }

    class TextTableReader : ITableReader
    {
        public List<MetaTable> GetTables()
        {
            //@ Not finished!

            var sourceName = "TAB_13_18.txt";
            var targetName = "TAB_13_18_UPG.txt";

            var tables = new List<MetaTable>();

            MetaTable table = null;
            Field field = null;

            var regex = new Regex(@"(?!.*[0-9]).*");

            var lineStack = new Stack<Tuple<LineType, int>>();
            var lineNo = 0;

            var readingFields = false;
            var readingField = false;
            var readingKeys = false;

            foreach (var line in File.ReadLines(Path.Combine(".\\", sourceName)))
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
                        var id = Regex.Replace(splitted[0], " ", "")[1] - '0';

                        field = new Field(id, splitted[2].Trim(), splitted[3].Trim());

                        if (splitted[4].Contains("FieldClass"))
                        {
                            var fcSplit = splitted[4].Trim().Split("=");
                            field.FieldClass = fcSplit[1];
                        }
                    }
                    else
                        lineStack.Push(Tuple.Create(LineType.Open, lineNo));
                }
                else if (readingField)
                {
                    if (l.StartsWith("FieldClass"))
                        field.FieldClass = l.Split("=")[1].TrimEnd(';');
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
