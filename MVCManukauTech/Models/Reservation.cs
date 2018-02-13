using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCManukauTech.Models
{
    public class Reservation
    {
        public int TransactionId { get; set; }
        public bool IsReserved { get; set; }
        public string Notes { get; set; }
    }
}