using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace hoistmt.Models.Tenant
{
    [Table("lineitems")]
    public class LineItem
    {
        [Key]
        public long Id { get; set; }

        [Column("invoice_id")]
        public int invoice_id { get; set; }  // Changed to int to match Invoice.invoice_id

        public string? Title { get; set; }
        public decimal? Rate { get; set; }
        public int? Hours { get; set; }
        public string? Type { get; set; }

        [ForeignKey("invoice_id")]
        [JsonIgnore] 
        public virtual Invoice? Invoice { get; set; }
    }
}