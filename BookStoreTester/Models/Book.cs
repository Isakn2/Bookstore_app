using System;
using System.Collections.Generic;


namespace BookStoreTester.Models
{
    public class Book
    {
        public int Index { get; set; }
        public string? ISBN { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public double AverageLikes { get; set; }
        public double AverageReviews { get; set; }
        public List<string> Reviews { get; set; } = new List<string>();
        public List<string> Reviewers { get; set; } = new List<string>();
        public int ActualLikes { get; set; }
        public string? CoverImageUrl { get; set; } = "https://via.placeholder.com/300x450.png?text=No+Cover";
    }
}