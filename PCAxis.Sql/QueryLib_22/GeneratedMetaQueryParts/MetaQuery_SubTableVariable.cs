using System;
using System.Collections.Generic;
using System.Data;

using PCAxis.Sql.DbConfig;


//This code is generated. 

namespace PCAxis.Sql.QueryLib_22
{
    public partial class MetaQuery
    {
        //returns the single "row" found when all PKs are spesified
        public SubTableVariableRow GetSubTableVariableRow(string aMainTable, string aSubTable, string aVariable)
        {
            //SqlDbConfig dbconf = DB;
            string sqlString = GetSubTableVariable_SQLString_NoWhere();
            sqlString += " WHERE " + DB.SubTableVariable.MainTableCol.Is(aMainTable) +
                             " AND " + DB.SubTableVariable.SubTableCol.Is(aSubTable) +
                             " AND " + DB.SubTableVariable.VariableCol.Is(aVariable);

            DataSet ds = mSqlCommand.ExecuteSelect(sqlString);
            DataRowCollection myRows = ds.Tables[0].Rows;
            if (myRows.Count != 1)
            {
                throw new PCAxis.Sql.Exceptions.DbException(36, " MainTable = " + aMainTable + " SubTable = " + aSubTable + " Variable = " + aVariable);
            }

            SubTableVariableRow myOut = new SubTableVariableRow(myRows[0], DB);
            return myOut;
        }

        public Dictionary<string, SubTableVariableRow> GetSubTableVariableRowskeyVariable(string aMainTable, string aSubTable, bool emptyRowSetIsOK)
        {
            Dictionary<string, SubTableVariableRow> myOut = new Dictionary<string, SubTableVariableRow>();
            SqlDbConfig dbconf = DB;
            string sqlString = GetSubTableVariable_SQLString_NoWhere();
            //
            // WHERE STV.MainTable = '<aMainTable>'
            //    AND STV.SubTable = '<aSubTable>'
            //
            sqlString += " WHERE " + DB.SubTableVariable.MainTableCol.Is(aMainTable) +
                         " AND " + DB.SubTableVariable.SubTableCol.Is(aSubTable);

            DataSet ds = mSqlCommand.ExecuteSelect(sqlString);
            DataRowCollection myRows = ds.Tables[0].Rows;

            if (myRows.Count < 1 && !emptyRowSetIsOK)
            {
                throw new PCAxis.Sql.Exceptions.DbException(35, " MainTable = " + aMainTable + " SubTable = " + aSubTable);
            }

            foreach (DataRow sqlRow in myRows)
            {
                SubTableVariableRow outRow = new SubTableVariableRow(sqlRow, DB);
                myOut.Add(outRow.Variable, outRow);
            }
            return myOut;
        }

        private String GetSubTableVariable_SQLString_NoWhere()
        {
            //SqlDbConfig dbconf = DB;   
            string sqlString = "SELECT ";


            sqlString +=
                DB.SubTableVariable.MainTableCol.ForSelect() + ", " +
                DB.SubTableVariable.SubTableCol.ForSelect() + ", " +
                DB.SubTableVariable.VariableCol.ForSelect() + ", " +
                DB.SubTableVariable.ValueSetCol.ForSelect() + ", " +
                DB.SubTableVariable.VariableTypeCol.ForSelect() + ", " +
                DB.SubTableVariable.StoreColumnNoCol.ForSelect();

            sqlString += " /" + "*** SQLID: GetSubTableVariableRowskeyVariable_01 ***" + "/ ";
            sqlString += " FROM " + DB.SubTableVariable.GetNameAndAlias();
            return sqlString;
        }
    }
}
