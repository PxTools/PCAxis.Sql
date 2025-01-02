using System;

using PCAxis.Sql.DbConfig;
using PCAxis.Sql.Repositories;

namespace PCAxis.Sql.QueryLib_24
{
    internal class Queries : AbstractQueries
    {
        private readonly SqlDbConfig_24 _db;
        internal Queries(SqlDbConfig db)
        {
            if (db == null) throw new ArgumentNullException("db");

            _db = (SqlDbConfig_24)db;
        }

        internal override string GetValueSetExistsIn(string lang, PxSqlCommand sqlCommand)
        {
            return $@"select '{lang}' As Language
                        from 
                         {_db.ValueSetLang2.GetNameAndAlias(lang).RemoveUnderscoreForDefaultLanguage()}
                        where
                         {_db.ValueSetLang2.ValueSetCol.Id(lang)} = {sqlCommand.GetParameterRef("aValueSet")}";
        }

        internal override string GetValueSetQuery(string lang, PxSqlCommand sqlCommand)
        {
            return $@"select 
                         {_db.ValueSetLang2.ValueSetCol.Id(lang)} AS ValueSet,
                         {_db.ValueSetLang2.PresTextCol.Id(lang)} AS PresText,
                         {_db.ValueSetLang2.DescriptionCol.Id(lang)} AS Description
                        from 
                         {_db.ValueSetLang2.GetNameAndAlias(lang).RemoveUnderscoreForDefaultLanguage()}
                        where
                         {_db.ValueSetLang2.ValueSetCol.Id(lang)} = {sqlCommand.GetParameterRef("aValueSet")}";
        }

        internal override string GetValueSetValuesQuery(string lang, PxSqlCommand sqlCommand)
        {
            return $@"SELECT
                            {_db.ValueLang2.ValueCodeCol.Id(lang)} AS valuecode,
	                        {_db.ValueLang2.ValuePoolCol.Id(lang)}  AS valuepool,
	                        {_db.VSValueLang2.ValueCodeCol.Id(lang)}  AS valueset,
                            {_db.ValueLang2.ValueTextLCol.Id(lang)} AS valuetextl,
                            {_db.ValueLang2.ValueTextSCol.Id(lang)} AS valuetexts,
                            {_db.ValueLang2.SortCodeCol.Id(lang)}   AS sortcodevalue,
	                        {_db.VSValueLang2.ValueCodeCol.Id(lang)} AS sortcodevsvalue
                        FROM
                            {_db.ValueLang2.GetNameAndAlias(lang).RemoveUnderscoreForDefaultLanguage()}
                        JOIN 
                            {_db.VSValueLang2.GetNameAndAlias(lang).RemoveUnderscoreForDefaultLanguage()}
	                        ON ( {_db.ValueLang2.ValuePoolCol.Id(lang)} = {_db.VSValueLang2.ValuePoolCol.Id(lang)}
                                AND {_db.ValueLang2.ValueCodeCol.Id(lang)} = {_db.VSValueLang2.ValueCodeCol.Id(lang)} ) 
                        where {_db.VSValueLang2.ValueSetCol.Id(lang)} = {sqlCommand.GetParameterRef("aValueSet")}

                        ORDER BY
                            {_db.VSValueLang2.SortCodeCol.Id(lang)},
                            {_db.ValueLang2.SortCodeCol.Id(lang)},
                            {_db.ValueLang2.ValueCodeCol.Id(lang)}";

        }

        internal override string GetGroupingQuery(string lang, PxSqlCommand sqlCommand)
        {
            return $@"SELECT
	                        {_db.GroupingLang2.GroupingCol.Id(lang)} AS Grouping, 
	                        {_db.GroupingLang2.PresTextCol.Id(lang)} AS Name
                        FROM 
	                        {_db.GroupingLang2.GetNameAndAlias(lang).RemoveUnderscoreForDefaultLanguage()}
                        WHERE
	                        {_db.GroupingLang2.GroupingCol.Id(lang)} = {sqlCommand.GetParameterRef("aGrouping")}";
        }

        internal override string GetGroupingExistsIn(string lang, PxSqlCommand sqlCommand)
        {
            return $@"select '{lang}' As Language
                        from 
                          {_db.GroupingLang2.GetNameAndAlias(lang).RemoveUnderscoreForDefaultLanguage()}
                        where
                          {_db.GroupingLang2.GroupingCol.Id(lang)} = {sqlCommand.GetParameterRef("aGrouping")}";
        }


        internal override string GetGroupingValuesQuery(string lang, PxSqlCommand sqlCommand)
        {
            return $@"select 
                         {_db.ValueGroup.GroupCodeCol.Id()} AS GroupCode,
                         {_db.ValueGroup.ValueCodeCol.Id()} AS ValueCode,
                         {_db.ValueLang2.ValueTextLCol.Id(lang)} AS TEXTL,
                         {_db.ValueLang2.ValueTextSCol.Id(lang)} AS TEXTS
                        from {_db.ValueGroup.GetNameAndAlias().RemoveUnderscoreForDefaultLanguage()}
                         join {_db.ValueLang2.GetNameAndAlias(lang).RemoveUnderscoreForDefaultLanguage()} 
                                on {_db.ValueGroup.ValuePoolCol.Id()} = {_db.ValueLang2.ValuePoolCol.Id(lang)} and {_db.ValueGroup.GroupCodeCol.Id()} = {_db.ValueLang2.ValueCodeCol.Id(lang)}
                        where {_db.ValueGroup.GroupingCol.Id()} = {sqlCommand.GetParameterRef("aGrouping")}
                        ORDER BY
                        {_db.ValueGroup.SortCodeCol.Id()},
                        {_db.ValueLang2.SortCodeCol.Id(lang)},
                        {_db.ValueGroup.GroupCodeCol.Id()}";

        }
    }


    public static class TableLangFixer
    {
        public static string RemoveUnderscoreForDefaultLanguage(this string name)
        {
            if (name.Contains("_ "))
            {
                return name.Replace("_ ", " ");
            }
            return name;
        }
    }
}
