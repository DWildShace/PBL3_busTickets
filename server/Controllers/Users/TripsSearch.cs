using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pbl3.Data;
using Pbl3.Dtos;
using Pbl3.Enums;

namespace Pbl3.Controllers
{
    [ApiController]
    [Route("api/trips/search")]
    [Tags("Search")]
    [Authorize]
    public class TripsSearchController : ControllerBase
    {
        private readonly ITripSearchService _searchService;

        public TripsSearchController(ITripSearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] TripSearchQuery query)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (query.MinPrice > query.MaxPrice)
            {
                return BadRequest(new
                {
                    message = "MinPrice cannot be greater than MaxPrice."
                });
            }

            var result = await _searchService.SearchTripsAsync(query);

            return Ok(result);
        }
    }

    public interface ITripSearchService
    {
        Task<TripSearchResult> SearchTripsAsync(TripSearchQuery request);
    }

    public class TripSearchService : ITripSearchService
    {
        private readonly ApplicationDbContext _context;

        public TripSearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TripSearchResult> SearchTripsAsync(TripSearchQuery request)
        {
            var origin = request.Origin.Trim();
            var destination = request.Destination.Trim();
            var searchDate = request.DepartureDate.Date;
searchDate = DateTime.SpecifyKind(searchDate, DateTimeKind.Utc);

            var originPattern = EscapeLikePattern(origin);
            var destinationPattern = EscapeLikePattern(destination);

            IQueryable<Models.Trip> query = _context.Trips
                .AsNoTracking()
                .Where(t => t.Status != TripStatus.Cancelled)
                .Where(t => t.Route != null)
                .Where(t => t.Route!.BusCompany != null)
                .Where(t => t.BusType != null)
                .Where(t => t.DepartureTime.Date == searchDate);

            // Search origin + destination
            query = query.Where(t =>
                t.Route!.RouteName != null &&
                EF.Functions.ILike(t.Route.RouteName, $"%{originPattern}%") &&
                EF.Functions.ILike(t.Route.RouteName, $"%{destinationPattern}%"));

            // Filter bus company
            if (request.BusCompanyIds?.Any() == true)
            {
                query = query.Where(t =>
                    request.BusCompanyIds.Contains(t.Route!.CompanyID));
            }

            // Filter seat type
            if (request.SeatTypes?.Any() == true)
            {
                query = query.Where(t =>
                    request.SeatTypes.Contains(t.BusType!.Name));
            }

            // Filter price
            if (request.MinPrice.HasValue)
            {
                query = query.Where(t =>
                    t.Tickets.Any(x =>
                        x.Status != TicketStatus.Cancelled &&
                        x.FinalPrice >= request.MinPrice.Value));
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(t =>
                    t.Tickets.Any(x =>
                        x.Status != TicketStatus.Cancelled &&
                        x.FinalPrice <= request.MaxPrice.Value));
            }

            // Filter time range
            if (request.TimeRanges?.Any() == true)
            {
                query = query.Where(t =>
                    request.TimeRanges.Any(range =>

                        (range == TimeRangeFilter.EarlyMorning &&
                         t.DepartureTime.TimeOfDay >= TimeSpan.Zero &&
                         t.DepartureTime.TimeOfDay < TimeSpan.FromHours(6))

                        ||

                        (range == TimeRangeFilter.Morning &&
                         t.DepartureTime.TimeOfDay >= TimeSpan.FromHours(6) &&
                         t.DepartureTime.TimeOfDay < TimeSpan.FromHours(12))

                        ||

                        (range == TimeRangeFilter.Afternoon &&
                         t.DepartureTime.TimeOfDay >= TimeSpan.FromHours(12) &&
                         t.DepartureTime.TimeOfDay < TimeSpan.FromHours(18))

                        ||

                        (range == TimeRangeFilter.Evening &&
                         t.DepartureTime.TimeOfDay >= TimeSpan.FromHours(18) &&
                         t.DepartureTime.TimeOfDay < TimeSpan.FromHours(24))
                    ));
            }

            // Projection
            var resultQuery = query.Select(t => new TripSearchDto
            {
                TripId = t.TripID,

                BusCompanyName = t.Route!.BusCompany!.Name,

                Origin = t.Route.RouteName.Split('-')[0].Trim(),

                Destination = t.Route.RouteName.Contains("-")
                    ? t.Route.RouteName.Split('-')[1].Trim()
                    : "",

                DepartureTime = t.DepartureTime,

                Price = t.Tickets
                    .Where(x => x.Status != TicketStatus.Cancelled)
                    .OrderBy(x => x.FinalPrice)
                    .Select(x => x.FinalPrice)
                    .FirstOrDefault(),

                AvailableSeats =
                    t.BusType!.TotalSeats -
                    t.Tickets.Count(x => x.Status != TicketStatus.Cancelled),

                Score = t.Reviews.Any()
                    ? t.Reviews.Average(r => r.RatingScore)
                    : 0
            });

            // Total count
            var total = await resultQuery.CountAsync();

            // Sorting
            resultQuery = request.SortBy switch
            {
                TripSortBy.EarliestDeparture =>
                    resultQuery.OrderBy(x => x.DepartureTime),

                TripSortBy.LatestDeparture =>
                    resultQuery.OrderByDescending(x => x.DepartureTime),

                TripSortBy.HighestRating =>
                    resultQuery.OrderByDescending(x => x.Score),

                TripSortBy.PriceAsc =>
                    resultQuery.OrderBy(x => x.Price),

                TripSortBy.PriceDesc =>
                    resultQuery.OrderByDescending(x => x.Price),

                _ =>
                    resultQuery
                        .OrderByDescending(x => x.Score)
                        .ThenBy(x => x.DepartureTime)
            };

            // Paging
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

            var items = await resultQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new TripSearchResult
            {
                TotalResults = total,
                Items = items
            };
        }

        // =========================
        // Helpers
        // =========================

        private static string EscapeLikePattern(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");
        }

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var normalized = text.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    builder.Append(c);
            }

            return builder
                .ToString()
                .Normalize(NormalizationForm.FormC)
                .ToLowerInvariant();
        }
    }
}