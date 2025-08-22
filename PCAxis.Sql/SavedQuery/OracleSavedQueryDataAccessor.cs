using System;

using Oracle.ManagedDataAccess.Client;

namespace PCAxis.Sql.SavedQuery
{
    public class OracleSavedQueryDataAccessor : ISavedQueryDatabaseAccessor
    {
        private readonly string _connectionString;
        private readonly string _savedQueryTableOwner;
        private readonly string _databaseType;
        private readonly string _database;

        public OracleSavedQueryDataAccessor(string connectionString, string savedQueryTableOwner, string databaseType, string database)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _savedQueryTableOwner = savedQueryTableOwner ?? throw new ArgumentNullException(nameof(savedQueryTableOwner));
            _databaseType = databaseType ?? throw new ArgumentNullException(nameof(databaseType));
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public string Load(int id)
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                var cmd = new OracleCommand($"select QueryText from {_savedQueryTableOwner}.SavedQueryMeta2 where QueryId = :queryId", conn);
                cmd.Parameters.Add("queryId", id);
                string query = cmd.ExecuteScalar() as string;

                return query;
            }
        }

        public int Save(string savedQuery, string mainTable, int? id)
        {

            using (var conn = new OracleConnection(_connectionString))
            {
                string insertSQL = @"BEGIN
                        insert into 
                        {3}.SavedQueryMeta2
                        (
                            {0}
                            DataSourceType, 
	                        DatabaseId, 
	                        DataSourceId, 
	                        ""STATUS"", 
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
                            {1}
	                        :databaseType,
	                        :databaseId,
	                        :mainTable,
	                        'A',
	                        'P',
	                        'P',
	                        'Anonymous',
	                        :title,
	                        sysdate,
	                        'PXSJSON',
	                        'D',
	                        :query,
                            0,
	                        0
                        ) {2};
                        END;";

                string queryIdPartCol = "";
                string queryIdPartValue = "";
                string returningPart = "returning queryid into :identity";

                if (id != null)
                {
                    queryIdPartCol = "QueryId, ";
                    queryIdPartValue = ":queryId, ";
                    returningPart = "";
                }

                insertSQL = string.Format(insertSQL, queryIdPartCol, queryIdPartValue, returningPart, _savedQueryTableOwner);

                conn.Open();
                var cmd = new OracleCommand(insertSQL, conn);
                cmd.BindByName = true;
                cmd.Parameters.Add("databaseType", _databaseType);
                cmd.Parameters.Add("databaseId", _database);
                cmd.Parameters.Add("mainTable", mainTable);
                cmd.Parameters.Add("title", " ");
                cmd.Parameters.Add("query", OracleDbType.Clob, savedQuery, System.Data.ParameterDirection.Input);
                cmd.Parameters.Add("identity", OracleDbType.Int16, System.Data.ParameterDirection.ReturnValue);

                if (id != null)
                {
                    cmd.Parameters.Add("queryId", id.Value);
                }

                cmd.ExecuteNonQuery();

                if (id == null)
                {
                    int newId = int.Parse(cmd.Parameters["identity"].Value.ToString());
                    return newId;
                }
                else
                {
                    return id.Value;
                }
            }

        }

        public bool MarkAsRunned(int queryId)
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();
                var cmd = new OracleCommand("update " + _savedQueryTableOwner + ".SavedQueryMeta2 set UsedDate = sysdate, Runs = Runs + 1 where QueryId = :queryId", conn);
                cmd.Parameters.Add("queryId", queryId);

                return cmd.ExecuteNonQuery() == 1;
            }
        }

        public string LoadDefaultSelection(string tableId)
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();
                var cmd = new OracleCommand(
                      $@"select 
                          QueryText 
                        from {_savedQueryTableOwner}.SavedQueryMeta2 
                          join {_savedQueryTableOwner}.DefaultSelection on 
                            {_savedQueryTableOwner}.SavedQueryMeta2.QueryId = {_savedQueryTableOwner}.DefaultSelection.SavedQueryId
                        where {_savedQueryTableOwner}.DefaultSelection.TableId = :tableId", conn);
                cmd.Parameters.Add("tableId", tableId);
                string query = cmd.ExecuteScalar() as string;

                return query;
            }
        }
    }
}
