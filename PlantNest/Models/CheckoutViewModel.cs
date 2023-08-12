using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlantNest.Models
{
    public class CheckoutViewModel
    {
        public string CreditCardNumber { get; set; }
        public string CCV { get; set; }
        public string ExpiryDate { get; set; }
        public string PIN { get; set; }
    }
}