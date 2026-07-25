using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pbl3.Configurations;
using Pbl3.Data;
using Pbl3.Dtos;
using Pbl3.Enums;
using Pbl3.Models;
using Pbl3.Utils;
using System.Net;
using System.Text;

namespace Pbl3.Services
{
    public interface IVnpayService
    {
        Task<CreateVnpayPaymentResponseDto> CreateVnpayPaymentAsync(Guid bookingId, Guid userId, string ipAddress);
        Task HandleVnpayIpnAsync(VnpayIpnRequestDto request);
        Task<VnpayReturnResultDto> VerifyVnpayReturnAsync(VnpayIpnRequestDto request, Guid userId);
    }

    public class VnpayService : IVnpayService
    {
        private readonly ApplicationDbContext _context;
        private readonly VnpayOptions _vnpayOptions;

        public VnpayService(ApplicationDbContext context, IOptions<VnpayOptions> vnpayOptions)
        {
            _context = context;
            _vnpayOptions = vnpayOptions.Value;
        }

        public async Task<CreateVnpayPaymentResponseDto> CreateVnpayPaymentAsync(Guid bookingId, Guid userId, string ipAddress)
        {
            ValidateVnpayOptions();

            var booking = await _context.Bookings
                .Include(b => b.PaymentIntents)
                .FirstOrDefaultAsync(b => b.BookingID == bookingId && b.UserID == userId);

            if (booking == null)
            {
                throw new KeyNotFoundException("Không tìm thấy booking.");
            }

            if (booking.Status == BookingStatus.Paid)
            {
                throw new InvalidOperationException("Booking này đã được thanh toán.");
            }

            if (booking.TotalAmount <= 0)
            {
                throw new InvalidOperationException("Số tiền thanh toán không hợp lệ.");
            }

            var existingSucceeded = booking.PaymentIntents.FirstOrDefault(pi =>
                pi.Provider == PaymentProvider.Vnpay && pi.Status == PaymentIntentStatus.Succeeded
            );
            if (existingSucceeded != null)
            {
                throw new InvalidOperationException("Booking này đã được thanh toán thành công.");
            }

            var intent = booking.PaymentIntents.FirstOrDefault(pi =>
                pi.Provider == PaymentProvider.Vnpay && pi.Status == PaymentIntentStatus.Created
            );

            var amount = decimal.Truncate(booking.TotalAmount);
            var orderId = intent?.ProviderOrderId ?? $"{booking.BookingID:N}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            var orderInfo = $"Thanh toan booking {booking.BookingID}";

            var vnpayParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", _vnpayOptions.TmnCode },
                { "vnp_Amount", ((long)(amount * 100)).ToString() },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", string.IsNullOrWhiteSpace(ipAddress) ? "127.0.0.1" : ipAddress },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", orderInfo },
                { "vnp_OrderType", "other" },
                { "vnp_ReturnUrl", _vnpayOptions.ReturnUrl },
                { "vnp_TxnRef", orderId }
            };

            var signDataBuilder = new StringBuilder();
            foreach (var kvp in vnpayParams)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    signDataBuilder.Append(Uri.EscapeDataString(kvp.Key) + "=" + Uri.EscapeDataString(kvp.Value) + "&");
                }
            }

            var signData = signDataBuilder.ToString();
            if (signData.EndsWith("&"))
            {
                signData = signData.Substring(0, signData.Length - 1);
            }

            var secureHash = VnpaySignatureHelper.ComputeHmacSha512(_vnpayOptions.HashSecret, signData);
            var payUrl = $"{_vnpayOptions.BaseUrl}?{signData}&vnp_SecureHash={secureHash}";

            if (intent == null)
            {
                intent = new PaymentIntent
                {
                    BookingID = booking.BookingID,
                    Provider = PaymentProvider.Vnpay,
                    Amount = booking.TotalAmount,
                    Currency = "VND",
                    Status = PaymentIntentStatus.Created,
                    ProviderOrderId = orderId,
                    PayUrl = payUrl,
                    CreatedAt = DateTime.UtcNow,
                };
                _context.PaymentIntents.Add(intent);
            }
            else
            {
                intent.PayUrl = payUrl;
            }

            await _context.SaveChangesAsync();

            return new CreateVnpayPaymentResponseDto
            {
                IntentId = intent.IntentID,
                BookingId = booking.BookingID,
                OrderId = orderId,
                Amount = intent.Amount,
                Currency = intent.Currency,
                Status = intent.Status,
                PayUrl = intent.PayUrl ?? string.Empty
            };
        }

        public async Task HandleVnpayIpnAsync(VnpayIpnRequestDto request)
        {
            await ProcessCallbackInternalAsync(request);
        }

        public async Task<VnpayReturnResultDto> VerifyVnpayReturnAsync(VnpayIpnRequestDto request, Guid userId)
        {
            ValidateVnpayOptions();
            
            PaymentIntent? intent = null;
            try
            {
                intent = await ProcessCallbackInternalAsync(request);
                if (intent.Booking?.UserID != userId)
                {
                    throw new UnauthorizedAccessException("Không có quyền xem giao dịch này.");
                }

                var success = request.vnp_ResponseCode == "00" && request.vnp_TransactionStatus == "00";
                return new VnpayReturnResultDto
                {
                    RedirectUrl = _vnpayOptions.ReturnUrl,
                    IntentId = intent.IntentID,
                    BookingId = intent.BookingID,
                    ResponseCode = request.vnp_ResponseCode,
                    Message = success ? "Thanh toán thành công" : "Thanh toán không thành công"
                };
            }
            catch (Exception ex)
            {
                intent ??= await _context.PaymentIntents
                    .Include(pi => pi.Booking)
                    .FirstOrDefaultAsync(pi => pi.Provider == PaymentProvider.Vnpay && pi.ProviderOrderId == request.vnp_TxnRef);

                if (intent?.Booking?.UserID != userId)
                {
                    throw;
                }

                return new VnpayReturnResultDto
                {
                    RedirectUrl = _vnpayOptions.ReturnUrl,
                    IntentId = intent?.IntentID,
                    BookingId = intent?.BookingID,
                    ResponseCode = request.vnp_ResponseCode ?? "99",
                    Message = ex.Message
                };
            }
        }

        private async Task<PaymentIntent> ProcessCallbackInternalAsync(VnpayIpnRequestDto request)
        {
            ValidateVnpayOptions();

            var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal);
            
            if (request.vnp_Amount > 0) vnpParams.Add("vnp_Amount", request.vnp_Amount.ToString());
            if (!string.IsNullOrEmpty(request.vnp_BankCode)) vnpParams.Add("vnp_BankCode", request.vnp_BankCode);
            if (!string.IsNullOrEmpty(request.vnp_BankTranNo)) vnpParams.Add("vnp_BankTranNo", request.vnp_BankTranNo);
            if (!string.IsNullOrEmpty(request.vnp_CardType)) vnpParams.Add("vnp_CardType", request.vnp_CardType);
            if (!string.IsNullOrEmpty(request.vnp_OrderInfo)) vnpParams.Add("vnp_OrderInfo", request.vnp_OrderInfo);
            if (!string.IsNullOrEmpty(request.vnp_PayDate)) vnpParams.Add("vnp_PayDate", request.vnp_PayDate);
            if (!string.IsNullOrEmpty(request.vnp_ResponseCode)) vnpParams.Add("vnp_ResponseCode", request.vnp_ResponseCode);
            if (!string.IsNullOrEmpty(request.vnp_TmnCode)) vnpParams.Add("vnp_TmnCode", request.vnp_TmnCode);
            if (!string.IsNullOrEmpty(request.vnp_TransactionNo)) vnpParams.Add("vnp_TransactionNo", request.vnp_TransactionNo);
            if (!string.IsNullOrEmpty(request.vnp_TransactionStatus)) vnpParams.Add("vnp_TransactionStatus", request.vnp_TransactionStatus);
            if (!string.IsNullOrEmpty(request.vnp_TxnRef)) vnpParams.Add("vnp_TxnRef", request.vnp_TxnRef);

            var signDataBuilder = new StringBuilder();
            foreach (var kvp in vnpParams)
            {
                signDataBuilder.Append(Uri.EscapeDataString(kvp.Key) + "=" + Uri.EscapeDataString(kvp.Value) + "&");
            }

            var signData = signDataBuilder.ToString();
            if (signData.EndsWith("&"))
            {
                signData = signData.Substring(0, signData.Length - 1);
            }

            var expectedSignature = VnpaySignatureHelper.ComputeHmacSha512(_vnpayOptions.HashSecret, signData);
            if (!string.Equals(expectedSignature, request.vnp_SecureHash, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Chữ ký IPN VNPAY không hợp lệ.");
            }

            var intent = await _context.PaymentIntents
                .Include(pi => pi.Booking)
                    .ThenInclude(booking => booking!.Tickets)
                .FirstOrDefaultAsync(pi => pi.Provider == PaymentProvider.Vnpay && pi.ProviderOrderId == request.vnp_TxnRef);

            if (intent == null)
            {
                throw new KeyNotFoundException("Không tìm thấy giao dịch VNPAY.");
            }

            if (!long.TryParse(request.vnp_TransactionNo, out long transId))
            {
                transId = 0;
            }
            intent.ProviderTransactionId = transId;
            intent.ProviderMessage = request.vnp_ResponseCode == "00" ? "Success" : $"Fail ({request.vnp_ResponseCode})";

            if (int.TryParse(request.vnp_ResponseCode, out int resCode))
            {
                intent.ProviderResultCode = resCode;
            }

            var success = request.vnp_ResponseCode == "00" && request.vnp_TransactionStatus == "00";
            if (success)
            {
                intent.Status = PaymentIntentStatus.Succeeded;
                intent.PaidAt ??= DateTime.UtcNow;

                if (intent.Booking != null)
                {
                    intent.Booking.Status = BookingStatus.Paid;
                    intent.Booking.ExpiresAt = null;

                    foreach (var ticket in intent.Booking.Tickets)
                    {
                        if (ticket.Status == TicketStatus.Cancelled) continue;
                        ticket.Status = TicketStatus.Issued;
                    }
                }
            }
            else if (intent.Status != PaymentIntentStatus.Succeeded)
            {
                intent.Status = PaymentIntentStatus.Failed;
                if (intent.Booking != null && intent.Booking.Status != BookingStatus.Paid)
                {
                    intent.Booking.Status = BookingStatus.Cancelled;
                    intent.Booking.ExpiresAt = null;

                    foreach (var ticket in intent.Booking.Tickets)
                    {
                        ticket.Status = TicketStatus.Cancelled;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return intent;
        }

        private void ValidateVnpayOptions()
        {
            if (string.IsNullOrWhiteSpace(_vnpayOptions.TmnCode) ||
                string.IsNullOrWhiteSpace(_vnpayOptions.HashSecret) ||
                string.IsNullOrWhiteSpace(_vnpayOptions.BaseUrl) ||
                string.IsNullOrWhiteSpace(_vnpayOptions.ReturnUrl))
            {
                throw new InvalidOperationException("Cấu hình VNPAY chưa đầy đủ.");
            }
        }
    }
}
