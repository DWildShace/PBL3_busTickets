using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pbl3.Data;
using Pbl3.Models;

namespace Pbl3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoutesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/routes
        [HttpGet]
        public async Task<IActionResult> GetAllRoutes()
        {
            var routes = await _context.BusRoutes
                .Include(r => r.BusCompany)
                .Include(r => r.BusRouteStops)
                .ThenInclude(rs => rs.Station)
                .Where(r => r.IsActive)
                .Select(r => new
                {
                    r.RouteID,
                    r.RouteName,
                    r.DistanceEstimate,
                    r.DurationEstimate,
                    CompanyName = r.BusCompany!.Name,
                    Stops = r.BusRouteStops.OrderBy(rs => rs.StopOrder).Select(rs => new
                    {
                        rs.StopOrder,
                        StationName = rs.Station!.Name,
                        rs.DurationFromStart
                    }).ToList()
                })
                .ToListAsync();

            return Ok(routes);
        }

        // GET: api/routes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRouteById(Guid id)
        {
            var route = await _context.BusRoutes
                .Include(r => r.BusCompany)
                .Include(r => r.BusRouteStops)
                    .ThenInclude(rs => rs.Station)
                .Where(r => r.RouteID == id)
                .Select(r => new
                {
                    r.RouteID,
                    r.RouteName,
                    r.DistanceEstimate,
                    r.DurationEstimate,
                    r.IsActive,
                    CompanyName = r.BusCompany!.Name,
                    CompanyHotline = r.BusCompany.Hotline,
                    Stops = r.BusRouteStops.OrderBy(rs => rs.StopOrder).Select(rs => new
                    {
                        rs.StopOrder,
                        StationName = rs.Station!.Name,
                        rs.Station.Province,
                        rs.DurationFromStart
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (route == null)
            {
                return NotFound(new { message = "Route not found" });
            }

            return Ok(route);
        }

        // GET: api/routes/search?origin=xxx&destination=xxx
        [HttpGet("search")]
        public async Task<IActionResult> SearchRoutes([FromQuery] string? origin, [FromQuery] string? destination)
        {
            var query = _context.BusRoutes
                .Include(r => r.BusCompany)
                .Include(r => r.BusRouteStops)
                    .ThenInclude(rs => rs.Station)
                .Where(r => r.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(origin) && !string.IsNullOrEmpty(destination))
            {
                query = query.Where(r =>
                    r.BusRouteStops.Any(rs => rs.Station!.Province!.Contains(origin) || rs.Station!.Name.Contains(origin)) &&
                    r.BusRouteStops.Any(rs => rs.Station!.Province!.Contains(destination) || rs.Station!.Name.Contains(destination))
                );
            }

            var routes = await query
                .Select(r => new
                {
                    r.RouteID,
                    r.RouteName,
                    r.DistanceEstimate,
                    r.DurationEstimate,
                    CompanyName = r.BusCompany!.Name,
                    OriginStation = r.BusRouteStops
                        .OrderBy(rs => rs.StopOrder)
                        .Select(rs => rs.Station!.Name)
                        .FirstOrDefault(),
                    DestinationStation = r.BusRouteStops
                        .OrderByDescending(rs => rs.StopOrder)
                        .Select(rs => rs.Station!.Name)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(routes);
        }
    }
}
