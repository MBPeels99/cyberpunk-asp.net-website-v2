using System.ComponentModel.DataAnnotations.Schema;

namespace NityCityWeb.Models
{
    public class District
    {
        public int DistrictId { get; set; }
        public string DistrictName { get; set; }
        public string Description { get; set; }
        public string ImageOne { get; set; }
        public string ImageTwo { get; set; }
        public string BackImage { get; set; }
        public string ImageMap { get; set; }
        public int? TotalStars { get; set; }
        public int? TotalVotes { get; set; }

        public string ShortDescription { get; set; }

        [NotMapped]
        public List<string> ImageUrls { get; set; }

        [NotMapped] // This property is calculated and not stored in the database
        public decimal? AverageRating
        {
            get
            {
                if (TotalVotes.HasValue && TotalVotes > 0)
                {
                    return TotalStars / (decimal)TotalVotes;
                }
                return null; // No rating if there are no votes
            }
        }
    }
}
