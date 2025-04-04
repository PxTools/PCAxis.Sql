using Oracle.ManagedDataAccess.Client;

namespace PCAxis.Sql.SavedQuery
{
    public class OracleSavedQueryDataAccessor : ISavedQueryDatabaseAccessor
    {
        private string _connectionString;

        private string _savedQueryTableOwner;

        public OracleSavedQueryDataAccessor()
        {
            if (string.IsNullOrWhiteSpace(System.Configuration.ConfigurationManager.AppSettings["SavedQueryConnectionString"]))
            {
                throw new System.Configuration.ConfigurationErrorsException("AppSetting SavedQueryConnectionString not set in config file");
            }
            _connectionString = System.Configuration.ConfigurationManager.AppSettings["SavedQueryConnectionString"];


            if (string.IsNullOrWhiteSpace(System.Configuration.ConfigurationManager.AppSettings["SavedQueryTableOwner"]))
            {
                throw new System.Configuration.ConfigurationErrorsException("AppSetting SavedQueryTableOwner not set in config file");
            }
            _savedQueryTableOwner = System.Configuration.ConfigurationManager.AppSettings["SavedQueryTableOwner"];
        }

        public string Load(int id)
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                var cmd = new OracleCommand("select QueryText from " + _savedQueryTableOwner + ".SavedQueryMeta where QueryId = :queryId", conn);
                cmd.Parameters.Add("queryId", id);
                string query = cmd.ExecuteScalar() as string;

                return query;
            }
        }

        public int Save(string savedQuery, int? id = null)
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
                cmd.Parameters.Add("databaseType", "TODO");
                cmd.Parameters.Add("databaseId", "TODO");
                cmd.Parameters.Add("mainTable", "TODO");
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

        public bool MarkAsRunned(int id)
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();
                var cmd = new OracleCommand("update " + _savedQueryTableOwner + ".SavedQueryMeta set UsedDate = sysdate, Runs = Runs + 1 where QueryId = :queryId", conn);
                cmd.Parameters.Add("queryId", id);

                return cmd.ExecuteNonQuery() == 1;
            }

            return false;
        }


    }
}
