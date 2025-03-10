using System.Data;

using PCAxis.Sql.DbConfig;



namespace PCAxis.Sql.QueryLib_22
{

    public partial class MetaQuery
    {
        #region for GetValueGroupMaxValueLevel
        public string GetValueGroupMaxValueLevel(string aGrouping, bool emptyRowSetIsOK)
        {
            string myOut;
            SqlDbConfig dbconf = DB;
            string sqlString = "SELECT ";
            sqlString +=
                "MAX(" +
                DB.ValueGroup.GroupLevelCol.Id() + ") ";
            sqlString += " FROM " + DB.ValueGroup.GetNameAndAlias();
            sqlString += " WHERE " + DB.ValueGroup.GroupingCol.Is(aGrouping);
            DataSet ds = mSqlCommand.ExecuteSelect(sqlString);
            DataRowCollection myRows = ds.Tables[0].Rows;
            if (myRows.Count < 1 && !emptyRowSetIsOK)
            {
                throw new PCAxis.Sql.Exceptions.DbException(35, " Grouping = " + aGrouping);
            }
            myOut = myRows[0][0].ToString();
            return myOut;
        }
        #endregion for GetValueGroupMaxValueLevel
    }
}
