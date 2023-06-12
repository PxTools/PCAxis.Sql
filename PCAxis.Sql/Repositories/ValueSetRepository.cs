using Oracle.ManagedDataAccess.Client;
using PCAxis.Paxiom;
using PCAxis.Sql.DbClient;
using PCAxis.Sql.DbConfig;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using static PCAxis.Sql.Repositories.GroupingRepository;
using PCAxis.Sql.Models;
using System.Security.Cryptography;

namespace PCAxis.Sql.Repositories
{
    public class ValueSetRepository
    {
        private string _database;
        public ValueSetRepository(string database)
        {
            _database = database;
        }

        public ValueSet GetValueSet(string name, string language)
        {
            //validate input
            if (name == null || language == null)
            {
                return null;
            }

            ValueSet valueset = null;
            string sqlValueset;
            string sqlValues;

            var config = SqlDbConfigsStatic.DataBases[_database];
            GetQueries(language, out sqlValueset, out sqlValues, config);

            InfoForDbConnection info;

            info = config.GetInfoForDbConnection(config.GetDefaultConnString());
            var cmd = new PxSqlCommandForTempTables(info.DataBaseType, info.DataProvider, info.ConnectionString);

            System.Data.Common.DbParameter[] parameters = new System.Data.Common.DbParameter[1];
            parameters[0] = cmd.GetStringParameter("valueset", name);
            var valuesetDS = cmd.ExecuteSelect(sqlValueset, parameters);

            parameters = new System.Data.Common.DbParameter[1];
            parameters[0] = cmd.GetStringParameter("valueset", name);
            var valueDS = cmd.ExecuteSelect(sqlValues, parameters);

            valueset = Parse(name, valuesetDS, valueDS);

            return valueset;
        }

        private static void GetQueries(string language, out string sqlValueset, out string sqlValues, SqlDbConfig config)
        {
            sqlValueset = string.Empty;
            sqlValues = string.Empty;

            if (config.MetaModel.Equals("2.1"))
            {
                //TODO maybe fix it
                throw new NotImplementedException("CNMM 2.1");
                //var meta = new PCAxis.Sql.QueryLib_21.MetaQuery((PCAxis.Sql.DbConfig.SqlDbConfig_21)config, config.GetInfoForDbConnection("", ""));
                //meta.LanguageCodes = config.GetAllLanguages();

            }
            else if (config.MetaModel.Equals("2.2"))
            {

            }
            else if (config.MetaModel.Equals("2.3"))
            {
                sqlValueset = QueryLib_23.Queries.GetValueSetQuery((SqlDbConfig_23)config, language);
                sqlValues = QueryLib_23.Queries.GetValueSetValuesQuery((SqlDbConfig_23)config, language);

            }
            else if (config.MetaModel.Equals("2.4"))
            {
                sqlValueset = QueryLib_24.Queries.GetValueSetQuery((SqlDbConfig_24)config, language);
                sqlValues = QueryLib_24.Queries.GetValueSetValuesQuery((SqlDbConfig_24)config, language);
            }
        }

        private static ValueSet Parse(string name, DataSet valuesetDS, DataSet vsValue)
        {
            ValueSet valueset = new ValueSet();
            valueset.Id = name;
            valueset.Name = valuesetDS.Tables[0].Rows[0][1].ToString();


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









