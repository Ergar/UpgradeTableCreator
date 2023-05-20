namespace UpgradeTableCreator
{
    public static class Helper
    {
        public static string GetSqlString(this string value)
        {
            return value.Replace(".", "_").Replace('/', '_');
        }
    }
}
