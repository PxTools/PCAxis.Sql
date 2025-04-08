using System;

namespace PCAxis.Sql.SavedQuery
{
    public class MsSqlSavedQueryDataAccessor : ISavedQueryDatabaseAccessor
    {
        private readonly string _connectionString;
        private readonly string _databaseType;
        private readonly string _database;

        public MsSqlSavedQueryDataAccessor(string connectionString, string databaseType, string database)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _databaseType = databaseType ?? throw new ArgumentNullException(nameof(databaseType));
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public string Load(int id)
        {
            using (var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new Microsoft.Data.SqlClient.SqlCommand("select QueryText from SavedQueryMeta2 where QueryId = @queryId", conn);
                cmd.Parameters.AddWithValue("queryId", id);
                string query = cmd.ExecuteScalar() as string;

                return query;
            }
        }

        public int Save(string savedQuery, string mainTable, int? id)
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
                cmd.Parameters.AddWithValue("databaseType", _databaseType);
                cmd.Parameters.AddWithValue("databaseId", _database);
                cmd.Parameters.AddWithValue("mainTable", mainTable);
                cmd.Parameters.AddWithValue("title", "");
                cmd.Parameters.AddWithValue("creationDate", DateTime.Now);
                cmd.Parameters.AddWithValue("query", savedQuery);
                int newid = Convert.ToInt32(cmd.ExecuteScalar());
                return newid;
            }

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

        public string LoadDefaultSelection(string tableId)
        {
            throw new NotImplementedException();
        }
    }
}
