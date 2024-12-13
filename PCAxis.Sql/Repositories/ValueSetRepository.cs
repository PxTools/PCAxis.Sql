using System;
using System.Data;

using PCAxis.Sql.DbClient;
using PCAxis.Sql.DbConfig;
using PCAxis.Sql.Models;

namespace PCAxis.Sql.Repositories
{
    internal class ValueSetRepository
    {

        internal ValueSetRepository()
        {
        }

        internal ValueSet GetValueSet(string name, string language)
        {
            //validate input
            if (name == null || language == null)
            {
                return null;
            }

            ValueSet valueset = null;
            string sqlValueset;
            string sqlValues;

            var config = SqlDbConfigsStatic.DefaultDatabase;

            InfoForDbConnection info;

            info = config.GetInfoForDbConnection(config.GetDefaultConnString());
            var cmd = new PxSqlCommandForTempTables(info.DataBaseType, info.DataProvider, info.ConnectionString);
            GetQueries(language, out sqlValueset, out sqlValues, config, cmd);

            System.Data.Common.DbParameter[] parameters = new System.Data.Common.DbParameter[1];
            parameters[0] = cmd.GetStringParameter("aValueSet", name);
            var valuesetDS = cmd.ExecuteSelect(sqlValueset, parameters);

            parameters = new System.Data.Common.DbParameter[1];
            parameters[0] = cmd.GetStringParameter("aValueSet", name);
            var valueDS = cmd.ExecuteSelect(sqlValues, parameters);

            valueset = Parse(name, valuesetDS, valueDS);

            return valueset;
        }

        private static void GetQueries(string language, out string sqlValueset, out string sqlValues, SqlDbConfig config, PxSqlCommand sqlCommand)
        {
            sqlValueset = string.Empty;
            sqlValues = string.Empty;

            if (config.MetaModel.Equals("2.1"))
            {
                sqlValueset = QueryLib_21.Queries.GetValueSetQuery((SqlDbConfig_21)config, language, sqlCommand);
                sqlValues = QueryLib_21.Queries.GetValueSetValuesQuery((SqlDbConfig_21)config, language, sqlCommand);

            }
            else if (config.MetaModel.Equals("2.2"))
            {
                sqlValueset = QueryLib_22.Queries.GetValueSetQuery((SqlDbConfig_22)config, language, sqlCommand);
                sqlValues = QueryLib_22.Queries.GetValueSetValuesQuery((SqlDbConfig_22)config, language, sqlCommand);
            }
            else if (config.MetaModel.Equals("2.3"))
            {
                sqlValueset = QueryLib_23.Queries.GetValueSetQuery((SqlDbConfig_23)config, language, sqlCommand);
                sqlValues = QueryLib_23.Queries.GetValueSetValuesQuery((SqlDbConfig_23)config, language, sqlCommand);

            }
            else if (config.MetaModel.Equals("2.4"))
            {
                sqlValueset = QueryLib_24.Queries.GetValueSetQuery((SqlDbConfig_24)config, language, sqlCommand);
                sqlValues = QueryLib_24.Queries.GetValueSetValuesQuery((SqlDbConfig_24)config, language, sqlCommand);
            }
        }

        private static ValueSet Parse(string name, DataSet valuesetDS, DataSet vsValue)
        {
            if (valuesetDS.Tables.Count == 0 || valuesetDS.Tables[0].Rows.Count < 1 || vsValue.Tables.Count == 0)
            {
                return null;
            }

            ValueSet valueset = new ValueSet();
            valueset.Id = name;
            valueset.Name = valuesetDS.Tables[0].Rows[0][1].ToString();


            //PresText came in version 2.1 and is optional  ...  desciption is up to 200 chars
            if (String.IsNullOrEmpty(valueset.Name))
            {
                var asPresText = valuesetDS.Tables[0].Rows[0][2].ToString(); ;
                int gridPosition = asPresText.IndexOf('#');
                if (gridPosition > 0)
                {
                    asPresText = asPresText.Substring(0, gridPosition);
                }
                valueset.Name = asPresText;
            }




            for (int i = 0; i < vsValue.Tables[0].Rows.Count; i++)
            {
                var v = new Models.Value();
                v.Code = vsValue.Tables[0].Rows[i][0].ToString();
                v.Text = vsValue.Tables[0].Rows[i][3] == DBNull.Value ? vsValue.Tables[0].Rows[i][4].ToString() : vsValue.Tables[0].Rows[i][3].ToString();
                valueset.Values.Add(v);
            }

            return valueset;
        }


    }

}









