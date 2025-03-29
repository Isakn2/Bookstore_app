using System.ComponentModel.DataAnnotations; // Add this using directive

namespace BookStoreTester.Models
{
    public class BookRequest
    {
        [RegularExpression("en|de|fr", ErrorMessage = "Invalid locale. Use 'en', 'de', or 'fr'")]
        public string Locale { get; set; } = "en";
        
        [Range(0, int.MaxValue)]
        public int Seed { get; set; } = 0;
        
        [Range(0, 10)]
        public double AvgLikes { get; set; } = 0;
        
        [Range(0, 100)]
        public double AvgReviews { get; set; } = 0;
        
        [Range(1, 1000)]
        public int Page { get; set; } = 1;
        
        [Range(1, 100)]
        public int PageSize { get; set; } = 20;
        
        public bool GalleryView { get; set; } = false;
    }
}