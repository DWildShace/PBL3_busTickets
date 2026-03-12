using Microsoft.EntityFrameworkCore;
using Pbl3.Models;
using Pbl3.Enums;

namespace Pbl3.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<BusCompany> BusCompanies { get; set; }
        public DbSet<BusCompanyAdmin> BusCompanyAdmins { get; set; }
        public DbSet<BusType> BusTypes { get; set; }
        public DbSet<SeatLayout> SeatLayouts { get; set; }
        public DbSet<Bus> Buses { get; set; }
        public DbSet<BusImage> BusImages { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<BusRoute> BusRoutes { get; set; }
        public DbSet<BusRouteStop> RouteStops { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<SeatHold> SeatHolds { get; set; }
        public DbSet<PaymentIntent> PaymentIntents { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Register Enums
            modelBuilder.HasPostgresEnum<UserRole>();
            modelBuilder.HasPostgresEnum<TripStatus>();
            modelBuilder.HasPostgresEnum<SeatType>();
            modelBuilder.HasPostgresEnum<StationType>();
            modelBuilder.HasPostgresEnum<BookingStatus>();
            modelBuilder.HasPostgresEnum<TicketStatus>();
            modelBuilder.HasPostgresEnum<SeatHoldStatus>();
            modelBuilder.HasPostgresEnum<PaymentProvider>();
            modelBuilder.HasPostgresEnum<PaymentIntentStatus>();
            modelBuilder.HasPostgresEnum<RefundStatus>();
            modelBuilder.HasPostgresEnum<NotificationType>();
            modelBuilder.HasPostgresEnum<NotificationStatus>();


            // BusCompanyAdmin Key
            modelBuilder.Entity<BusCompanyAdmin>()
                .HasKey(bca => bca.UserID);

            modelBuilder.Entity<BusCompanyAdmin>()
                .HasOne(bca => bca.User)
                .WithMany(u => u.BusCompanyAdmins)
                .HasForeignKey(bca => bca.UserID);

            // RouteStop Relations
            modelBuilder.Entity<BusRouteStop>()
                .HasOne(rs => rs.Route)
                .WithMany(r => r.BusRouteStops)
                .HasForeignKey(rs => rs.RouteID);

            modelBuilder.Entity<BusRouteStop>()
                .HasOne(rs => rs.Station)
                .WithMany(s => s.BusRouteStops)
                .HasForeignKey(rs => rs.StationID);

            // Ticket Relations
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Booking)
                .WithMany(b => b.Tickets)
                .HasForeignKey(t => t.BookingID)
                .OnDelete(DeleteBehavior.Cascade); // Deleting booking deletes tickets

            modelBuilder.Entity<Ticket>()
               .HasOne(t => t.Trip)
               .WithMany(tr => tr.Tickets)
               .HasForeignKey(t => t.TripID)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Passenger)
                .WithMany(p => p.Tickets)
                .HasForeignKey(t => t.PassengerID)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking Relations
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserID)
                .OnDelete(DeleteBehavior.SetNull);

            // SeatHold
            modelBuilder.Entity<SeatHold>()
                .HasOne(sh => sh.Trip)
                .WithMany(t => t.SeatHolds)
                .HasForeignKey(sh => sh.TripID);

            modelBuilder.Entity<SeatHold>()
                .HasOne(sh => sh.SeatLayout)
                .WithMany()
                .HasForeignKey(sh => sh.SeatLayoutID);

            // Review
            modelBuilder.Entity<Review>()
               .HasOne(r => r.Booking)
               .WithMany(b => b.Reviews)
               .HasForeignKey(r => r.BookingID)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Trip)
                .WithMany(t => t.Reviews)
                .HasForeignKey(r => r.TripID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<BusRoute>()
                .HasOne(r => r.BusCompany)
                .WithMany(c => c.Routes)
                .HasForeignKey(r => r.CompanyID);

            // Refund Relations
            modelBuilder.Entity<Refund>()
                .HasOne(r => r.PaymentIntent)
                .WithMany(pi => pi.Refunds)
                .HasForeignKey(r => r.IntentID);
        }
    }
}
