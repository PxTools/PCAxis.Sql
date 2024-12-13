using System.Collections.Generic;

namespace PCAxis.Sql.Models
{
    public class ValueSet
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public List<Value> Values { get; set; }

        //To do: public List<string> AvailableLanguages { get; set; }

        public ValueSet()
        {
            Values = new List<Value>();
            //  AvailableLanguages = new List<string>();
        }

    }
}
