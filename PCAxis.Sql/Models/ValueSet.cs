using System;
using System.Collections.Generic;
using System.Text;

namespace PCAxis.Sql.Models
{
    public class ValueSet
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public List<Value> Values { get; set; }

        public ValueSet()
        {
            Values = new List<Value>();
        }

    }
}
