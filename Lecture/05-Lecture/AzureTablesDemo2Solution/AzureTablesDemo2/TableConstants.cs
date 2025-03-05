namespace AzureTables2
{

    /// <summary>
    /// Defines table constants
    /// </summary>
    public static class TableConstants
    {        
        public const string LOG_TABLENAME = "logentry";

        public const string INTERNALLOG_PARTITIONKEY = "InternalLogEntry";

        public const string EXTERNALLOG_PARTITIONKEY = "ExternalLogEntry";

        public const string TABLE_CONNECTION_STRING_NAME = "TableConnectionString";

    }
}
