using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pbl3.Data;
using Pbl3.Dtos;
using Pbl3.Enums;
using Pbl3.Models;

namespace Pbl3.Services.Admin
{
    public class RevenueAnalyticsService(ApplicationDbContext context) : IRevenueAnalyticsService
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync(
            DateTime? startDate,
            DateTime? endDate,
            int topRoutesLimit = 10,
            int topCompaniesLimit = 10
        )
        {
            // Default date range: Last 30 days
            var end = endDate?.Date ?? DateTime.UtcNow.Date;
            var start = startDate?.Date ?? end.AddDays(-30);

            // === TICKET-CENTRIC APPROACH ===
            // Load all Issued/CheckedIn tickets in the date range directly.
            // Revenue is computed from Ticket.FinalPrice — no dependency on PaymentIntent.Status.
            var tickets = await _context.Tickets
                .AsNoTracking()
                .Include(t => t.Booking)
                    .ThenInclude(b => b!.PaymentIntents)
                        .ThenInclude(pi => pi.Refunds)
                .Include(t => t.Trip)
                    .ThenInclude(tr => tr!.Route)
                        .ThenInclude(r => r!.BusCompany)
                .Where(t =>
                    (t.Status == TicketStatus.Issued || t.Status == TicketStatus.CheckedIn)
                    && t.Booking != null
                    && t.Booking.CreatedAt >= start
                    && t.Booking.CreatedAt < end.AddDays(1)
                )
                .ToListAsync();

            // ─── Summary ─────────────────────────────────────────────────────────
            var totalRevenue = tickets.Sum(t => t.FinalPrice);
            var ticketsSold = tickets.Count;

            // Collect unique bookings to avoid double-counting refunds
            var bookingMap = tickets
                .Where(t => t.Booking != null)
                .GroupBy(t => t.BookingID)
                .ToDictionary(g => g.Key, g => g.First().Booking!);

            var totalRefunded = bookingMap.Values
                .SelectMany(b => b.PaymentIntents)
                .SelectMany(pi => pi.Refunds)
                .Where(r => r.Status == RefundStatus.Completed)
                .Sum(r => r.Amount);

            var netRevenue = totalRevenue - totalRefunded;
            var totalTransactions = bookingMap.Count; // unique bookings = transactions
            var avgTransactionValue = totalTransactions > 0
                ? totalRevenue / totalTransactions
                : 0;

            // ─── Growth (previous period) ─────────────────────────────────────────
            var periodDays = (end - start).Days + 1;
            var previousStart = start.AddDays(-periodDays);
            var previousEnd = start.AddDays(-1);

            var previousRevenue = await _context.Tickets
                .AsNoTracking()
                .Where(t =>
                    (t.Status == TicketStatus.Issued || t.Status == TicketStatus.CheckedIn)
                    && t.Booking != null
                    && t.Booking.CreatedAt >= previousStart
                    && t.Booking.CreatedAt < previousEnd.AddDays(1)
                )
                .SumAsync(t => (decimal?)t.FinalPrice) ?? 0m;

            var previousTransactions = await _context.Bookings
                .AsNoTracking()
                .CountAsync(b =>
                    b.CreatedAt >= previousStart
                    && b.CreatedAt < previousEnd.AddDays(1)
                    && b.Tickets.Any(t =>
                        t.Status == TicketStatus.Issued || t.Status == TicketStatus.CheckedIn
                    )
                );

            var revenueGrowth = previousRevenue > 0
                ? ((totalRevenue - previousRevenue) / previousRevenue) * 100
                : 0;
            var transactionGrowth = previousTransactions > 0
                ? ((totalTransactions - previousTransactions) / (decimal)previousTransactions) * 100
                : 0;

            var summary = new RevenueSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalTransactions = totalTransactions,
                AverageTransactionValue = avgTransactionValue,
                TicketsSold = ticketsSold,
                TotalRefunded = totalRefunded,
                NetRevenue = netRevenue,
                RevenueGrowthPercent = revenueGrowth,
                TransactionGrowthPercent = transactionGrowth,
            };

            // ─── Pre-compute refund per booking for daily trends ──────────────────
            var refundByBooking = bookingMap.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.PaymentIntents
                    .SelectMany(pi => pi.Refunds)
                    .Where(r => r.Status == RefundStatus.Completed)
                    .Sum(r => r.Amount)
            );

            // ─── Daily Trends (grouped by Booking.CreatedAt date) ────────────────
            var dailyTrends = tickets
                .GroupBy(t => t.Booking!.CreatedAt.Date)
                .Select(g =>
                {
                    var distinctBookingIds = g.Select(t => t.BookingID).Distinct().ToList();
                    var dayRefund = distinctBookingIds.Sum(bid =>
                        refundByBooking.TryGetValue(bid, out var amt) ? amt : 0
                    );
                    return new RevenueTrendDto
                    {
                        Date = g.Key,
                        Revenue = g.Sum(t => t.FinalPrice),
                        TransactionCount = distinctBookingIds.Count,
                        TicketCount = g.Count(),
                        RefundAmount = dayRefund,
                    };
                })
                .OrderBy(t => t.Date)
                .ToList();

            // ─── Revenue by Payment Provider ──────────────────────────────────────
            // Pull from Succeeded PaymentIntents linked to these bookings.
            // "Không xác định": tickets that have no Succeeded PaymentIntent (test/manual data).
            var succeededIntents = bookingMap.Values
                .SelectMany(b => b.PaymentIntents)
                .Where(pi => pi.Status == PaymentIntentStatus.Succeeded)
                .ToList();

            // Map provider enum to Vietnamese display name
            static string ProviderDisplayName(PaymentProvider p) => p switch
            {
                PaymentProvider.Momo  => "Momo",
                PaymentProvider.Cash  => "Thanh toán trực tiếp",
                _                     => "Không xác định",
            };

            List<RevenueByProviderDto> byProvider;
            if (succeededIntents.Any())
            {
                byProvider = succeededIntents
                    .GroupBy(pi => pi.Provider)
                    .Select(g => new RevenueByProviderDto
                    {
                        Provider    = g.Key,
                        ProviderName = ProviderDisplayName(g.Key),
                        Revenue      = g.Sum(pi => pi.Amount),
                        TransactionCount = g.Count(),
                        Percentage   = totalRevenue > 0
                            ? (g.Sum(pi => pi.Amount) / totalRevenue) * 100
                            : 0,
                    })
                    .OrderByDescending(p => p.Revenue)
                    .ToList();

                // If some tickets have no Succeeded PaymentIntent, show the unattributed gap
                var knownRevenue   = byProvider.Sum(p => p.Revenue);
                var unknownRevenue = totalRevenue - knownRevenue;
                if (unknownRevenue > 0)
                {
                    byProvider.Add(new RevenueByProviderDto
                    {
                        Provider         = (PaymentProvider)(-1), // sentinel: not a real provider
                        ProviderName     = "Không xác định",
                        Revenue          = unknownRevenue,
                        TransactionCount = 0,
                        Percentage       = totalRevenue > 0 ? (unknownRevenue / totalRevenue) * 100 : 0,
                    });
                }
            }
            else
            {
                // No PaymentIntent data at all — all revenue is unattributed
                byProvider =
                [
                    new RevenueByProviderDto
                    {
                        Provider         = (PaymentProvider)(-1),
                        ProviderName     = "Không xác định",
                        Revenue          = totalRevenue,
                        TransactionCount = totalTransactions,
                        Percentage       = 100,
                    },
                ];
            }



            // ─── Top Routes ───────────────────────────────────────────────────────
            var topRoutes = tickets
                .Where(t => t.Trip?.Route != null)
                .GroupBy(t => new
                {
                    RouteID = t.Trip!.Route!.RouteID,
                    RouteName = t.Trip.Route.RouteName,
                    CompanyID = t.Trip.Route.CompanyID,
                    CompanyName = t.Trip.Route.BusCompany?.Name ?? "Unknown",
                })
                .Select(g => new TopRouteRevenueDto
                {
                    RouteID = g.Key.RouteID,
                    RouteName = g.Key.RouteName,
                    Revenue = g.Sum(t => t.FinalPrice),
                    TicketsSold = g.Count(),
                    AverageTicketPrice = g.Average(t => t.FinalPrice),
                    CompanyID = g.Key.CompanyID,
                    CompanyName = g.Key.CompanyName,
                })
                .OrderByDescending(r => r.Revenue)
                .Take(topRoutesLimit)
                .ToList();

            // ─── Revenue by Company ───────────────────────────────────────────────
            var companyTickets = tickets.Where(t => t.Trip?.Route?.BusCompany != null).ToList();
            var totalTicketRevenue = companyTickets.Sum(t => t.FinalPrice);

            var byCompany = companyTickets
                .GroupBy(t => new
                {
                    CompanyID = t.Trip!.Route!.CompanyID,
                    CompanyName = t.Trip.Route.BusCompany!.Name,
                })
                .Select(g => new RevenueByCompanyDto
                {
                    CompanyID = g.Key.CompanyID,
                    CompanyName = g.Key.CompanyName,
                    Revenue = g.Sum(t => t.FinalPrice),
                    TicketsSold = g.Count(),
                    TripCount = g.Select(t => t.TripID).Distinct().Count(),
                    Percentage = totalTicketRevenue > 0
                        ? (g.Sum(t => t.FinalPrice) / totalTicketRevenue) * 100
                        : 0,
                })
                .OrderByDescending(c => c.Revenue)
                .Take(topCompaniesLimit)
                .ToList();

            return new RevenueAnalyticsDto
            {
                Summary = summary,
                DailyTrends = dailyTrends,
                ByProvider = byProvider,
                TopRoutes = topRoutes,
                ByCompany = byCompany,
            };
        }
    }
}

