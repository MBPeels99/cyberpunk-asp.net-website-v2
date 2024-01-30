using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NityCityWeb.Models
{
    public enum BookingStatus
    {
        Confirmed,
        BookedIn,
        Completed,
        Cancelled
        // add more as needed
    }

    public class Booking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookingId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User User { get; set; }

        [ForeignKey(nameof(District))]
        public int DistrictId { get; set; }
        public District District { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime TripStartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime TripEndDate { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Number of travelers must be between 1 and 10.")]
        public int NumberOfTravelers { get; set; }

        [Required]
        public BookingStatus Status { get; set; } // Using the enum here

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }
    }
}
