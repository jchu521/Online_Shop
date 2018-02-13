using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCManukauTech.Models
{
    public class Checkout
    {

        public string CustomerName { get; set; }
        public string AddressStreet { get; set; }
        public string Location { get; set; }
        public string Country { get; set; }
        public string PostCode { get; set; }
        public string CardOwner { get; set; }
        public string CardType { get; set; }
        public string CardNumber { get; set; }
        public string CSC { get; set; }

        //150609 JPC added to display GrandTotal on Checkout page
        public decimal GrandTotal { get; set; }
     
    }
}