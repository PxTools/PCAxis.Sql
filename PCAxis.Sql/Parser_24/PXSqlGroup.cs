﻿namespace PCAxis.Sql.Parser_24
{
    using System.Collections.Generic;

    /// <summary>Stores A = B + C + D +</summary>
    public class PXSqlGroup
    {
        /// <summary>The code of the Item to which the children belong</summary>
        private string parentCode;


        private int groupLevel;
        /// <summary>
        /// If Hierarchy groups level no
        /// </summary>
        public int GroupLevel
        {
            get { return groupLevel; }
            set { groupLevel = value; }
        }



        private bool isLeaf = false;

        /// <summary>
        /// True if parentcode = childCode
        /// </summary>
        public bool IsLeaf
        {
            get { return isLeaf; }
        }



        /// <summary>The list of codes of the children</summary>
        private List<string> childCodes = new List<string>();

        /// <summary>Initializes a new instance of the PXSqlGroup class,  with the given parentCode</summary>
        /// <param name="parentCode">The code of the parent item</param>
        internal PXSqlGroup(string parentCode)
        {
            this.parentCode = parentCode;
        }

        /// <summary>Gets the code of the Item to which the children belong</summary>
        public string ParentCode
        {
            get { return this.parentCode; }
        }

        /// <summary>Gets the list containing B,C,,, </summary>
        public IList<string> ChildCodes
        {
            get { return this.childCodes.AsReadOnly(); }
        }

        /// <summary>Adds a code to the list of codes of the children</summary>
        /// <param name="childCode">The code of child item</param>
        public void AddChildCode(string childCode)
        {
            this.childCodes.Add(childCode);
            if (parentCode.Equals(childCode))
            {
                isLeaf = true;
            }

        }
    }
}
