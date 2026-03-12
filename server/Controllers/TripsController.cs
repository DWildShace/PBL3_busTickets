using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pbl3.Data;
using Pbl3.Enums;

namespace Pbl3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TripsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/trips
        [HttpGet]
        public async Task<IActionResult> GetAllTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var trips = await _context.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r!.BusRouteStops)
                    .ThenInclude(rs => rs.Station)
                .Include(t => t.Bus)
                .Include(t => t.BusType)
                .Where(t => t.Status == TripStatus.Scheduled && t.DepartureDate >= DateOnly.FromDateTime(DateTime.UtcNow))
                .OrderBy(t => t.DepartureDate)
                .ThenBy(t => t.DepartureTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.TripID,
                    RouteName = t.Route!.RouteName,
                    t.DepartureDate,
                    t.DepartureTime,
                    t.ArrivalTime,
                    t.Status,
                    BusType = t.BusType!.Name,
                    TotalSeats = t.BusType.TotalSeats,
                    BusPlate = t.Bus!.PlateNumber,
                    OriginStation = t.Route.BusRouteStops
                        .OrderBy(rs => rs.StopOrder)
                        .Select(rs => rs.Station!.Name)
                        .FirstOrDefault(),
                    DestinationStation = t.Route.BusRouteStops
                        .OrderByDescending(rs => rs.StopOrder)
                        .Select(rs => rs.Station!.Name)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(trips);
        }

        // GET: api/trips/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTripById(Guid id)
        {
            var trip = await _context.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r!.BusCompany)
                .Include(t => t.Route)
                    .ThenInclude(r => r!.BusRouteStops)
                    .ThenInclude(rs => rs.Station)
                .Include(t => t.Bus)
                .Include(t => t.BusType)
                    .ThenInclude(bt => bt!.SeatLayouts)
                .Where(t => t.TripID == id)
                .Select(t => new
                {
                    t.TripID,
                    t.DepartureDate,
                    t.DepartureTime,
                    t.ArrivalTime,
                    t.Status,
                    Route = new
                    {
                        t.Route!.RouteID,
                        t.Route.RouteName,
                        t.Route.DistanceEstimate,
                        t.Route.DurationEstimate,
                        CompanyName = t.Route.BusCompany!.Name,
                        CompanyHotline = t.Route.BusCompany.Hotline,
                        Stops = t.Route.BusRouteStops.OrderBy(rs => rs.StopOrder).Select(rs => new
                        {
                            rs.StopOrder,
                            StationName = rs.Station!.Name,
                            rs.Station.Province,
                            rs.DurationFromStart
                        }).ToList()
                    },
                    Bus = new
                    {
                        t.Bus!.BusID,
                        t.Bus.PlateNumber
                    },
                    BusType = new
                    {
                        t.BusType!.BusTypeID,
                        t.BusType.Name,
                        t.BusType.TotalSeats,
                        t.BusType.Description
                    }
                })
                .FirstOrDefaultAsync();

            if (trip == null)
            {
                return NotFound(new { message = "Trip not found" });
            }

            return Ok(trip);
        }

        // GET: api/trips/search?routeId=xxx&date=2026-03-06
        [HttpGet("search")]
        public async Task<IActionResult> SearchTrips([FromQuery] Guid? routeId, [FromQuery] DateOnly? date)
        {
            var query = _context.Trips
                .Include(t => t.Route)
                    .ThenInclude(r => r!.BusRouteStops)
                    .ThenInclude(rs => rs.Station)
                .Include(t => t.Bus)
                .Include(t => t.BusType)
                .Where(t => t.Status == TripStatus.Scheduled)
                .AsQueryable();

            if (routeId.HasValue)
            {
                query = query.Where(t => t.RouteID == routeId.Value);
            }

            if (date.HasValue)
            {
                query = query.Where(t => t.DepartureDate == date.Value);
            }

            var trips = await query
                .OrderBy(t => t.DepartureDate)
                .ThenBy(t => t.DepartureTime)
                .Select(t => new
                {
                    t.TripID,
                    RouteName = t.Route!.RouteName,
                    t.DepartureDate,
                    t.DepartureTime,
                    t.ArrivalTime,
                    t.Status,
                    BusType = t.BusType!.Name,
                    TotalSeats = t.BusType.TotalSeats,
                    BusPlate = t.Bus!.PlateNumber,
                    OriginStation = t.Route.BusRouteStops
                        .OrderBy(rs => rs.StopOrder)
                        .Select(rs => rs.Station!.Name)
                        .FirstOrDefault(),
                    DestinationStation = t.Route.BusRouteStops
                        .OrderByDescending(rs => rs.StopOrder)
                        .Select(rs => rs.Station!.Name)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(trips);
        }

        // GET: api/trips/{id}/seats
        [HttpGet("{id}/seats")]
        public async Task<IActionResult> GetAvailableSeats(Guid id)
        {
            var trip = await _context.Trips
                .Include(t => t.BusType)
                    .ThenInclude(bt => bt!.SeatLayouts)
                .FirstOrDefaultAsync(t => t.TripID == id);

            if (trip == null)
            {
                return NotFound(new { message = "Trip not found" });
            }

            // Get booked seats (by SeatLayoutID)
            var bookedSeatLayoutIds = await _context.Tickets
                .Where(t => t.TripID == id && t.Status != TicketStatus.Cancelled)
                .Select(t => t.SeatLayoutID)
                .ToListAsync();

            // Get held seats (temporary holds)
            var heldSeatLayoutIds = await _context.SeatHolds
                .Where(sh => sh.TripID == id && 
                             sh.Status == SeatHoldStatus.Held && 
                             sh.ExpiresAt > DateTime.UtcNow)
                .Select(sh => sh.SeatLayoutID)
                .ToListAsync();

            var allSeats = trip.BusType!.SeatLayouts.Select(sl => new
            {
                SeatLabel = sl.SeatLabel,
                sl.SeatType,
                sl.Floor,
                sl.PositionX,
                sl.PositionY,
                IsBooked = bookedSeatLayoutIds.Contains(sl.LayoutID),
                IsHeld = heldSeatLayoutIds.Contains(sl.LayoutID),
                IsAvailable = !bookedSeatLayoutIds.Contains(sl.LayoutID) && !heldSeatLayoutIds.Contains(sl.LayoutID)
            }).OrderBy(s => s.Floor).ThenBy(s => s.PositionY).ThenBy(s => s.PositionX).ToList();

            return Ok(new
            {
                TripID = trip.TripID,
                BusType = new
                {
                    trip.BusType.Name,
                    trip.BusType.TotalSeats
                },
                AvailableSeatsCount = allSeats.Count(s => s.IsAvailable),
                BookedSeatsCount = bookedSeatLayoutIds.Count,
                HeldSeatsCount = heldSeatLayoutIds.Count,
                Seats = allSeats
            });
        }
    }
}
