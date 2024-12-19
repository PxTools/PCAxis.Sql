using System.Collections.Generic;

namespace PCAxis.Sql.Models
{
    public class Grouping
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public List<GroupedValue> Values { get; set; }

        /// <summary>
        /// Always contains the primary language and the language of the request. Other secondary languages is added if the grouping is translated to that language. 
        /// </summary>
        public List<string> AvailableLanguages { get; set; }

        public Grouping()
        {
            Values = new List<GroupedValue>();
            AvailableLanguages = new List<string>();
        }

    }
}
