using System.ComponentModel.DataAnnotations;
using Pbl3.Enums;

namespace Pbl3.Models
{
    public class Refund
    {
        [Key]
        public Guid RefundID { get; set; } = Guid.NewGuid();
        public Guid IntentID { get; set; }
        public PaymentIntent? PaymentIntent { get; set; }

        public decimal Amount { get; set; }
        public string? Reason { get; set; }
        public RefundStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
