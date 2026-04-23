using System.ComponentModel.DataAnnotations;

namespace Pbl3.Dtos
{
    public class TripSearchDto
    {
        public Guid TripId { get; set; }

        public string BusCompanyName { get; set; } = default!;

        public string Origin { get; set; } = default!;

        public string Destination { get; set; } = default!;

        public DateTime DepartureTime { get; set; }

        public decimal Price { get; set; }

        public int AvailableSeats { get; set; }

        public double Score { get; set; }
    }

    public enum TripSortBy
    {
        Default = 0,
        EarliestDeparture = 1,
        LatestDeparture = 2,
        HighestRating = 3,
        PriceAsc = 4,
        PriceDesc = 5
    }

    public enum TimeRangeFilter
    {
        EarlyMorning = 1, // 00:00 - 06:00
        Morning = 2,      // 06:00 - 12:00
        Afternoon = 3,    // 12:00 - 18:00
        Evening = 4       // 18:00 - 24:00
    }

    public class TripSearchQuery
    {
        [Required]
        public string Origin { get; set; } = default!;

        [Required]
        public string Destination { get; set; } = default!;

        [Required]
        public DateTime DepartureDate { get; set; }

        public TripSortBy SortBy { get; set; } = TripSortBy.Default;

        public List<TimeRangeFilter>? TimeRanges { get; set; }

        public List<Guid>? BusCompanyIds { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public List<string>? SeatTypes { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }

    public class TripSearchResult
    {
        public int TotalResults { get; set; }

        public List<TripSearchDto> Items { get; set; } = new();
    }
}