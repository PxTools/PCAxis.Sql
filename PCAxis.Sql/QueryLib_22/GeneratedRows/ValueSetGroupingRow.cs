using System;
using System.Data;

using PCAxis.Sql.DbConfig;

//This code is generated. 

namespace PCAxis.Sql.QueryLib_22
{

    /// <summary>
    /// Holds the attributes for ValueSetGrouping. (This entity is language independent.) 
    /// 
    /// The table connects value set to grouping. 
    /// </summary>
    public class ValueSetGroupingRow
    {
        private String mValueSet;
        /// <summary>
        /// Name of the stored value set\n.\nSee description of table ValueSet. 
        /// </summary>
        public String ValueSet
        {
            get { return mValueSet; }
        }
        private String mGrouping;
        /// <summary>
        /// Name of grouping.\nSee further in the description of the table Grouping.
        /// </summary>
        public String Grouping
        {
            get { return mGrouping; }
        }

        public ValueSetGroupingRow(DataRow myRow, SqlDbConfig_22 dbconf)
        {
            this.mValueSet = myRow[dbconf.ValueSetGrouping.ValueSetCol.Label()].ToString();
            this.mGrouping = myRow[dbconf.ValueSetGrouping.GroupingCol.Label()].ToString();
        }
    }
}
