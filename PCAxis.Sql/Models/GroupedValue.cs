using System;
using System.Collections.Generic;
using System.Text;

namespace PCAxis.Sql.Models
{
    public class GroupedValue : Value
    {
        public List<string> Codes { get; set; }

        public GroupedValue() : base()
        {
            Codes = new List<string>();
        }
    }
}
