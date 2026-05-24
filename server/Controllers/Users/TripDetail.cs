using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pbl3.Data;
using Pbl3.Dtos;
using Pbl3.Enums;
using Pbl3.Services;

namespace Pbl3.Controllers
{
    [ApiController]
    [Route("api/trips")]
    [Tags("Trips")]
    public class TripDetailController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserContext _currentUserContext;

        public TripDetailController(
            ApplicationDbContext context,
            ICurrentUserContext currentUserContext
        )
        {
            _context = context;
            _currentUserContext = currentUserContext;
        }

        [HttpGet("{tripId:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TripDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTripDetail(Guid tripId)
        {
            var utcNow = DateTime.UtcNow;

            var trip = await _context
                .Trips.AsNoTracking()
                .Where(t => t.TripID == tripId)
                .Select(t => new
                {
                    t.TripID,
                    t.RouteID,
                    t.BusTypeID,
                    t.DepartureDate,
                    t.DepartureTime,
                    t.ArrivalTime,
                    t.Status,
                    t.BasePrice,
                    t.CancellationPolicy,
                    t.Notes,
                    CompanyId = t.Route!.CompanyID,
                    BusCompanyName = t.Route.BusCompany!.Name,
                    AllowPayOnBoard = t.Route.BusCompany.AllowPayOnBoard,
                    BusTypeName = t.BusType!.Name,
                    BusTypeDescription = t.BusType.Description,
                    BusTypeAmenityIds = t
                        .BusType.BusTypeAmenities.Select(bta => bta.AmenityID)
                        .ToList(),
                    RouteName = t.Route.RouteName,
                    TotalSeats = t.BusType.TotalSeats,
                    SoldSeats = t.Tickets.Count(ticket =>
                        ticket.Status == TicketStatus.Issued
                        || ticket.Status == TicketStatus.CheckedIn
                    ),
                    HeldSeats = t.SeatHolds.Count(hold =>
                        hold.Status == SeatHoldStatus.Held && hold.ExpiresAt > utcNow
                    ),
                    LowestPrice = t.Tickets.Where(ticket =>
                            ticket.Status == TicketStatus.Issued
                            || ticket.Status == TicketStatus.CheckedIn
                        )
                        .Select(ticket => (decimal?)ticket.FinalPrice)
                        .Min()
                        ?? 0,
                    Rating = t.Reviews.Where(review => review.Status == ReviewStatus.Approved)
                        .Select(review => (double?)review.RatingScore)
                        .Average()
                        ?? 0,
                    ReviewCount = t.Reviews.Count(review => review.Status == ReviewStatus.Approved),
                    Images = t.Bus != null
                        ? t
                            .Bus.BusImages.OrderBy(img => img.ImageID)
                            .Select(img => img.ImageURL)
                            .ToList()
                        : new List<string>(),
                    RouteStops = t
                        .Route.BusRouteStops.Select(stop => new
                        {
                            stop.StationID,
                            StationName = stop.Station!.Name,
                            stop.Station.ProvinceCode,
                            stop.Station.DistrictCode,
                            stop.Station.WardCode,
                            stop.Station.AddressDetail,
                            stop.StopOrder,
                            stop.IsPickUp,
                            stop.IsDropOff,
                            stop.DurationFromStart,
                        })
                        .ToList(),
                })
                .FirstOrDefaultAsync();

            if (trip == null)
            {
                return NotFound(new { message = "Trip not found" });
            }

            if (trip.Status != TripStatus.Scheduled)
            {
                return NotFound(new { message = "Trip is not available" });
            }

            var amenities = await _context
                .Amenities.AsNoTracking()
                .Where(a => trip.BusTypeAmenityIds.Contains(a.AmenityID) && a.IsActive)
                .OrderBy(a => a.DisplayOrder)
                .Select(a => new AmenityDto
                {
                    AmenityId = a.AmenityID,
                    Name = a.Name,
                    Description = a.Description,
                    IconName = a.IconName,
                    Category = a.Category,
                })
                .ToListAsync();

            var seats = await BuildTripSeatsAsync(trip.TripID, trip.BusTypeID, utcNow);

            var pickupStops = trip
                .RouteStops.Where(stop => stop.IsPickUp)
                .OrderBy(stop => stop.StopOrder)
                .Select(stop => new TripDetailRouteStopDto
                {
                    StationId = stop.StationID,
                    StationName = stop.StationName,
                    ProvinceCode = stop.ProvinceCode,
                    DistrictCode = stop.DistrictCode,
                    WardCode = stop.WardCode,
                    AddressDetail = stop.AddressDetail,
                    StopOrder = stop.StopOrder,
                    IsPickUp = stop.IsPickUp,
                    IsDropOff = stop.IsDropOff,
                    DurationFromStart = stop.DurationFromStart,
                })
                .ToList();

            var dropoffStops = trip
                .RouteStops.Where(stop => stop.IsDropOff)
                .OrderBy(stop => stop.StopOrder)
                .Select(stop => new TripDetailRouteStopDto
                {
                    StationId = stop.StationID,
                    StationName = stop.StationName,
                    ProvinceCode = stop.ProvinceCode,
                    DistrictCode = stop.DistrictCode,
                    WardCode = stop.WardCode,
                    AddressDetail = stop.AddressDetail,
                    StopOrder = stop.StopOrder,
                    IsPickUp = stop.IsPickUp,
                    IsDropOff = stop.IsDropOff,
                    DurationFromStart = stop.DurationFromStart,
                })
                .ToList();

            var result = new TripDetailDto
            {
                TripId = trip.TripID,
                RouteId = trip.RouteID,
                CompanyId = trip.CompanyId,
                BusCompanyName = trip.BusCompanyName,
                AllowPayOnBoard = trip.AllowPayOnBoard,
                BusTypeName = trip.BusTypeName,
                BusTypeDescription = trip.BusTypeDescription,
                RouteName = trip.RouteName,
                DepartureDate = trip.DepartureDate,
                DepartureTime = trip.DepartureTime,
                ArrivalTime = trip.ArrivalTime,
                DurationMinutes = Math.Max(
                    0,
                    (int)Math.Round((trip.ArrivalTime - trip.DepartureTime).TotalMinutes)
                ),
                TotalSeats = trip.TotalSeats,
                AvailableSeats = Math.Max(0, trip.TotalSeats - trip.SoldSeats - trip.HeldSeats),
                BasePrice = trip.BasePrice,
                LowestPrice = trip.LowestPrice,
                Rating = trip.Rating,
                ReviewCount = trip.ReviewCount,
                Amenities = amenities,
                Images = trip.Images,
                Seats = seats,
                PickupStops = pickupStops,
                DropoffStops = dropoffStops,
                CancellationPolicy = trip.CancellationPolicy,
                Notes = trip.Notes,
            };

            return Ok(result);
        }

        [HttpGet("{tripId:guid}/seats")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<TripSeatDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTripSeats(Guid tripId)
        {
            var trip = await _context
                .Trips.AsNoTracking()
                .Where(t => t.TripID == tripId)
                .Select(t => new
                {
                    t.TripID,
                    t.BusTypeID,
                    t.Status,
                })
                .FirstOrDefaultAsync();

            if (trip == null)
            {
                return NotFound(new { message = "Trip not found" });
            }

            if (trip.Status != TripStatus.Scheduled)
            {
                return NotFound(new { message = "Trip is not available" });
            }

            return Ok(await BuildTripSeatsAsync(trip.TripID, trip.BusTypeID, DateTime.UtcNow));
        }

        [HttpGet("{tripId:guid}/reviews")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TripReviewsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTripReviews(Guid tripId)
        {
            var tripExists = await _context.Trips.AsNoTracking().AnyAsync(t => t.TripID == tripId);

            if (!tripExists)
            {
                return NotFound(new { message = "Trip not found" });
            }

            var reviews = await _context
                .Reviews.AsNoTracking()
                .Where(r => r.TripID == tripId && r.Status == ReviewStatus.Approved)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new TripReviewItemDto
                {
                    ReviewId = r.ReviewID,
                    RatingScore = r.RatingScore,
                    Comment = r.Comment,
                    ReviewerName =
                        !string.IsNullOrWhiteSpace(r.User!.FullName) ? r.User.FullName!
                        : !string.IsNullOrWhiteSpace(r.Booking!.ContactName) ? r.Booking.ContactName
                        : r.Booking!.ContactEmail,
                    CreatedAt = r.CreatedAt,
                })
                .ToListAsync();

            return Ok(
                new TripReviewsResponseDto
                {
                    AverageRating = reviews.Count > 0 ? reviews.Average(r => r.RatingScore) : 0,
                    TotalReviews = reviews.Count,
                    Items = reviews,
                }
            );
        }

        [HttpPost("/api/reviews")]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(typeof(CreateReviewResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            var userId = _currentUserContext.GetRequiredUserId();

            var booking = await _context
                .Bookings.Include(b => b.Tickets)
                .FirstOrDefaultAsync(b => b.BookingID == dto.BookingId && b.UserID == userId);

            if (booking == null)
            {
                return NotFound(new { message = "Booking not found" });
            }

            var hasTripTicket = booking.Tickets.Any(ticket => ticket.TripID == dto.TripId);
            if (!hasTripTicket)
            {
                return BadRequest(new { message = "Booking does not belong to this trip" });
            }

            if (booking.Status != BookingStatus.Paid)
            {
                return BadRequest(new { message = "Only paid bookings can be reviewed" });
            }

            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.TripID == dto.TripId);
            if (trip == null)
            {
                return NotFound(new { message = "Trip not found" });
            }

            if (trip.Status != TripStatus.Completed && trip.ArrivalTime > DateTime.UtcNow)
            {
                return BadRequest(new { message = "You can only review completed trips" });
            }

            var alreadyReviewed = await _context.Reviews.AnyAsync(r =>
                r.BookingID == dto.BookingId && r.TripID == dto.TripId
            );
            if (alreadyReviewed)
            {
                return BadRequest(new { message = "You have already reviewed this trip" });
            }

            var review = new Models.Review
            {
                BookingID = dto.BookingId,
                TripID = dto.TripId,
                RatingScore = dto.Rating,
                Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim(),
                UserID = userId,
                Status = ReviewStatus.Pending,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Created(
                $"/api/reviews/{review.ReviewID}",
                new CreateReviewResponseDto
                {
                    ReviewId = review.ReviewID,
                    Status = (int)review.Status,
                    CreatedAt = review.CreatedAt,
                    Message = "Review submitted successfully and is awaiting approval",
                }
            );
        }

        private async Task<List<TripSeatDto>> BuildTripSeatsAsync(
            Guid tripId,
            Guid busTypeId,
            DateTime utcNow
        )
        {
            var unavailableSeatIds = await _context
                .Tickets.AsNoTracking()
                .Where(ticket =>
                    ticket.TripID == tripId
                    && (
                        ticket.Status == TicketStatus.Issued
                        || ticket.Status == TicketStatus.CheckedIn
                    )
                )
                .Select(ticket => ticket.SeatLayoutID)
                .Concat(
                    _context
                        .SeatHolds.AsNoTracking()
                        .Where(hold =>
                            hold.TripID == tripId
                            && hold.Status == SeatHoldStatus.Held
                            && hold.ExpiresAt > utcNow
                        )
                        .Select(hold => hold.SeatLayoutID)
                )
                .Distinct()
                .ToListAsync();

            return await _context
                .SeatLayouts.AsNoTracking()
                .Where(layout => layout.BusTypeID == busTypeId)
                .OrderBy(layout => layout.Floor)
                .ThenBy(layout => layout.PositionY)
                .ThenBy(layout => layout.PositionX)
                .Select(layout => new TripSeatDto
                {
                    LayoutId = layout.LayoutID,
                    SeatLabel = layout.SeatLabel,
                    Floor = layout.Floor,
                    SeatType = layout.SeatType,
                    PositionX = layout.PositionX,
                    PositionY = layout.PositionY,
                    IsAvailable = !unavailableSeatIds.Contains(layout.LayoutID),
                })
                .ToListAsync();
        }
    }
}
