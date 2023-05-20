namespace UpgradeTableCreator
{
    public class SqlMetaTableReaderOption
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public int FromTableId { get; set; }
        public int ToTableId { get; set; }
        public int FromFieldId { get; set; }
        public int ToFieldId { get; set; }
        public int StartNewTableId { get; set; }
        public string TablePrefix { get; set; } = "UPG ";
        public string FieldPrefix { get; set; }
        public string VersionList { get; set; } = "UPG";
        public bool ExportToAL { get; set; }
        public bool SplitFile { get; set; }
        public bool KeepTableId { get; set; }
        public string FromTxtFile { get; set; }

        public SqlMetaTableReaderOption()
        { }

        public SqlMetaTableReaderOption(string[] args)
        {
            ParseArgs(args);
        }

        public void ParseArgs(string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("No arguments found!");

            foreach (var arg in args)
            {
                var split = arg.Split('=');

                var argUp = split[0].ToUpperInvariant();
                switch (argUp)
                {
                    case "SERVER":
                        Server = split[1];
                        break;
                    case "DATABASE":
                        Database = split[1];
                        break;
                    case "FROMTABLEID":
                        FromTableId = ParseArgAsInt(argUp, split[1]);
                        break;
                    case "TOTABLEID":
                        ToTableId = ParseArgAsInt(argUp, split[1]);
                        break;
                    case "FROMFIELDID":
                        FromFieldId = ParseArgAsInt(argUp, split[1]);
                        break;
                    case "TOFIELDID":
                        ToFieldId = ParseArgAsInt(argUp, split[1]);
                        break;
                    case "STARTNEWTABLEID":
                        StartNewTableId = ParseArgAsInt(argUp, split[1]);
                        break;
                    case "TABLEPREFIX":
                        TablePrefix = split[1];
                        break;
                    case "FIELDPREFIX":
                        FieldPrefix = split[1];
                        break;
                    case "VERSIONLIST":
                        VersionList = split[1];
                        break;
                    case "EXPORTTOAL":
                        ExportToAL = true;
                        break;
                    case "SPLITFILE":
                        SplitFile = true;
                        break;
                    case "KEEPTABLEID":
                        KeepTableId = true;
                        break;
                    case "FROMTXTFILE":
                        FromTxtFile = split[1];
                        break;
                    default:
                        Console.WriteLine($"Argument \"{argUp}\" not found!");
                        break;
                }
            }

            if (string.IsNullOrEmpty(FromTxtFile))
            {
                if (string.IsNullOrEmpty(Server))
                    throw new ArgumentException("Server argument must be set!");

                if (string.IsNullOrEmpty(Database))
                    throw new ArgumentException("Database argument must be set!");
            }
        }

        private int ParseArgAsInt(string argName, string argValue)
        {
            if (int.TryParse(argValue, out var id))
                return id;
            else
                Console.WriteLine($"Argument \"{argName}\" could not be parsed into an integer!");
            return 0;
        }

        public string GetConnectionString()
        {
            return @$"Server={Server};Database={Database};Trusted_Connection=True;";
        }
    }
}