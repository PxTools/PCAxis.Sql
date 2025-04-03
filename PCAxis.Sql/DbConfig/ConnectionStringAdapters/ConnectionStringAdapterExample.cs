namespace PCAxis.Sql.DbConfig.ConnectionStringAdapters
{
    /// <summary>
    /// An example class for the implementation of a
    /// ConnectionStringAdapter.
    /// This class will be instantiated dynamically
    /// if SqlDb.config element Database -> Connection
    /// contains the attribute connectionStringAdapterType
    /// with a value naming the DLL and the namespace and class.
    /// Example for SqlDb.condig:
    /// <Connection .. connectionStringAdapter="PCAxis.Sql.DbConfig.ConnectionStringAdapters.DstConnectionStringAdapter, PCAxis.Sql">
    /// </summary>
    internal class ConnectionStringAdapterExample : IConnectionStringAdapter
    {
        /// <summary>
        /// Makes it possible to override the connectionString
        /// and replace it with another dynamically.
        /// Note that applications might use cache and persist
        /// the connection string.
        /// In that case, an application restart
        /// or deletion of the cache is needed.
        /// </summary>
        /// <param name="connectionString">The original connectionString build from SqlDb.config</param>
        /// <returns>The replaced connectionString</returns>
        public string ReplaceConnectionString(string connectionString)
        {
            // Does nothing beside showcasing the possibilty to return
            // whatever connectionString you want to.
            // At this point, you could switch to another data source, user,
            // set timeout differently, invoke other parameters etc.
            return connectionString;
        }
    }
}
