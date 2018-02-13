namespace MVCManukauTech.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public partial class OrderDetailsQueryForCart
    {
        public int OrderId { get; set; }
        public int LineNumber { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        //140901 JPC add this.  A mystery how the code ran before with ImageFileName in the SQL but not in this model class.
        //  Need to display in the view forces to give it some attention and add the next line
        public string ImageFileName { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<decimal> UnitCost { get; set; }
        public float Discount { get; set; }
    }
}

