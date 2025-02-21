using System.Collections.Generic;

namespace PCAxis.Sql.Models
{
    public class Grouping
    {
        public string Id { get; set; }
        public string Label { get; set; }

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


        /*  

       valueset:
       {
         "id": "vs_FylkerSvalbardIalt",
         "label": "Alle fylker ",
         "values": [
             { "code": "Ialt", "label": "I alt",  "valueMap": [ "Ialt" ] },
             { "code": "30", "label": "Viken", "valueMap": [ "30" ] },
             ....

        grouping:
          Aggregation : 
          {
            "id": "agg_FylkerSvIaltB",
            "label": "Fylker 1972-2019",
            "values": [
             { "code": "FylkerSvIaltB", "label": "Fylker 1972-2019", "valueMap": ["01", "Ialt", "03", "04",
             .......  
        
        grouping:
          selection: 
          {
             "id": "agg_FylkerSvIaltB",
             "label": "Fylker 1972-2019",
             "values": [
                 { "code": "Ialt", "label": "I alt", "valueMap": ["Ialt"] },
                 { "code": "30","label": "Viken", "valueMap": ["30"] },
                 ....

        */

    }
}
