using System;
using System.Collections.Generic;
using System.Data;

using PCAxis.Sql.DbConfig;


//This code is generated. 

namespace PCAxis.Sql.QueryLib_22
{
    public partial class MetaQuery
    {
        public Dictionary<string, SubTableRow> GetSubTableRows(string aMainTable, bool emptyRowSetIsOK)
        {
            Dictionary<string, SubTableRow> myOut = new Dictionary<string, SubTableRow>();
            SqlDbConfig dbconf = DB;
            string sqlString = GetSubTable_SQLString_NoWhere();
            //
            // WHERE STB.MainTable = '<aMainTable>'
            //
            sqlString += " WHERE " + DB.SubTable.MainTableCol.Is(aMainTable);

            DataSet ds = mSqlCommand.ExecuteSelect(sqlString);
            DataRowCollection myRows = ds.Tables[0].Rows;

            if (myRows.Count < 1 && !emptyRowSetIsOK)
            {
                throw new PCAxis.Sql.Exceptions.DbException(35, " MainTable = " + aMainTable);
            }

            foreach (DataRow sqlRow in myRows)
            {
                SubTableRow outRow = new SubTableRow(sqlRow, DB, mLanguageCodes);
                myOut.Add(outRow.SubTable, outRow);
            }
            return myOut;
        }

        private String GetSubTable_SQLString_NoWhere()
        {
            //SqlDbConfig dbconf = DB;   
            string sqlString = "SELECT ";


            sqlString +=
                DB.SubTable.SubTableCol.ForSelect() + ", " +
                DB.SubTable.MainTableCol.ForSelect() + ", " +
                DB.SubTable.PresTextCol.ForSelect() + ", " +
                DB.SubTable.CleanTableCol.ForSelect();


            foreach (String langCode in mLanguageCodes)
            {
                if (DB.isSecondaryLanguage(langCode))
                {
                    sqlString += ", " + DB.SubTableLang2.PresTextCol.ForSelectWithFallback(langCode, DB.SubTable.PresTextCol);
                }
            }

            sqlString += " /" + "*** SQLID: GetSubTableRows_01 ***" + "/ ";
            sqlString += " FROM " + DB.SubTable.GetNameAndAlias();

            foreach (String langCode in mLanguageCodes)
            {
                if (DB.isSecondaryLanguage(langCode))
                {
                    sqlString += " LEFT JOIN " + DB.SubTableLang2.GetNameAndAlias(langCode);
                    sqlString += " ON " + DB.SubTable.MainTableCol.Is(DB.SubTableLang2.MainTableCol, langCode) +
                                 " AND " + DB.SubTable.SubTableCol.Is(DB.SubTableLang2.SubTableCol, langCode);
                }
            }

            return sqlString;
        }
    }
}
