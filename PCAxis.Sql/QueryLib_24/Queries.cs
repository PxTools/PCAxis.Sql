﻿using System;

using PCAxis.Sql.DbConfig;

namespace PCAxis.Sql.QueryLib_24
{
    public class Queries
    {
        public static string GetValueSetExistsIn(SqlDbConfig_24 db, string lang, PxSqlCommand sqlCommand)
        {
            if (db == null) throw new ArgumentNullException("db");

            return $@"select '{lang}' As Language
                        from 
                         {db.ValueSetLang2.GetNameAndAlias(lang).RemoveUnderscoreForDefaultLanguage()}
                        where
                         {db.ValueSetLang2.ValueSetCol.Id(lang)} = {sqlCommand.GetParameterRef("aValueSet")}";
        }

        public static string GetValueSetQuery(SqlDbConfig_24 db, string lang, PxSqlCommand sqlCommand)
        {
            if (db == null) throw new ArgumentNullException("db");

            return $@"select 
                         {db.ValueSetLang2.ValueSetCol.Id(lang)} AS ValueSet,
                         {db.ValueSetLang2.PresTextCol.Id(lang)} AS PresText,
                         {db.ValueSetLang2.DescriptionCol.Id(lang)} AS Description
                        from 
                         {db.ValueSetLang2.GetNameAndAlias(lang).RemoveUnderscoreForDefaultLanguage()}
                        where
                         {db.ValueSetLang2.ValueSetCol.Id(lang)} = {sqlCommand.GetParameterRef("aValueSet")}";
        }

        public static string GetValueSetValuesQuery(SqlDbConfig_24 db, string lang, PxSqlCommand sqlCommand)
        {
            if (db == null) throw new ArgumentNullException("db");

            return $@"SELECT
                            {db.ValueLang2.ValueCodeCol.Id(lang)} AS valuecode,
	                        {db.ValueLang2.ValuePoolCol.Id(lang)}  AS valuepool,
	                        {db.VSValueLang2.ValueCodeCol.Id(lang)}  AS valueset,
                            {db.ValueLang2.ValueTextLCol.Id(lang)} AS valuetextl,
                            {db.ValueLang2.ValueTextSCol.Id(lang)} AS valuetexts,
                            {db.ValueLang2.SortCodeCol.Id(lang)}   AS sortcodevalue,
	                        {db.VSValueLang2.ValueCodeCol.Id(lang)} AS sortcodevsvalue
                        FROM
                            {db.ValueLang2.GetNameAndAlias(lang).RemoveUnderscoreForDefaultLanguage()}
                        JOIN 
                            {db.VSValueLang2.GetNameAndAlias(lang).RemoveUnderscoreForDefaultLanguage()}
	                        ON ( {db.ValueLang2.ValuePoolCol.Id(lang)} = {db.VSValueLang2.ValuePoolCol.Id(lang)}
                                AND {db.ValueLang2.ValueCodeCol.Id(lang)} = {db.VSValueLang2.ValueCodeCol.Id(lang)} ) 
                        where {db.VSValueLang2.ValueSetCol.Id(lang)} = {sqlCommand.GetParameterRef("aValueSet")}

                        ORDER BY
                            {db.VSValueLang2.SortCodeCol.Id(lang)},
                            {db.ValueLang2.SortCodeCol.Id(lang)},
                            {db.ValueLang2.ValueCodeCol.Id(lang)}";

        }


        public static string GetGroupingQuery(SqlDbConfig_24 db, string lang, PxSqlCommand sqlCommand)
        {
            if (db == null) throw new ArgumentNullException("db");

            return $@"SELECT
	                        {db.GroupingLang2.GroupingCol.Id(lang)} AS Grouping, 
	                        {db.GroupingLang2.PresTextCol.Id(lang)} AS Name
                        FROM 
	                        {db.GroupingLang2.GetNameAndAlias(lang).RemoveUnderscoreForDefaultLanguage()}
                        WHERE
	                        {db.GroupingLang2.GroupingCol.Id(lang)} = {sqlCommand.GetParameterRef("aGrouping")}";
        }

        public static string GetGroupingExistsIn(SqlDbConfig_24 db, string lang, PxSqlCommand sqlCommand)
        {
            if (db == null) throw new ArgumentNullException("db");

            return $@"select '{lang}' As Language
                        from 
                          {db.GroupingLang2.GetNameAndAlias(lang).RemoveUnderscoreForDefaultLanguage()}
                        where
                          {db.GroupingLang2.GroupingCol.Id(lang)} = {sqlCommand.GetParameterRef("aGrouping")}";
        }


        public static string GetGroupingValuesQuery(SqlDbConfig_24 db, string lang, PxSqlCommand sqlCommand)
        {
            if (db == null) throw new ArgumentNullException("db");

            return $@"select 
                         {db.ValueGroup.GroupCodeCol.Id()} AS GroupCode,
                         {db.ValueGroup.ValueCodeCol.Id()} AS ValueCode,
                         {db.ValueLang2.ValueTextLCol.Id(lang)} AS TEXTL,
                         {db.ValueLang2.ValueTextSCol.Id(lang)} AS TEXTS
                        from {db.ValueGroup.GetNameAndAlias().RemoveUnderscoreForDefaultLanguage()}
                         join {db.ValueLang2.GetNameAndAlias(lang).RemoveUnderscoreForDefaultLanguage()} 
                                on {db.ValueGroup.ValuePoolCol.Id()} = {db.ValueLang2.ValuePoolCol.Id(lang)} and {db.ValueGroup.GroupCodeCol.Id()} = {db.ValueLang2.ValueCodeCol.Id(lang)}
                        where {db.ValueGroup.GroupingCol.Id()} = {sqlCommand.GetParameterRef("aGrouping")}
                        ORDER BY
                        {db.ValueGroup.SortCodeCol.Id()},
                        {db.ValueLang2.SortCodeCol.Id(lang)},
                        {db.ValueGroup.GroupCodeCol.Id()}";

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
