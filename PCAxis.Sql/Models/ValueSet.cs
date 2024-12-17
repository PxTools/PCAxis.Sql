using System.Collections.Generic;

namespace PCAxis.Sql.Models
{
    public class ValueSet
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public List<Value> Values { get; set; }

        /// <summary>
        /// Always contains the primary language and the language of the request. Other secondary languages is added if the valueset is translated to that language. 
        /// </summary>
        public List<string> AvailableLanguages { get; set; }

        public ValueSet()
        {
            Values = new List<Value>();
            AvailableLanguages = new List<string>();
        }

    }
}
