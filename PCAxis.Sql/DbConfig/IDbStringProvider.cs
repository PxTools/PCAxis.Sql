namespace PCAxis.Sql.DbConfig
{
    public interface IDbStringProvider
    {
        string GetConnectionString(SqlDbConfig config, string user, string password);
    }
}
