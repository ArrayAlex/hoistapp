using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hoistmt.Models
{
    public class AdhocEntry
    {
        [Key]
        public int AdHocEntryID { get; set; } // Primary key with auto-increment

        public int invoice_id { get; set; } // Foreign key linking to Invoice table


        [StringLength(255)]
        public string Description { get; set; } // Description of the entry, maximum length 255


        public decimal Amount { get; set; } // Amount with precision 10 and scale 2
    }
}