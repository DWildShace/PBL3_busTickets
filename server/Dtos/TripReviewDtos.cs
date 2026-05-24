using System.ComponentModel.DataAnnotations;

namespace Pbl3.Dtos
{
    public class TripReviewItemDto
    {
        public Guid ReviewId { get; set; }

        public int RatingScore { get; set; }

        public string? Comment { get; set; }

        public string ReviewerName { get; set; } = default!;

        public DateTime CreatedAt { get; set; }
    }

    public class TripReviewsResponseDto
    {
        public double AverageRating { get; set; }

        public int TotalReviews { get; set; }

        public List<TripReviewItemDto> Items { get; set; } = [];
    }

    public class CreateReviewDto
    {
        [Required]
        public Guid BookingId { get; set; }

        [Required]
        public Guid TripId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }

    public class CreateReviewResponseDto
    {
        public Guid ReviewId { get; set; }

        public int Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Message { get; set; } = default!;
    }
}
