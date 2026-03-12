using Microsoft.EntityFrameworkCore;
using Pbl3.Models;
using Pbl3.Enums;

namespace Pbl3.Data
{
    public class DataSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(ApplicationDbContext context, ILogger<DataSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            // Check if data already exists
            if (await _context.Users.AnyAsync())
            {
                _logger.LogInformation("Database already has data. Skipping seed.");
                return;
            }

            _logger.LogInformation("Starting to seed data...");

            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedBusCompaniesAsync();
            await SeedBusTypesAsync();
            await SeedSeatLayoutsAsync();
            await SeedBusesAsync();
            await SeedBusImagesAsync();
            await SeedStationsAsync();
            await SeedRoutesAsync();
            await SeedRouteStopsAsync();
            await SeedTripsAsync();
            await SeedBookingsAsync();
            await SeedPassengersAsync();
            await SeedTicketsAsync();
            await SeedPaymentIntentsAsync();
            await SeedRefundsAsync();
            await SeedReviewsAsync();
            await SeedNotificationsAsync();
            await SeedSeatHoldsAsync();

            _logger.LogInformation("Successfully seeded comprehensive test data!");
        }

        private async Task SeedRolesAsync()
        {
            var roles = new List<Role>
            {
                new Role { RoleID = Guid.NewGuid(), RoleName = UserRole.SysAdmin.ToString() },
                new Role { RoleID = Guid.NewGuid(), RoleName = UserRole.BusAdmin.ToString() },
                new Role { RoleID = Guid.NewGuid(), RoleName = UserRole.Passenger.ToString() }
            };

            _context.Roles.AddRange(roles);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} roles", roles.Count);
        }

        private async Task SeedUsersAsync()
        {
            var sysAdminRole = await _context.Roles.FirstAsync(r => r.RoleName == UserRole.SysAdmin.ToString());
            var busAdminRole = await _context.Roles.FirstAsync(r => r.RoleName == UserRole.BusAdmin.ToString());
            var passengerRole = await _context.Roles.FirstAsync(r => r.RoleName == UserRole.Passenger.ToString());

            var users = new List<User>
            {
                new User
                {
                    UserID = Guid.NewGuid(),
                    Username = "sysadmin",
                    Email = "admin@example.com",
                    PasswordHash = "hashed_password_123", // In production, use proper password hashing
                    PhoneNumber = "0901234567",
                    RoleID = sysAdminRole.RoleID,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserID = Guid.NewGuid(),
                    Username = "busadmin_pt",
                    Email = "admin@phuongtrang.com",
                    PasswordHash = "hashed_password_123",
                    PhoneNumber = "0902345678",
                    RoleID = busAdminRole.RoleID,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserID = Guid.NewGuid(),
                    Username = "busadmin_mt",
                    Email = "admin@maitanh.com",
                    PasswordHash = "hashed_password_123",
                    PhoneNumber = "0903456789",
                    RoleID = busAdminRole.RoleID,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserID = Guid.NewGuid(),
                    Username = "nguyenvana",
                    Email = "nguyenvana@gmail.com",
                    PasswordHash = "hashed_password_123",
                    PhoneNumber = "0904567890",
                    RoleID = passengerRole.RoleID,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserID = Guid.NewGuid(),
                    Username = "tranthib",
                    Email = "tranthib@gmail.com",
                    PasswordHash = "hashed_password_123",
                    PhoneNumber = "0905678901",
                    RoleID = passengerRole.RoleID,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} users", users.Count);
        }

        private async Task SeedBusCompaniesAsync()
        {
            var busAdmin1 = await _context.Users.FirstAsync(u => u.Username == "busadmin_pt");
            var busAdmin2 = await _context.Users.FirstAsync(u => u.Username == "busadmin_mt");

            var companies = new List<BusCompany>
            {
                new BusCompany
                {
                    CompanyID = Guid.NewGuid(),
                    Name = "Phương Trang (FUTA Bus Lines)",
                    LicenseNumber = "VN-PT-001",
                    Hotline = "19006067",
                    IsApproved = true
                },
                new BusCompany
                {
                    CompanyID = Guid.NewGuid(),
                    Name = "Mai Linh Express",
                    LicenseNumber = "VN-ML-002",
                    Hotline = "19006292",
                    IsApproved = true
                },
                new BusCompany
                {
                    CompanyID = Guid.NewGuid(),
                    Name = "Thiện Thành",
                    LicenseNumber = "VN-TT-003",
                    Hotline = "02513828282",
                    IsApproved = true
                }
            };

            _context.BusCompanies.AddRange(companies);
            await _context.SaveChangesAsync();

            // Link admins to companies
            var phuongTrang = await _context.BusCompanies.FirstAsync(c => c.LicenseNumber == "VN-PT-001");
            var maiLinh = await _context.BusCompanies.FirstAsync(c => c.LicenseNumber == "VN-ML-002");

            var admins = new List<BusCompanyAdmin>
            {
                new BusCompanyAdmin
                {
                    UserID = busAdmin1.UserID,
                    CompanyID = phuongTrang.CompanyID
                },
                new BusCompanyAdmin
                {
                    UserID = busAdmin2.UserID,
                    CompanyID = maiLinh.CompanyID
                }
            };

            _context.BusCompanyAdmins.AddRange(admins);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} bus companies", companies.Count);
        }

        private async Task SeedBusTypesAsync()
        {
            var busTypes = new List<BusType>
            {
                new BusType
                {
                    BusTypeID = Guid.NewGuid(),
                    Name = "Giường nằm 29 chỗ",
                    TotalSeats = 29,
                    Description = "Xe giường nằm cao cấp 29 chỗ"
                },
                new BusType
                {
                    BusTypeID = Guid.NewGuid(),
                    Name = "Ghế ngồi 40 chỗ",
                    TotalSeats = 40,
                    Description = "Xe ghế ngồi tiêu chuẩn 40 chỗ"
                },
                new BusType
                {
                    BusTypeID = Guid.NewGuid(),
                    Name = "Ghế ngồi 45 chỗ",
                    TotalSeats = 45,
                    Description = "Xe ghế ngồi 45 chỗ"
                }
            };

            _context.BusTypes.AddRange(busTypes);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} bus types", busTypes.Count);
        }

        private async Task SeedSeatLayoutsAsync()
        {
            var busType29Seat = await _context.BusTypes.FirstAsync(bt => bt.Name == "Giường nằm 29 chỗ");
            var busType40Seat = await _context.BusTypes.FirstAsync(bt => bt.Name == "Ghế ngồi 40 chỗ");
            var busType45Seat = await _context.BusTypes.FirstAsync(bt => bt.Name == "Ghế ngồi 45 chỗ");

            var seatLayouts = new List<SeatLayout>();
            seatLayouts.AddRange(CreateSeatLayouts(busType29Seat.BusTypeID, 29, 3, 1, "A"));
            seatLayouts.AddRange(CreateSeatLayouts(busType40Seat.BusTypeID, 40, 4, 1, "B"));
            seatLayouts.AddRange(CreateSeatLayouts(busType45Seat.BusTypeID, 45, 5, 1, "C"));

            _context.SeatLayouts.AddRange(seatLayouts);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} seat layouts", seatLayouts.Count);
        }

        private async Task SeedBusesAsync()
        {
            var phuongTrang = await _context.BusCompanies.FirstAsync(c => c.LicenseNumber == "VN-PT-001");
            var maiLinh = await _context.BusCompanies.FirstAsync(c => c.LicenseNumber == "VN-ML-002");
            var thienThanh = await _context.BusCompanies.FirstAsync(c => c.LicenseNumber == "VN-TT-003");

            var busType29Seat = await _context.BusTypes.FirstAsync(bt => bt.Name == "Giường nằm 29 chỗ");
            var busType40Seat = await _context.BusTypes.FirstAsync(bt => bt.Name == "Ghế ngồi 40 chỗ");
            var busType45Seat = await _context.BusTypes.FirstAsync(bt => bt.Name == "Ghế ngồi 45 chỗ");

            var buses = new List<Bus>
            {
                new Bus
                {
                    BusID = Guid.NewGuid(),
                    CompanyID = phuongTrang.CompanyID,
                    BusTypeID = busType29Seat.BusTypeID,
                    PlateNumber = "51B-12345",
                    IsActive = true
                },
                new Bus
                {
                    BusID = Guid.NewGuid(),
                    CompanyID = phuongTrang.CompanyID,
                    BusTypeID = busType29Seat.BusTypeID,
                    PlateNumber = "51B-67890",
                    IsActive = true
                },
                new Bus
                {
                    BusID = Guid.NewGuid(),
                    CompanyID = maiLinh.CompanyID,
                    BusTypeID = busType40Seat.BusTypeID,
                    PlateNumber = "79A-11111",
                    IsActive = true
                },
                new Bus
                {
                    BusID = Guid.NewGuid(),
                    CompanyID = maiLinh.CompanyID,
                    BusTypeID = busType45Seat.BusTypeID,
                    PlateNumber = "79A-22222",
                    IsActive = true
                },
                new Bus
                {
                    BusID = Guid.NewGuid(),
                    CompanyID = thienThanh.CompanyID,
                    BusTypeID = busType29Seat.BusTypeID,
                    PlateNumber = "92B-33333",
                    IsActive = true
                }
            };

            _context.Buses.AddRange(buses);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} buses", buses.Count);
        }

        private async Task SeedBusImagesAsync()
        {
            var bus1 = await _context.Buses.FirstAsync(b => b.PlateNumber == "51B-12345");
            var bus2 = await _context.Buses.FirstAsync(b => b.PlateNumber == "51B-67890");
            var bus3 = await _context.Buses.FirstAsync(b => b.PlateNumber == "79A-11111");
            var bus4 = await _context.Buses.FirstAsync(b => b.PlateNumber == "79A-22222");
            var bus5 = await _context.Buses.FirstAsync(b => b.PlateNumber == "92B-33333");

            var busImages = new List<BusImage>
            {
                new BusImage { ImageID = Guid.NewGuid(), BusID = bus1.BusID, ImageURL = "https://images.example.com/buses/51B-12345.jpg" },
                new BusImage { ImageID = Guid.NewGuid(), BusID = bus2.BusID, ImageURL = "https://images.example.com/buses/51B-67890.jpg" },
                new BusImage { ImageID = Guid.NewGuid(), BusID = bus3.BusID, ImageURL = "https://images.example.com/buses/79A-11111.jpg" },
                new BusImage { ImageID = Guid.NewGuid(), BusID = bus4.BusID, ImageURL = "https://images.example.com/buses/79A-22222.jpg" },
                new BusImage { ImageID = Guid.NewGuid(), BusID = bus5.BusID, ImageURL = "https://images.example.com/buses/92B-33333.jpg" }
            };

            _context.BusImages.AddRange(busImages);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} bus images", busImages.Count);
        }

        private async Task SeedStationsAsync()
        {
            var stations = new List<Station>
            {
                new Station
                {
                    StationID = Guid.NewGuid(),
                    Name = "Bến xe Miền Đông",
                    AddressDetail = "292 Đinh Bộ Lĩnh, Phường 26, Bình Thạnh",
                    Province = "Hồ Chí Minh",
                    Type = StationType.BusStation,
                    Latitude = 10.8142,
                    Longitude = 106.7100
                },
                new Station
                {
                    StationID = Guid.NewGuid(),
                    Name = "Bến xe Miền Tây",
                    AddressDetail = "395 Kinh Dương Vương, Phường An Lạc, Bình Tân",
                    Province = "Hồ Chí Minh",
                    Type = StationType.BusStation,
                    Latitude = 10.7387,
                    Longitude = 106.6102
                },
                new Station
                {
                    StationID = Guid.NewGuid(),
                    Name = "Bến xe Mỹ Đình",
                    AddressDetail = "Đường Phạm Hùng, Nam Từ Liêm",
                    Province = "Hà Nội",
                    Type = StationType.BusStation,
                    Latitude = 21.0278,
                    Longitude = 105.7802
                },
                new Station
                {
                    StationID = Guid.NewGuid(),
                    Name = "Bến xe Trung tâm Đà Nẵng",
                    AddressDetail = "201 Tôn Đức Thắng, Hòa Minh, Liên Chiểu",
                    Province = "Đà Nẵng",
                    Type = StationType.BusStation,
                    Latitude = 16.0544,
                    Longitude = 108.1851
                },
                new Station
                {
                    StationID = Guid.NewGuid(),
                    Name = "Bến xe Phương Trang Đà Lạt",
                    AddressDetail = "1 Tô Hiến Thành, Phường 3",
                    Province = "Lâm Đồng",
                    Type = StationType.BusStation,
                    Latitude = 11.9404,
                    Longitude = 108.4583
                },
                new Station
                {
                    StationID = Guid.NewGuid(),
                    Name = "Bến xe Vũng Tàu",
                    AddressDetail = "192 Nam Kỳ Khởi Nghĩa, Phường 7",
                    Province = "Bà Rịa - Vũng Tàu",
                    Type = StationType.BusStation,
                    Latitude = 10.3459,
                    Longitude = 107.0843
                },
                new Station
                {
                    StationID = Guid.NewGuid(),
                    Name = "Bến xe Cần Thơ",
                    AddressDetail = "Đường Nguyễn Văn Linh, Hưng Lợi, Ninh Kiều",
                    Province = "Cần Thơ",
                    Type = StationType.BusStation,
                    Latitude = 10.0452,
                    Longitude = 105.7469
                },
                new Station
                {
                    StationID = Guid.NewGuid(),
                    Name = "Bến xe Nha Trang",
                    AddressDetail = "23 Tháng 10, Phước Long, Nha Trang",
                    Province = "Khánh Hòa",
                    Type = StationType.BusStation,
                    Latitude = 12.2388,
                    Longitude = 109.1967
                }
            };

            _context.Stations.AddRange(stations);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} stations", stations.Count);
        }

        private async Task SeedRoutesAsync()
        {
            var phuongTrang = await _context.BusCompanies.FirstAsync(c => c.LicenseNumber == "VN-PT-001");
            var maiLinh = await _context.BusCompanies.FirstAsync(c => c.LicenseNumber == "VN-ML-002");
            var thienThanh = await _context.BusCompanies.FirstAsync(c => c.LicenseNumber == "VN-TT-003");

            var routes = new List<BusRoute>
            {
                new BusRoute
                {
                    RouteID = Guid.NewGuid(),
                    CompanyID = phuongTrang.CompanyID,
                    RouteName = "TP.HCM - Đà Lạt",
                    DistanceEstimate = 308,
                    DurationEstimate = 8,
                    IsActive = true
                },
                new BusRoute
                {
                    RouteID = Guid.NewGuid(),
                    CompanyID = phuongTrang.CompanyID,
                    RouteName = "TP.HCM - Vũng Tàu",
                    DistanceEstimate = 125,
                    DurationEstimate = 2.5m,
                    IsActive = true
                },
                new BusRoute
                {
                    RouteID = Guid.NewGuid(),
                    CompanyID = maiLinh.CompanyID,
                    RouteName = "TP.HCM - Cần Thơ",
                    DistanceEstimate = 169,
                    DurationEstimate = 4,
                    IsActive = true
                },
                new BusRoute
                {
                    RouteID = Guid.NewGuid(),
                    CompanyID = thienThanh.CompanyID,
                    RouteName = "TP.HCM - Nha Trang",
                    DistanceEstimate = 450,
                    DurationEstimate = 10,
                    IsActive = true
                },
                new BusRoute
                {
                    RouteID = Guid.NewGuid(),
                    CompanyID = phuongTrang.CompanyID,
                    RouteName = "TP.HCM - Đà Nẵng",
                    DistanceEstimate = 964,
                    DurationEstimate = 18,
                    IsActive = true
                }
            };

            _context.BusRoutes.AddRange(routes);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} routes", routes.Count);
        }

        private async Task SeedRouteStopsAsync()
        {
            var routeHCMDaLat = await _context.BusRoutes.FirstAsync(r => r.RouteName == "TP.HCM - Đà Lạt");
            var routeHCMVungTau = await _context.BusRoutes.FirstAsync(r => r.RouteName == "TP.HCM - Vũng Tàu");
            var routeHCMCanTho = await _context.BusRoutes.FirstAsync(r => r.RouteName == "TP.HCM - Cần Thơ");
            var routeHCMNhaTrang = await _context.BusRoutes.FirstAsync(r => r.RouteName == "TP.HCM - Nha Trang");
            var routeHCMDaNang = await _context.BusRoutes.FirstAsync(r => r.RouteName == "TP.HCM - Đà Nẵng");

            var stationBenTreHCM = await _context.Stations.FirstAsync(s => s.Name == "Bến xe Miền Đông");
            var stationBenThanhHCM = await _context.Stations.FirstAsync(s => s.Name == "Bến xe Miền Tây");
            var stationDaLat = await _context.Stations.FirstAsync(s => s.Name == "Bến xe Phương Trang Đà Lạt");
            var stationVungTau = await _context.Stations.FirstAsync(s => s.Name == "Bến xe Vũng Tàu");
            var stationCanTho = await _context.Stations.FirstAsync(s => s.Name == "Bến xe Cần Thơ");
            var stationNhaTrang = await _context.Stations.FirstAsync(s => s.Name == "Bến xe Nha Trang");
            var stationDaNang = await _context.Stations.FirstAsync(s => s.Name == "Bến xe Trung tâm Đà Nẵng");

            var routeStops = new List<BusRouteStop>
            {
                // HCM - Da Lat
                new BusRouteStop
                {
                    BusRouteStopID = Guid.NewGuid(),
                    RouteID = routeHCMDaLat.RouteID,
                    StationID = stationBenTreHCM.StationID,
                    StopOrder = 1,
                    IsPickUp = true,
                    IsDropOff = false,
                    DurationFromStart = 0
                },
                new BusRouteStop
                {
                    BusRouteStopID = Guid.NewGuid(),
                    RouteID = routeHCMDaLat.RouteID,
                    StationID = stationDaLat.StationID,
                    StopOrder = 2,
                    IsPickUp = false,
                    IsDropOff = true,
                    DurationFromStart = 480
                },
                // HCM - Vung Tau
                new BusRouteStop
                {
                    BusRouteStopID = Guid.NewGuid(),
                    RouteID = routeHCMVungTau.RouteID,
                    StationID = stationBenTreHCM.StationID,
                    StopOrder = 1,
                    IsPickUp = true,
                    IsDropOff = false,
                    DurationFromStart = 0
                },
                new BusRouteStop
                {
                    BusRouteStopID = Guid.NewGuid(),
                    RouteID = routeHCMVungTau.RouteID,
                    StationID = stationVungTau.StationID,
                    StopOrder = 2,
                    IsPickUp = false,
                    IsDropOff = true,
                    DurationFromStart = 150
                },
                // HCM - Can Tho
                new BusRouteStop
                {
                    BusRouteStopID = Guid.NewGuid(),
                    RouteID = routeHCMCanTho.RouteID,
                    StationID = stationBenThanhHCM.StationID,
                    StopOrder = 1,
                    IsPickUp = true,
                    IsDropOff = false,
                    DurationFromStart = 0
                },
                new BusRouteStop
                {
                    BusRouteStopID = Guid.NewGuid(),
                    RouteID = routeHCMCanTho.RouteID,
                    StationID = stationCanTho.StationID,
                    StopOrder = 2,
                    IsPickUp = false,
                    IsDropOff = true,
                    DurationFromStart = 240
                },
                // HCM - Nha Trang
                new BusRouteStop
                {
                    BusRouteStopID = Guid.NewGuid(),
                    RouteID = routeHCMNhaTrang.RouteID,
                    StationID = stationBenTreHCM.StationID,
                    StopOrder = 1,
                    IsPickUp = true,
                    IsDropOff = false,
                    DurationFromStart = 0
                },
                new BusRouteStop
                {
                    BusRouteStopID = Guid.NewGuid(),
                    RouteID = routeHCMNhaTrang.RouteID,
                    StationID = stationNhaTrang.StationID,
                    StopOrder = 2,
                    IsPickUp = false,
                    IsDropOff = true,
                    DurationFromStart = 600
                },
                // HCM - Da Nang
                new BusRouteStop
                {
                    BusRouteStopID = Guid.NewGuid(),
                    RouteID = routeHCMDaNang.RouteID,
                    StationID = stationBenTreHCM.StationID,
                    StopOrder = 1,
                    IsPickUp = true,
                    IsDropOff = false,
                    DurationFromStart = 0
                },
                new BusRouteStop
                {
                    BusRouteStopID = Guid.NewGuid(),
                    RouteID = routeHCMDaNang.RouteID,
                    StationID = stationDaNang.StationID,
                    StopOrder = 2,
                    IsPickUp = false,
                    IsDropOff = true,
                    DurationFromStart = 1080
                }
            };

            _context.RouteStops.AddRange(routeStops);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} route stops", routeStops.Count);
        }

        private async Task SeedTripsAsync()
        {
            var routeHCMDaLat = await _context.BusRoutes.FirstAsync(r => r.RouteName == "TP.HCM - Đà Lạt");
            var routeHCMVungTau = await _context.BusRoutes.FirstAsync(r => r.RouteName == "TP.HCM - Vũng Tàu");
            var routeHCMCanTho = await _context.BusRoutes.FirstAsync(r => r.RouteName == "TP.HCM - Cần Thơ");
            var routeHCMNhaTrang = await _context.BusRoutes.FirstAsync(r => r.RouteName == "TP.HCM - Nha Trang");
            var routeHCMDaNang = await _context.BusRoutes.FirstAsync(r => r.RouteName == "TP.HCM - Đà Nẵng");

            var bus1 = await _context.Buses.FirstAsync(b => b.PlateNumber == "51B-12345");
            var bus2 = await _context.Buses.FirstAsync(b => b.PlateNumber == "51B-67890");
            var bus3 = await _context.Buses.FirstAsync(b => b.PlateNumber == "79A-11111");
            var bus4 = await _context.Buses.FirstAsync(b => b.PlateNumber == "79A-22222");
            var bus5 = await _context.Buses.FirstAsync(b => b.PlateNumber == "92B-33333");

            var busType29Seat = await _context.BusTypes.FirstAsync(bt => bt.Name == "Giường nằm 29 chỗ");
            var busType40Seat = await _context.BusTypes.FirstAsync(bt => bt.Name == "Ghế ngồi 40 chỗ");
            var busType45Seat = await _context.BusTypes.FirstAsync(bt => bt.Name == "Ghế ngồi 45 chỗ");

            var today = DateTime.UtcNow;
            var trips = new List<Trip>
            {
                // HCM - Da Lat trips
                new Trip
                {
                    TripID = Guid.NewGuid(),
                    RouteID = routeHCMDaLat.RouteID,
                    BusID = bus1.BusID,
                    BusTypeID = busType29Seat.BusTypeID,
                    DepartureDate = DateOnly.FromDateTime(today.AddDays(1)),
                    DepartureTime = today.AddDays(1).Date.AddHours(7),
                    ArrivalTime = today.AddDays(1).Date.AddHours(15),
                    Status = TripStatus.Scheduled
                },
                new Trip
                {
                    TripID = Guid.NewGuid(),
                    RouteID = routeHCMDaLat.RouteID,
                    BusID = bus2.BusID,
                    BusTypeID = busType29Seat.BusTypeID,
                    DepartureDate = DateOnly.FromDateTime(today.AddDays(1)),
                    DepartureTime = today.AddDays(1).Date.AddHours(22),
                    ArrivalTime = today.AddDays(2).Date.AddHours(6),
                    Status = TripStatus.Scheduled
                },
                new Trip
                {
                    TripID = Guid.NewGuid(),
                    RouteID = routeHCMDaLat.RouteID,
                    BusID = bus1.BusID,
                    BusTypeID = busType29Seat.BusTypeID,
                    DepartureDate = DateOnly.FromDateTime(today.AddDays(2)),
                    DepartureTime = today.AddDays(2).Date.AddHours(7),
                    ArrivalTime = today.AddDays(2).Date.AddHours(15),
                    Status = TripStatus.Scheduled
                },
                // HCM - Vung Tau trips
                new Trip
                {
                    TripID = Guid.NewGuid(),
                    RouteID = routeHCMVungTau.RouteID,
                    BusID = bus2.BusID,
                    BusTypeID = busType29Seat.BusTypeID,
                    DepartureDate = DateOnly.FromDateTime(today.AddDays(1)),
                    DepartureTime = today.AddDays(1).Date.AddHours(8),
                    ArrivalTime = today.AddDays(1).Date.AddHours(10).AddMinutes(30),
                    Status = TripStatus.Scheduled
                },
                new Trip
                {
                    TripID = Guid.NewGuid(),
                    RouteID = routeHCMVungTau.RouteID,
                    BusID = bus1.BusID,
                    BusTypeID = busType29Seat.BusTypeID,
                    DepartureDate = DateOnly.FromDateTime(today.AddDays(1)),
                    DepartureTime = today.AddDays(1).Date.AddHours(14),
                    ArrivalTime = today.AddDays(1).Date.AddHours(16).AddMinutes(30),
                    Status = TripStatus.Scheduled
                },
                // HCM - Can Tho trips
                new Trip
                {
                    TripID = Guid.NewGuid(),
                    RouteID = routeHCMCanTho.RouteID,
                    BusID = bus3.BusID,
                    BusTypeID = busType40Seat.BusTypeID,
                    DepartureDate = DateOnly.FromDateTime(today.AddDays(1)),
                    DepartureTime = today.AddDays(1).Date.AddHours(6),
                    ArrivalTime = today.AddDays(1).Date.AddHours(10),
                    Status = TripStatus.Scheduled
                },
                new Trip
                {
                    TripID = Guid.NewGuid(),
                    RouteID = routeHCMCanTho.RouteID,
                    BusID = bus4.BusID,
                    BusTypeID = busType45Seat.BusTypeID,
                    DepartureDate = DateOnly.FromDateTime(today.AddDays(2)),
                    DepartureTime = today.AddDays(2).Date.AddHours(12),
                    ArrivalTime = today.AddDays(2).Date.AddHours(16),
                    Status = TripStatus.Scheduled
                },
                // HCM - Nha Trang trips
                new Trip
                {
                    TripID = Guid.NewGuid(),
                    RouteID = routeHCMNhaTrang.RouteID,
                    BusID = bus5.BusID,
                    BusTypeID = busType29Seat.BusTypeID,
                    DepartureDate = DateOnly.FromDateTime(today.AddDays(1)),
                    DepartureTime = today.AddDays(1).Date.AddHours(20),
                    ArrivalTime = today.AddDays(2).Date.AddHours(6),
                    Status = TripStatus.Scheduled
                },
                // HCM - Da Nang trips
                new Trip
                {
                    TripID = Guid.NewGuid(),
                    RouteID = routeHCMDaNang.RouteID,
                    BusID = bus1.BusID,
                    BusTypeID = busType29Seat.BusTypeID,
                    DepartureDate = DateOnly.FromDateTime(today.AddDays(3)),
                    DepartureTime = today.AddDays(3).Date.AddHours(18),
                    ArrivalTime = today.AddDays(4).Date.AddHours(12),
                    Status = TripStatus.Scheduled
                }
            };

            _context.Trips.AddRange(trips);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} trips", trips.Count);
        }

        private async Task SeedBookingsAsync()
        {
            var passenger1 = await _context.Users.FirstAsync(u => u.Username == "nguyenvana");
            var passenger2 = await _context.Users.FirstAsync(u => u.Username == "tranthib");

            var bookings = new List<Booking>
            {
                new Booking
                {
                    BookingID = Guid.NewGuid(),
                    UserID = passenger1.UserID,
                    ContactName = "Nguyễn Văn A",
                    ContactPhone = "0904567890",
                    ContactEmail = "nguyenvana@gmail.com",
                    TotalAmount = 250000,
                    Status = BookingStatus.Paid,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    ExpiresAt = null
                },
                new Booking
                {
                    BookingID = Guid.NewGuid(),
                    UserID = passenger2.UserID,
                    ContactName = "Trần Thị B",
                    ContactPhone = "0905678901",
                    ContactEmail = "tranthib@gmail.com",
                    TotalAmount = 180000,
                    Status = BookingStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15)
                }
            };

            _context.Bookings.AddRange(bookings);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} bookings", bookings.Count);
        }

        private async Task SeedPassengersAsync()
        {
            var user1 = await _context.Users.FirstAsync(u => u.Username == "nguyenvana");
            var user2 = await _context.Users.FirstAsync(u => u.Username == "tranthib");

            var passengers = new List<Passenger>
            {
                new Passenger
                {
                    PassengerID = Guid.NewGuid(),
                    UserID = user1.UserID,
                    FullName = "Nguyễn Văn A",
                    PhoneNumber = "0904567890",
                    IdentityCard = "079203001234",
                    Email = "nguyenvana@gmail.com"
                },
                new Passenger
                {
                    PassengerID = Guid.NewGuid(),
                    UserID = user2.UserID,
                    FullName = "Trần Thị B",
                    PhoneNumber = "0905678901",
                    IdentityCard = "079203005678",
                    Email = "tranthib@gmail.com"
                },
                new Passenger
                {
                    PassengerID = Guid.NewGuid(),
                    UserID = null,
                    FullName = "Lê Văn C",
                    PhoneNumber = "0911112233",
                    IdentityCard = "079203009999",
                    Email = "levanc@example.com"
                }
            };

            _context.Passengers.AddRange(passengers);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} passengers", passengers.Count);
        }

        private async Task SeedTicketsAsync()
        {
            var paidBooking = await _context.Bookings.FirstAsync(b => b.ContactEmail == "nguyenvana@gmail.com" && b.Status == BookingStatus.Paid);

            var passenger1 = await _context.Passengers.FirstAsync(p => p.Email == "nguyenvana@gmail.com");

            var tripDaLatMorning = await _context.Trips.FirstAsync(t => t.Route!.RouteName == "TP.HCM - Đà Lạt" && t.DepartureTime.Hour == 7);

            var seatA1 = await _context.SeatLayouts.FirstAsync(s => s.BusType!.Name == "Giường nằm 29 chỗ" && s.SeatLabel == "A1");

            var tickets = new List<Ticket>
            {
                new Ticket
                {
                    TicketID = Guid.NewGuid(),
                    BookingID = paidBooking.BookingID,
                    TripID = tripDaLatMorning.TripID,
                    PassengerID = passenger1.PassengerID,
                    SeatLayoutID = seatA1.LayoutID,
                    FinalPrice = 250000,
                    Status = TicketStatus.Issued,
                    TicketCode = "TKT-DALAT-0001",
                    QrCode = "QR-TKT-DALAT-0001"
                }
            };

            _context.Tickets.AddRange(tickets);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} tickets", tickets.Count);
        }

        private async Task SeedPaymentIntentsAsync()
        {
            var paidBooking = await _context.Bookings.FirstAsync(b => b.ContactEmail == "nguyenvana@gmail.com" && b.Status == BookingStatus.Paid);
            var pendingBooking = await _context.Bookings.FirstAsync(b => b.ContactEmail == "tranthib@gmail.com" && b.Status == BookingStatus.Pending);

            var paymentIntents = new List<PaymentIntent>
            {
                new PaymentIntent
                {
                    IntentID = Guid.NewGuid(),
                    BookingID = paidBooking.BookingID,
                    Provider = PaymentProvider.Momo,
                    Amount = 250000,
                    Currency = "VND",
                    Status = PaymentIntentStatus.Succeeded,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new PaymentIntent
                {
                    IntentID = Guid.NewGuid(),
                    BookingID = pendingBooking.BookingID,
                    Provider = PaymentProvider.Cash,
                    Amount = 180000,
                    Currency = "VND",
                    Status = PaymentIntentStatus.Created,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.PaymentIntents.AddRange(paymentIntents);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} payment intents", paymentIntents.Count);
        }

        private async Task SeedRefundsAsync()
        {
            var succeededIntent = await _context.PaymentIntents.FirstAsync(pi => pi.Status == PaymentIntentStatus.Succeeded);

            var refunds = new List<Refund>
            {
                new Refund
                {
                    RefundID = Guid.NewGuid(),
                    IntentID = succeededIntent.IntentID,
                    Amount = 50000,
                    Reason = "Hoàn phí khuyến mại do đổi lịch",
                    Status = RefundStatus.Processed,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            _context.Refunds.AddRange(refunds);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} refunds", refunds.Count);
        }

        private async Task SeedReviewsAsync()
        {
            var paidBooking = await _context.Bookings.FirstAsync(b => b.ContactEmail == "nguyenvana@gmail.com" && b.Status == BookingStatus.Paid);
            var tripDaLatMorning = await _context.Trips.FirstAsync(t => t.Route!.RouteName == "TP.HCM - Đà Lạt" && t.DepartureTime.Hour == 7);

            var reviews = new List<Review>
            {
                new Review
                {
                    ReviewID = Guid.NewGuid(),
                    BookingID = paidBooking.BookingID,
                    TripID = tripDaLatMorning.TripID,
                    RatingScore = 5,
                    Comment = "Xe sạch sẽ, tài xế chạy đúng giờ và nhân viên hỗ trợ tốt."
                }
            };

            _context.Reviews.AddRange(reviews);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} reviews", reviews.Count);
        }

        private async Task SeedNotificationsAsync()
        {
            var user1 = await _context.Users.FirstAsync(u => u.Username == "nguyenvana");
            var user2 = await _context.Users.FirstAsync(u => u.Username == "tranthib");
            var paidBooking = await _context.Bookings.FirstAsync(b => b.ContactEmail == "nguyenvana@gmail.com" && b.Status == BookingStatus.Paid);
            var pendingBooking = await _context.Bookings.FirstAsync(b => b.ContactEmail == "tranthib@gmail.com" && b.Status == BookingStatus.Pending);

            var notifications = new List<Notification>
            {
                new Notification
                {
                    NotifID = Guid.NewGuid(),
                    UserID = user1.UserID,
                    BookingID = paidBooking.BookingID,
                    Type = NotificationType.Email,
                    Content = "Đặt vé TP.HCM - Đà Lạt đã được thanh toán thành công.",
                    Status = NotificationStatus.Sent
                },
                new Notification
                {
                    NotifID = Guid.NewGuid(),
                    UserID = user2.UserID,
                    BookingID = pendingBooking.BookingID,
                    Type = NotificationType.SMS,
                    Content = "Đơn đặt vé TP.HCM - Vũng Tàu đang chờ thanh toán trước khi giữ ghế hết hạn.",
                    Status = NotificationStatus.Sent
                }
            };

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} notifications", notifications.Count);
        }

        private async Task SeedSeatHoldsAsync()
        {
            var user2 = await _context.Users.FirstAsync(u => u.Username == "tranthib");
            var tripVungTauMorning = await _context.Trips.FirstAsync(t => t.Route!.RouteName == "TP.HCM - Vũng Tàu" && t.DepartureTime.Hour == 8);
            var seatA2 = await _context.SeatLayouts.FirstAsync(s => s.BusType!.Name == "Giường nằm 29 chỗ" && s.SeatLabel == "A2");
            var seatA3 = await _context.SeatLayouts.FirstAsync(s => s.BusType!.Name == "Giường nằm 29 chỗ" && s.SeatLabel == "A3");

            var seatHolds = new List<SeatHold>
            {
                new SeatHold
                {
                    HoldID = Guid.NewGuid(),
                    TripID = tripVungTauMorning.TripID,
                    SeatLayoutID = seatA2.LayoutID,
                    UserID = user2.UserID,
                    SessionID = "session-tranthib-001",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    Status = SeatHoldStatus.Confirmed
                },
                new SeatHold
                {
                    HoldID = Guid.NewGuid(),
                    TripID = tripVungTauMorning.TripID,
                    SeatLayoutID = seatA3.LayoutID,
                    UserID = null,
                    SessionID = "guest-session-001",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    Status = SeatHoldStatus.Held
                }
            };

            _context.SeatHolds.AddRange(seatHolds);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} seat holds", seatHolds.Count);
        }

        private static IEnumerable<SeatLayout> CreateSeatLayouts(Guid busTypeId, int totalSeats, int seatsPerRow, int floor, string seatPrefix)
        {
            var seatLayouts = new List<SeatLayout>();

            for (int i = 1; i <= totalSeats; i++)
            {
                var positionX = (i - 1) % seatsPerRow + 1;
                seatLayouts.Add(new SeatLayout
                {
                    LayoutID = Guid.NewGuid(),
                    BusTypeID = busTypeId,
                    SeatLabel = $"{seatPrefix}{i}",
                    Floor = floor,
                    SeatType = ResolveSeatType(positionX, seatsPerRow),
                    PositionX = positionX,
                    PositionY = (i - 1) / seatsPerRow + 1
                });
            }

            return seatLayouts;
        }

        private static SeatType ResolveSeatType(int positionX, int seatsPerRow)
        {
            if (positionX == 1 || positionX == seatsPerRow)
            {
                return SeatType.Window;
            }

            if (seatsPerRow % 2 == 1 && positionX == (seatsPerRow / 2) + 1)
            {
                return SeatType.Middle;
            }

            return SeatType.Aisle;
        }
    }
}
