using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NityCityWeb.Models
{
    public class Pricing
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PricingId { get; set; }

        [ForeignKey("District")]
        public int DistrictId { get; set; }
        public District District { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PricePerPerson { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [StringLength(255)]
        public string Description { get; set; }
        public decimal DefaultPrice { get; set; }
    }
}
