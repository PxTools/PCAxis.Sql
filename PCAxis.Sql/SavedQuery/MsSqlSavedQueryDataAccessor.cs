using System;

namespace PCAxis.Sql.SavedQuery
{
    public class MsSqlSavedQueryDataAccessor : ISavedQueryDatabaseAccessor
    {
        private string _connectionString;

        public MsSqlSavedQueryDataAccessor()
        {
            if (string.IsNullOrWhiteSpace(System.Configuration.ConfigurationManager.AppSettings["SavedQueryConnectionString"]))
            {
                throw new System.Configuration.ConfigurationErrorsException("AppSetting SavedQueryConnectionString not set in config file");
            }
            _connectionString = System.Configuration.ConfigurationManager.AppSettings["SavedQueryConnectionString"];
        }

        public string Load(int id)
        {
            using (var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new Microsoft.Data.SqlClient.SqlCommand("select QueryText from SavedQueryMeta where QueryId = @queryId", conn);
                cmd.Parameters.AddWithValue("queryId", id);
                string query = cmd.ExecuteScalar() as string;

                return query;
            }

            return null;
        }

        public int Save(string savedQuery, int? id)
        {

            using (var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                    @"insert into 
                        SavedQueryMeta2
                        (
	                        DataSourceType, 
	                        DatabaseId, 
	                        DataSourceId, 
	                        [Status], 
	                        StatusUse, 
	                        StatusChange, 
	                        OwnerId, 
	                        MyDescription, 
	                        CreatedDate, 
	                        SavedQueryFormat, 
	                        SavedQueryStorage, 
	                        QueryText,
                            Runs,
                            Fails
                        )
                        values
                        (
	                        @databaseType,
	                        @databaseId,
	                        @mainTable,
	                        'A',
	                        'P',
	                        'P',
	                        'Anonymous',
	                        @title,
	                        @creationDate,
	                        'PXSJSON',
	                        'D',
	                        @query,
                            0,
	                        0
                        );
                        SELECT @@IDENTITY AS 'Identity';", conn);
                cmd.Parameters.AddWithValue("databaseType", "TODO");
                cmd.Parameters.AddWithValue("databaseId", "TODO");
                cmd.Parameters.AddWithValue("mainTable", "TODO");
                cmd.Parameters.AddWithValue("title", "");
                cmd.Parameters.AddWithValue("creationDate", DateTime.Now);
                cmd.Parameters.AddWithValue("query", savedQuery);
                int newid = Convert.ToInt32(cmd.ExecuteScalar());
                return newid;
            }

            return -1;
        }

        public bool MarkAsRunned(int id)
        {
            using (var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new Microsoft.Data.SqlClient.SqlCommand("update SavedQueryMeta set UsedDate = @lastUsed, Runs = Runs + 1 where QueryId = @queryId", conn);
                cmd.Parameters.AddWithValue("queryId", id);
                cmd.Parameters.AddWithValue("lastUsed", DateTime.Now);
                return cmd.ExecuteNonQuery() == 1;
            }
        }

    }
}
