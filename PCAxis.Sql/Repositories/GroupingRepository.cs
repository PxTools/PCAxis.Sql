using PCAxis.Paxiom;
using PCAxis.Sql.DbClient;
using PCAxis.Sql.DbConfig;
using PCAxis.Sql.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace PCAxis.Sql.Repositories
{
    public class GroupingRepository
    {
        private string _database;
        public GroupingRepository(string database)
        {
            _database = database;
        }

        public Models.Grouping GetGrouping(string name, string language)
        {
            //validate input
            if (name == null || language == null)
            {
                return null;
            }

            var grouping = new Models.Grouping();
            string sqlGrouping;
            string sqlValues;

            var config = SqlDbConfigsStatic.DataBases[_database];
            GetQueries(language, out sqlGrouping, out sqlValues, config);

            InfoForDbConnection info;

            info = config.GetInfoForDbConnection(config.GetDefaultConnString());
            var cmd = new PxSqlCommandForTempTables(info.DataBaseType, info.DataProvider, info.ConnectionString);

            System.Data.Common.DbParameter[] parameters = new System.Data.Common.DbParameter[1];
            parameters[0] = cmd.GetStringParameter("grouping", name);
            var valueGroup = cmd.ExecuteSelect(sqlGrouping, parameters);

            parameters = new System.Data.Common.DbParameter[1];
            parameters[0] = cmd.GetStringParameter("grouping", name);
            var vsValue = cmd.ExecuteSelect(sqlValues, parameters);

            grouping = Parse(valueGroup, vsValue);

            return grouping;
        }

        private static void GetQueries(string language, out string sqlGrouping, out string sqlValues, SqlDbConfig config)
        {
            sqlGrouping = string.Empty;
            sqlValues = string.Empty;

            if (config.MetaModel.Equals("2.1"))
            {
                SqlDbConfig_21 cfg = config as SqlDbConfig_21;
                sqlGrouping = QueryLib_21.Queries.GetGroupingQuery(cfg, language);
                sqlValues = QueryLib_21.Queries.GetGroupingValuesQuery(cfg, language);

            }
            else if (config.MetaModel.Equals("2.2"))
            {
                SqlDbConfig_22 cfg = config as SqlDbConfig_22;
                sqlGrouping = QueryLib_22.Queries.GetGroupingQuery(cfg, language);
                sqlValues = QueryLib_22.Queries.GetGroupingValuesQuery(cfg, language);
            }
            else if (config.MetaModel.Equals("2.3"))
            {
                //var meta = new QueryLib_23.MetaQuery((SqlDbConfig_23)config, config.GetInfoForDbConnection("", ""));
                //meta.LanguageCodes = config.GetAllLanguages();
                SqlDbConfig_23 cfg = config as SqlDbConfig_23;
                sqlGrouping = QueryLib_23.Queries.GetGroupingQuery(cfg, language);
                sqlValues = QueryLib_23.Queries.GetGroupingValuesQuery(cfg, language);



            }
            else if (config.MetaModel.Equals("2.4"))
            {
                SqlDbConfig_24 cfg = config as SqlDbConfig_24;
                sqlGrouping = QueryLib_24.Queries.GetGroupingQuery(cfg, language);
                sqlValues = QueryLib_24.Queries.GetGroupingValuesQuery(cfg, language);
            }

        }

        private static PCAxis.Sql.Models.Grouping Parse(DataSet valueGroup, DataSet vsValue)
        {
            //Make sure we have a grouping
            if (valueGroup.Tables.Count == 0 || valueGroup.Tables[0].Rows.Count < 1 || vsValue.Tables.Count == 0)
            {
                return null;
            }
            
            var grouping = new PCAxis.Sql.Models.Grouping();
            grouping.Id = valueGroup.Tables[0].Rows[0][0].ToString(); ;
            grouping.Name = valueGroup.Tables[0].Rows[0][1].ToString();


            var values = new Dictionary<string, GroupedValue>();
            for (int i = 0; i < vsValue.Tables[0].Rows.Count; i++)
            {
                string groupCode = vsValue.Tables[0].Rows[i][0].ToString();
                GroupedValue gValue;

                if (values.ContainsKey(groupCode))
                {
                    gValue = values[groupCode];
                }
                else
                {
                    gValue = new GroupedValue();
                    gValue.Code = groupCode;
                    gValue.Text = vsValue.Tables[0].Rows[i][2] == DBNull.Value ? vsValue.Tables[0].Rows[i][3].ToString() : vsValue.Tables[0].Rows[i][2].ToString();
                    grouping.Values.Add(gValue);
                    values.Add(groupCode, gValue);
                }
                gValue.Codes.Add(vsValue.Tables[0].Rows[i][1].ToString());
            }
            return grouping;
        }

      
    }
}
