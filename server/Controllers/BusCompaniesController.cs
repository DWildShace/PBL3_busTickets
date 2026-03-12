using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pbl3.Data;

namespace Pbl3.Controllers
{
    [ApiController]
    [Route("api/bus_companies")]
    public class BusCompaniesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BusCompaniesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/bus_companies
        [HttpGet]
        public async Task<IActionResult> GetAllBusCompanies()
        {
            var companies = await _context.BusCompanies
                .Where(bc => bc.IsApproved)
                .Select(bc => new
                {
                    bc.CompanyID,
                    bc.Name,
                    bc.LicenseNumber,
                    bc.Hotline,
                    bc.IsApproved,
                    BusCount = bc.Buses.Count(b => b.IsActive),
                    RouteCount = bc.Routes.Count(r => r.IsActive)
                })
                .ToListAsync();

            return Ok(companies);
        }

        // GET: api/bus_companies/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBusCompanyById(Guid id)
        {
            var company = await _context.BusCompanies
                .Where(bc => bc.CompanyID == id)
                .Select(bc => new
                {
                    bc.CompanyID,
                    bc.Name,
                    bc.LicenseNumber,
                    bc.Hotline,
                    bc.IsApproved,
                    BusCount = bc.Buses.Count(b => b.IsActive),
                    RouteCount = bc.Routes.Count(r => r.IsActive),
                    Routes = bc.Routes.Where(r => r.IsActive).Select(r => new
                    {
                        r.RouteID,
                        r.RouteName,
                        r.DistanceEstimate,
                        r.DurationEstimate
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (company == null)
            {
                return NotFound(new { message = "Bus company not found" });
            }

            return Ok(company);
        }

        // GET: api/bus_companies/{id}/buses
        [HttpGet("{id}/buses")]
        public async Task<IActionResult> GetBusesByCompany(Guid id)
        {
            var companyExists = await _context.BusCompanies.AnyAsync(bc => bc.CompanyID == id);
            if (!companyExists)
            {
                return NotFound(new { message = "Bus company not found" });
            }

            var buses = await _context.Buses
                .Include(b => b.BusType)
                .Where(b => b.CompanyID == id && b.IsActive)
                .Select(b => new
                {
                    b.BusID,
                    b.PlateNumber,
                    BusType = new
                    {
                        b.BusType!.BusTypeID,
                        b.BusType.Name,
                        b.BusType.TotalSeats,
                        b.BusType.Description
                    },
                    b.IsActive
                })
                .ToListAsync();

            return Ok(buses);
        }

        // GET: api/bus_companies/{id}/routes
        [HttpGet("{id}/routes")]
        public async Task<IActionResult> GetRoutesByCompany(Guid id)
        {
            var companyExists = await _context.BusCompanies.AnyAsync(bc => bc.CompanyID == id);
            if (!companyExists)
            {
                return NotFound(new { message = "Bus company not found" });
            }

            var routes = await _context.BusRoutes
                .Include(r => r.BusRouteStops)
                    .ThenInclude(rs => rs.Station)
                .Where(r => r.CompanyID == id && r.IsActive)
                .Select(r => new
                {
                    r.RouteID,
                    r.RouteName,
                    r.DistanceEstimate,
                    r.DurationEstimate,
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
