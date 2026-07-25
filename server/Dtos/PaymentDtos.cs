using System.ComponentModel.DataAnnotations;
using Pbl3.Enums;

namespace Pbl3.Dtos
{
    public class CreateMomoPaymentRequestDto
    {
        [Required]
        public Guid BookingId { get; set; }
    }

    public class CreateMomoPaymentResponseDto
    {
        public Guid IntentId { get; set; }
        public Guid BookingId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public PaymentIntentStatus Status { get; set; }
        public string PayUrl { get; set; } = string.Empty;
        public string? QrCodeUrl { get; set; }
        public string? Deeplink { get; set; }
        public string? Message { get; set; }
    }

    public class PaymentStatusDto
    {
        public Guid IntentId { get; set; }
        public Guid BookingId { get; set; }
        public PaymentProvider Provider { get; set; }
        public PaymentIntentStatus Status { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public string? PayUrl { get; set; }
        public string? QrCodeUrl { get; set; }
        public string? Deeplink { get; set; }
        public string? Message { get; set; }
        public long? ProviderTransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class MomoReturnResultDto
    {
        public string RedirectUrl { get; set; } = string.Empty;
        public Guid? IntentId { get; set; }
        public Guid? BookingId { get; set; }
        public int ResultCode { get; set; }
        public string? Message { get; set; }
    }

    public class MomoIpnRequestDto
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
        public string OrderType { get; set; } = string.Empty;
        public long TransId { get; set; }
        public int ResultCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PayType { get; set; } = string.Empty;
        public long ResponseTime { get; set; }
        public string ExtraData { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }

    public class CreateVnpayPaymentResponseDto
    {
        public Guid IntentId { get; set; }
        public Guid BookingId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public PaymentIntentStatus Status { get; set; }
        public string PayUrl { get; set; } = string.Empty;
        public string? Message { get; set; }
    }

    public class VnpayReturnResultDto
    {
        public string RedirectUrl { get; set; } = string.Empty;
        public Guid? IntentId { get; set; }
        public Guid? BookingId { get; set; }
        public string? ResponseCode { get; set; }
        public string? Message { get; set; }
    }

    public class VnpayIpnRequestDto
    {
        public long vnp_Amount { get; set; }
        public string? vnp_BankCode { get; set; }
        public string? vnp_BankTranNo { get; set; }
        public string? vnp_CardType { get; set; }
        public string? vnp_OrderInfo { get; set; }
        public string? vnp_PayDate { get; set; }
        public string? vnp_ResponseCode { get; set; }
        public string? vnp_TmnCode { get; set; }
        public string? vnp_TransactionNo { get; set; }
        public string? vnp_TransactionStatus { get; set; }
        public string? vnp_TxnRef { get; set; }
        public string? vnp_SecureHash { get; set; }
    }
}
