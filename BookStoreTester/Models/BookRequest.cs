// BookRequest.cs
using System.ComponentModel.DataAnnotations; // Add this using directive

namespace BookStoreTester.Models
{
    public class BookRequest
    {
        public string Locale { get; set; } = "en";
        public int Seed { get; set; } = 0;
        public double AvgLikes { get; set; } = 0;  // Ensure this exists
        public double AvgReviews { get; set; } = 0; // Ensure this exists
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}