namespace PCAxis.Sql.DbConfig
{
    /// <summary>
    /// Makes it possible to overrule the processing of the connection string,
    /// </summary>
    public interface IConnectionStringAdapter
    {
        /// <summary>
        /// Replaces the connection string with a different one based on a local implementation.
        /// If nothing can or will be processed the method should return the unaltered connectionString variable.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        string ReplaceConnectionString(string connectionString);
    }
}
