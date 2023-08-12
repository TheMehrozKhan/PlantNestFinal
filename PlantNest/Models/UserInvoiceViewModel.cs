using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlantNest.Models
{
    public class UserInvoiceViewModel
    {
        public int in_id { get; set; }
        public int in_fk_user { get; set; }
        public int in_totallbill { get; set; }
        public DateTime in_date { get; set; }
        public int u_id { get; set; }
        public string u_name { get; set; }
        public int o_id { get; set; }
        public int o_date { get; set; }
    }
}