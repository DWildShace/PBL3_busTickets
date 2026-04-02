using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pbl3.Enums;

namespace Pbl3.Models
{
    public class Station
    {
        [Key]
        public Guid StationID { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public string? AddressDetail { get; set; }

        [Column("province_code")]
        public string? ProvinceCode { get; set; }
        public Province? Province { get; set; }

        [Column("district_code")]
        public string? DistrictCode { get; set; }
        public District? District { get; set; }

        [Column("ward_code")]
        public string? WardCode { get; set; }
        public Ward? Ward { get; set; }

        public StationType Type { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public ICollection<BusRouteStop> BusRouteStops { get; set; } = new List<BusRouteStop>();
    }
}
