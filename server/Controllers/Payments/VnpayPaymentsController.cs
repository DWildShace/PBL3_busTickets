using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pbl3.Dtos;
using Pbl3.Services;

namespace Pbl3.Controllers.Payments
{
    [ApiController]
    [Route("api/payments/vnpay")]
    public class VnpayPaymentsController : ControllerBase
    {
        private readonly IVnpayService _vnpayService;
        private readonly ICurrentUserContext _currentUserContext;

        public VnpayPaymentsController(
            IVnpayService vnpayService,
            ICurrentUserContext currentUserContext
        )
        {
            _vnpayService = vnpayService;
            _currentUserContext = currentUserContext;
        }

        [HttpPost("create")]
        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> CreatePayment([FromBody] CreateMomoPaymentRequestDto dto)
        {
            try
            {
                var userId = _currentUserContext.GetRequiredUserId();
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                var result = await _vnpayService.CreateVnpayPaymentAsync(dto.BookingId, userId, ipAddress);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleIpn([FromQuery] VnpayIpnRequestDto dto)
        {
            try
            {
                await _vnpayService.HandleVnpayIpnAsync(dto);
                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            }
            catch (KeyNotFoundException)
            {
                return Ok(new { RspCode = "01", Message = "Order not found" });
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("Chữ ký"))
                {
                    return Ok(new { RspCode = "97", Message = "Invalid signature" });
                }
                return Ok(new { RspCode = "99", Message = ex.Message });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VNPAY IPN Error: {ex.Message}");
                return Ok(new { RspCode = "99", Message = "Input required data" });
            }
        }

        [HttpPost("return/verify")]
        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> VerifyReturn([FromBody] VnpayIpnRequestDto dto)
        {
            try
            {
                var userId = _currentUserContext.GetRequiredUserId();
                var result = await _vnpayService.VerifyVnpayReturnAsync(dto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
