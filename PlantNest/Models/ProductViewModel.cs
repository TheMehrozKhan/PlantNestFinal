using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlantNest.Models
{
    public class ProductViewModel
    {
        public int cat_id { get; set; }
        public string cat_name { get; set; }
        public int pro_id { get; set; }
        public string pro_name { get; set; }
        public Nullable<int> pro_price { get; set; }
        public string pro_image { get; set; }
        public string pro_desc { get; set; }
        public int u_id { get; set; }
        public string u_name { get; set; }

        public int ad_id { get; set; }
        public string ad_name { get; set; }
        public List<string> ImageUrls { get; set; }
    }
}