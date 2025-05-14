using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;

using Microsoft.IdentityModel.Tokens;

using PCAxis.Sql.DbConfig;
using PCAxis.Sql.Repositories;

namespace PCAxis.Sql.QueryLib_23
{
    internal class Queries : AbstractQueries
    {
        private readonly SqlDbConfig_23 _db;
        private readonly MetaQuery _metaQuery;
        internal Queries(SqlDbConfig db)
        {
            if (db == null) throw new ArgumentNullException("db");

            _db = (SqlDbConfig_23)db;
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
            SetLang(lang);
            var cnmmRow = _metaQuery.GetGroupingRow(mGroupingId);
            Models.Grouping myOut = new Models.Grouping();
            myOut.Id = mGroupingId;
            myOut.Label = cnmmRow.texts[lang].PresText;

            bool isSelectionGrouping = cnmmRow.GroupPres.Equals(_db.Codes.GroupPresI);

            if (isSelectionGrouping)
            {
                // remaking the groupedValues
                myOut.Values = RemakeValues(lang, cnmmRow.ValuePool, groupedValues);
            }
            else
            {
                myOut.Values = groupedValues;
            }
            return myOut;
        }

        /// <summary>
        /// For selection groupings: each childcode of the incoming mothers is "promoted" to a mother with itself as the only child.
        /// A sorted list of these new mothers is returned.
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="valuePoolId"></param>
        /// <param name="groupedValues"></param>
        /// <returns></returns>
        private List<Models.GroupedValue> RemakeValues(string lang, string valuePoolId, List<Models.GroupedValue> groupedValues)
        {
            List<Models.GroupedValue> myOut = new List<Models.GroupedValue>();

            //the Codes is needed as a StringCollection since GetValueRowsByValuePool is old :-)


            StringCollection stringCollection = GetAllChildrenAsStringCollection(groupedValues);

            List<ValueRowHM> unsortedChildren = _metaQuery.GetValueRowsByValuePool(valuePoolId, stringCollection, "this param is not used by method only 2.2");
            List<ValueRowHM> sortedChildren = unsortedChildren.Where(x => x.texts != null && x.texts.Count > 0 && x.texts[lang] != null)
                .OrderBy(x => x.texts[lang].SortCode).ToList();

            if (sortedChildren.Count != unsortedChildren.Count)
            {
                throw new DataException("Some values are missing texts in the db.");
            }

            foreach (ValueRowHM row in sortedChildren)
            {
                Models.GroupedValue tmp = new Models.GroupedValue();

                tmp.Text = row.texts[lang].ValueTextL;
                if (tmp.Text.IsNullOrEmpty())
                {
                    tmp.Text = row.texts[lang].ValueTextS;
                }
                tmp.Code = row.ValueCode;
                tmp.Codes.Add(tmp.Code);
                myOut.Add(tmp);
            }

            return myOut;
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
                            JOIN {_db.SecondaryLanguage.GetNameAndAlias()} ON {_db.MainTable.MainTableCol.Id()} = {_db.SecondaryLanguage.MainTableCol.Id()} 
                            JOIN {_db.MenuSelection.GetNameAndAlias()} ON {_db.MenuSelection.SelectionCol.Id()} = {_db.MainTable.MainTableCol.Id()}
                        WHERE 
                            {_db.SecondaryLanguage.LanguageCol.Id()} = '{lang}' AND
                            {_db.SecondaryLanguage.CompletelyTranslatedCol.Id()} = '{_db.Codes.Yes}'";
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
                            {_db.MenuSelection.LevelNoCol.Id()} NOT IN (SELECT {_db.MetaAdm.ValueCol.Id()} FROM {_db.MetaAdm.GetNameAndAlias()} WHERE upper({_db.MetaAdm.PropertyCol.Id()}) = 'MENULEVELS')";
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
                            {_db.MenuSelection.GetNameAndAlias()} ON {_db.MenuSelectionLang2.MenuCol.Id(lang)} = {_db.MenuSelection.MenuCol.Id()} AND {_db.MenuSelectionLang2.SelectionCol.Id(lang)} = {_db.MenuSelection.SelectionCol.Id()}
                        WHERE 
                            {_db.MenuSelection.LevelNoCol.Id()} NOT IN (SELECT {_db.MetaAdm.ValueCol.Id()} FROM {_db.MetaAdm.GetNameAndAlias()} WHERE upper({_db.MetaAdm.PropertyCol.Id()}) = 'MENULEVELS')";
            }

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
