using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pbl3.Data;

namespace Pbl3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/stations
        [HttpGet]
        public async Task<IActionResult> GetAllStations()
        {
            var stations = await _context.Stations
                .Select(s => new
                {
                    s.StationID,
                    s.Name,
                    s.AddressDetail,
                    s.Province,
                    s.Type,
                    s.Latitude,
                    s.Longitude
                })
                .ToListAsync();

            return Ok(stations);
        }

        // GET: api/stations/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStationById(Guid id)
        {
            var station = await _context.Stations
                .Where(s => s.StationID == id)
                .Select(s => new
                {
                    s.StationID,
                    s.Name,
                    s.AddressDetail,
                    s.Province,
                    s.Type,
                    s.Latitude,
                    s.Longitude
                })
                .FirstOrDefaultAsync();

            if (station == null)
            {
                return NotFound(new { message = "Station not found" });
            }

            return Ok(station);
        }

        // GET: api/stations/search?province=xxx&name=xxx
        [HttpGet("search")]
        public async Task<IActionResult> SearchStations([FromQuery] string? province, [FromQuery] string? name)
        {
            var query = _context.Stations.AsQueryable();

            if (!string.IsNullOrEmpty(province))
            {
                query = query.Where(s => s.Province!.Contains(province));
            }

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(s => s.Name.Contains(name));
            }

            var stations = await query
                .Select(s => new
                {
                    s.StationID,
                    s.Name,
                    s.AddressDetail,
                    s.Province,
                    s.Type,
                    s.Latitude,
                    s.Longitude
                })
                .ToListAsync();

            return Ok(stations);
        }

        // GET: api/stations/provinces
        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var provinces = await _context.Stations
                .Where(s => s.Province != null)
                .Select(s => s.Province!)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();

            return Ok(provinces);
        }
    }
}
