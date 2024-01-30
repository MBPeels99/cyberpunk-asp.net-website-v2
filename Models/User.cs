using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NityCityWeb.Models
{
    public class User
    {
        public User()
        {
            // Set the default value for SecurityLevel
            SecurityLevel = -1;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string Country { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(100)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Range(-1, Int32.MaxValue)]
        public int SecurityLevel { get; set; }
    }
}
