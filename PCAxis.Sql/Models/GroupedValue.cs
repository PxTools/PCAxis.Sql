using System.Collections.Generic;

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
