using DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.DTO.ReviewDTOs
{
    public class ReviewDTO
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int OrderId { get; set; }
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public static class ReviewExtensions
    {
        public static ReviewDTO ToReviewDTO(this Review review)
        {
            return new ReviewDTO
            {
                ProductId = review.ProductId,
                OrderId = review.OrderId,
                Rating = review.Rating,
                Comment = review.Comment,
            };
        }
    }
}
