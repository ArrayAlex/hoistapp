using System.ComponentModel.DataAnnotations;

namespace hoistmt.Models
{

    public class invoiceEntry
    {
        [Key]
        public int entry_id { get; set; }
        public int invoice_id { get; set; }
        public int quantity { get; set; }
        public int unit_price { get; set; }

        public int total { get; set; }
        
    }
}

