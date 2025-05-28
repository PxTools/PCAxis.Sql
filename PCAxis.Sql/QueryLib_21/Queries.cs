using System;
using System.Collections.Generic;

using PCAxis.Sql.DbConfig;
using PCAxis.Sql.Repositories;

namespace PCAxis.Sql.QueryLib_21
{
    internal class Queries : AbstractQueries
    {
        private readonly SqlDbConfig_21 _db;
        private readonly MetaQuery _metaQuery;
        internal Queries(SqlDbConfig db)
        {
            if (db == null) throw new ArgumentNullException("db");

            _db = (SqlDbConfig_21)db;
            InfoForDbConnection info = _db.GetInfoForDbConnection(String.Empty, String.Empty);
            _metaQuery = new MetaQuery(_db, info);
        }

        private void SetLang(string lang)
        {
            var langs = new System.Collections.Specialized.StringCollection();
            langs.Add(lang);
            _metaQuery.LanguageCodes = langs;
        }

        internal override string GetValueSetExistsIn(string lang, PxSqlCommand sqlCommand)
        {
            return $@"select '{lang}' As Language
                        from 
                         {_db.ValueSetLang2.GetNameAndAlias(lang).RemoveUnderscoreForDefaultLanguage()}
                        where
                         {_db.ValueSetLang2.ValueSetCol.Id(lang)} = {sqlCommand.GetParameterRef("aValueSet")}";
        }


        //Returns a ValueSet with some of the properties set
        internal override Models.ValueSet GetPartialValueset(string lang, string myValueSetId)
        {
            Models.ValueSet myOut = new Models.ValueSet();
            SetLang(lang);

            var cnmmRow = _metaQuery.GetValueSetRow(myValueSetId);

            myOut.Id = cnmmRow.ValueSet;
            myOut.Label = cnmmRow.texts[lang].PresText;
            //PresText came in version 2.1 and is optional  ...  desciption is up to 200 chars
            if (String.IsNullOrEmpty(myOut.Label))
            {
                var asPresText = cnmmRow.texts[lang].Description;
                int gridPosition = asPresText.IndexOf('#');
                if (gridPosition > 0)
                {
                    asPresText = asPresText.Substring(0, gridPosition);
                }
                myOut.Label = asPresText;
            }

            //cnmmRow.Elimination  = "N":no elimination , "A": Aggregate  , other use value  
            bool isN = cnmmRow.Elimination.Equals(_db.Codes.EliminationN);

            myOut.Elimination = !isN;
            if (!(isN || cnmmRow.Elimination.Equals(_db.Codes.EliminationA)))
            {
                myOut.EliminationValueCode = cnmmRow.Elimination;
            }
            return myOut;

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

        internal override Models.Grouping FixGrouping(string lang, string mGroupingId, List<Models.GroupedValue> groupedValues)
        {
            throw new NotImplementedException();
        }

        internal override string GetGroupingValuesQuery(string lang, PxSqlCommand sqlCommand)
        {
            throw new NotImplementedException();

        }

        internal override string GetMenuLookupTablesQuery(string lang)
        {
            if (!_db.isSecondaryLanguage(lang))
            {
                return $@"SELECT 
                            {_db.MenuSelection.MenuCol.ForSelect()}, 
                            {_db.MainTable.MainTableCol.ForSelect()}, 
                            {_db.MainTable.TableIdCol.ForSelect()} 
                        FROM 
                            {_db.MainTable.GetNameAndAlias()} 
                            JOIN {_db.MenuSelection.GetNameAndAlias()} ON {_db.MenuSelection.SelectionCol.Id()} = {_db.MainTable.MainTableCol.Id()}";
            }
            else
            {
                return $@"SELECT 
                            {_db.MenuSelection.MenuCol.ForSelect()}, 
                            {_db.MainTable.MainTableCol.ForSelect()}, 
                            {_db.MainTable.TableIdCol.ForSelect()} 
                    FROM 
                            {_db.MainTable.GetNameAndAlias()} 
                            JOIN 
                {_db.MenuSelectionLang2.GetNameAndAlias(lang)} ON {_db.MainTable.MainTableCol.Id()} = {_db.MainTableLang2.MainTableCol.Id(lang)} 
                            JOIN {_db.MenuSelection.GetNameAndAlias()} ON {_db.MenuSelection.SelectionCol.Id()} = {_db.MainTable.MainTableCol.Id()}
                    WHERE 
                            {_db.MainTableLang2.StatusCol.Id(lang)} = '{_db.Codes.Yes}'";
            }
        }
        internal override string GetMenuLookupFolderQuery(string lang)
        {
            if (!_db.isSecondaryLanguage(lang))
            {
                return $@"SELECT 
                            {_db.MenuSelection.MenuCol.ForSelect()}, 
                            {_db.MenuSelection.SelectionCol.ForSelect()}, 
                            {_db.MenuSelection.SelectionCol.ForSelect()} 
                        FROM 
                            {_db.MenuSelection.GetNameAndAlias()}
                        WHERE 
                            {_db.MenuSelection.LevelNoCol.Id()} NOT IN (SELECT {_db.MetaAdm.ValueCol} FROM {_db.MetaAdm.GetNameAndAlias()} WHERE upper({_db.MetaAdm.PropertyCol.Id()}) = 'MENULEVELS')";
            }
            else
            {
                return $@"SELECT 
                            {_db.MenuSelectionLang2.MenuCol.ForSelect(lang)}, 
                            {_db.MenuSelectionLang2.SelectionCol.ForSelect(lang)}, 
                            {_db.MenuSelectionLang2.SelectionCol.ForSelect(lang)}
                        FROM 
                            {_db.MenuSelectionLang2.GetNameAndAlias(lang)} 
                        JOIN 
                            {_db.MenuSelectionLang2.GetNameAndAlias(lang)} ON {_db.MenuSelectionLang2.MenuCol.Id(lang)} = {_db.MenuSelection.MenuCol.Id()} AND {_db.MenuSelectionLang2.SelectionCol.Id(lang)} = {_db.MenuSelection.SelectionCol.Id()}
                        WHERE 
                            {_db.MenuSelection.LevelNoCol.Id()} NOT IN (SELECT {_db.MetaAdm.ValueCol} FROM {_db.MetaAdm.GetNameAndAlias()} WHERE upper({_db.MetaAdm.PropertyCol.Id()}) = 'MENULEVELS')";
            }
        }
        internal override string GetTablesPublishedSinceQuery(PxSqlCommand sqlCommand)
        {
            throw new NotImplementedException();
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
