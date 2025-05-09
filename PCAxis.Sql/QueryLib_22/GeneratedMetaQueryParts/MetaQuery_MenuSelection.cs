using System;
using System.Data;


//This code is generated. 

namespace PCAxis.Sql.QueryLib_22
{
    public partial class MetaQuery
    {
        /* For SubjectArea*/
        //returns the single "row" found when all PKs are spesified
        public MenuSelectionRow GetMenuSelectionRow(string aMenu, string aSelection)
        {
            //SqlDbConfig dbconf = DB;
            string sqlString = GetMenuSelection_SQLString_NoWhere();
            sqlString += " WHERE " + DB.MenuSelection.MenuCol.Is(aMenu) +
                             " AND " + DB.MenuSelection.SelectionCol.Is(aSelection);

            DataSet ds = mSqlCommand.ExecuteSelect(sqlString);
            DataRowCollection myRows = ds.Tables[0].Rows;
            if (myRows.Count != 1)
            {
                throw new PCAxis.Sql.Exceptions.DbException(36, " Menu = " + aMenu + " Selection = " + aSelection);
            }

            MenuSelectionRow myOut = new MenuSelectionRow(myRows[0], DB, mLanguageCodes);
            return myOut;
        }


        private String GetMenuSelection_SQLString_NoWhere()
        {
            //SqlDbConfig dbconf = DB;   
            string sqlString = "SELECT ";


            sqlString +=
                DB.MenuSelection.MenuCol.ForSelect() + ", " +
                DB.MenuSelection.SelectionCol.ForSelect() + ", " +
                DB.MenuSelection.PresTextCol.ForSelect() + ", " +
                DB.MenuSelection.PresTextSCol.ForSelect() + ", " +
                DB.MenuSelection.DescriptionCol.ForSelect() + ", " +
                DB.MenuSelection.LevelNoCol.ForSelect() + ", " +
                DB.MenuSelection.SortCodeCol.ForSelect() + ", " +
                DB.MenuSelection.PresentationCol.ForSelect() + ", " +
                DB.MenuSelection.InternalIdCol.ForSelect();


            foreach (String langCode in mLanguageCodes)
            {
                if (DB.isSecondaryLanguage(langCode))
                {
                    sqlString += ", " + DB.MenuSelectionLang2.PresTextCol.ForSelectWithFallback(langCode, DB.MenuSelection.PresTextCol);
                    sqlString += ", " + DB.MenuSelectionLang2.PresTextSCol.ForSelectWithFallback(langCode, DB.MenuSelection.PresTextSCol);
                    sqlString += ", " + DB.MenuSelectionLang2.DescriptionCol.ForSelectWithFallback(langCode, DB.MenuSelection.DescriptionCol);
                    sqlString += ", " + DB.MenuSelectionLang2.SortCodeCol.ForSelectWithFallback(langCode, DB.MenuSelection.SortCodeCol);
                    sqlString += ", " + DB.MenuSelectionLang2.PresentationCol.ForSelectWithFallback(langCode, DB.MenuSelection.PresentationCol);
                }
            }

            sqlString += " /" + "*** SQLID: GetMenuSelectionRow_01 ***" + "/ ";
            sqlString += " FROM " + DB.MenuSelection.GetNameAndAlias();

            foreach (String langCode in mLanguageCodes)
            {
                if (DB.isSecondaryLanguage(langCode))
                {
                    sqlString += " LEFT JOIN " + DB.MenuSelectionLang2.GetNameAndAlias(langCode);
                    sqlString += " ON " + DB.MenuSelection.MenuCol.Is(DB.MenuSelectionLang2.MenuCol, langCode) +
                                 " AND " + DB.MenuSelection.SelectionCol.Is(DB.MenuSelectionLang2.SelectionCol, langCode);
                }
            }

            return sqlString;
        }
    }
}
