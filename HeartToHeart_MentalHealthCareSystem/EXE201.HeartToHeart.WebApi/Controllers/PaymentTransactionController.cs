using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.BLL.Services;
using EXE201.HeartToHeart.Common.Constants;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;

namespace EXE201.HeartToHeart.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentTransactionController : ControllerBase
    {
        private readonly IVnpay _vnpay;
        private readonly IConfiguration _configuration;
        private readonly IPaymentTransactionService _paymentTransactionService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IAuthService _authService;
        private readonly ILogger<PaymentTransactionController> _logger;

        public PaymentTransactionController(IVnpay vnpay, IConfiguration configuration, IPaymentTransactionService paymentTransactionService, ISubscriptionService subscriptionService, IAuthService authService, ILogger<PaymentTransactionController> logger)
        {
            _vnpay = vnpay;
            _configuration = configuration;
            _vnpay.Initialize(_configuration["Vnpay:TmnCode"], _configuration["Vnpay:HashSecret"], _configuration["Vnpay:BaseUrl"], _configuration["Vnpay:CallbackUrl"]);
            _paymentTransactionService = paymentTransactionService;
            _subscriptionService = subscriptionService;
            _authService = authService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("CreatePaymentUrl")]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] CreatePaymentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var latestSub = await _subscriptionService.GetLatestByUserIdAsync(userId);
                var startDate = DateTime.UtcNow;
                if (latestSub != null && latestSub.Status == SubscriptionStatus.Active)
                    startDate = latestSub.EndDate;
                if (dto.DurationDays <= 0)
                    return BadRequest("Thời gian sử dụng phải lớn hơn 0 ngày.");
                if(dto.PaymentMethod == PaymentMethod.VnPay)
                {
                    var subscriptionDto = new SubscriptionDto
                    {
                        UserId = userId,
                        PlanName = dto.PlanName,
                        Amount = dto.Amount,
                        StartDate = startDate,
                        EndDate = startDate.AddDays(dto.DurationDays),
                        Status = SubscriptionStatus.Pending,
                    };
                    var subscription = await _subscriptionService.AddAsync(subscriptionDto);
                    var transactionId = DateTime.Now.Ticks;
                    var paymentTransactionDto = new PaymentTransactionDto
                    {
                        UserId = userId,
                        SubscriptionId = subscription.Id,
                        Amount = dto.Amount,
                        PaymentMethod = dto.PaymentMethod,
                        TransactionDate = DateTime.UtcNow,
                        Status = PaymentStatus.Pending,
                        TransactionId = transactionId.ToString(),
                        CreatedAt = DateTime.UtcNow,
                    };
                    var payment = await _paymentTransactionService.AddAsync(paymentTransactionDto);

                    // Prepare payment request
                    var ipAddress = NetworkHelper.GetIpAddress(HttpContext); // Lấy địa chỉ IP của thiết bị thực hiện giao dịch
                    var request = new PaymentRequest
                    {
                        PaymentId = transactionId,
                        Money = (double)subscription.Amount,
                        Description = $"Thanh toán đơn hàng #{subscription.Id} mua subscription Heart To Heart",
                        IpAddress = ipAddress,
                        BankCode = BankCode.ANY, // Tùy chọn. Mặc định là tất cả phương thức giao dịch
                        CreatedDate = DateTime.Now, // Tùy chọn. Mặc định là thời điểm hiện tại
                        Currency = Currency.VND, // Tùy chọn. Mặc định là VND (Việt Nam đồng)
                        Language = DisplayLanguage.Vietnamese // Tùy chọn. Mặc định là tiếng Việt
                    };
                    var paymentUrl = _vnpay.GetPaymentUrl(request);
                    return Ok(new
                    {
                        PaymentTransactionId = payment.Id.ToString(),
                        PaymentUrl = paymentUrl
                    });
                }
                else if (dto.PaymentMethod == PaymentMethod.SePay)
                {
                    var subscriptionDto = new SubscriptionDto
                    {
                        UserId = userId,
                        PlanName = dto.PlanName,
                        Amount = dto.Amount,
                        StartDate = startDate,
                        EndDate = startDate.AddDays(dto.DurationDays),
                        Status = SubscriptionStatus.Pending,
                    };
                    var subscription = await _subscriptionService.AddAsync(subscriptionDto);
                    var paymentCode = $"DH{DateTime.Now:MMddHHmmss}";
                    var paymentTransactionDto = new PaymentTransactionDto
                    {
                        UserId = userId,
                        SubscriptionId = subscription.Id,
                        Amount = dto.Amount,
                        PaymentMethod = dto.PaymentMethod,
                        TransactionDate = DateTime.UtcNow,
                        Status = PaymentStatus.Pending,
                        TransactionId = paymentCode,
                        CreatedAt = DateTime.UtcNow,
                    };
                    var payment = await _paymentTransactionService.AddAsync(paymentTransactionDto);
                    var qrUrl = $"https://qr.sepay.vn/img?acc=VQRQADLRE4750&bank=MBBank&amount={payment.Amount:0}&des={payment.TransactionId}";
                    return Ok(new
                    {
                        PaymentTransactionId = payment.Id.ToString(),
                        PaymentUrl = qrUrl
                    });
                }
                return BadRequest("Tạo URL Thanh toán thất bại");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("IpnAction")]
        public async Task<IActionResult> IpnActionAsync()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    _logger.LogInformation("Received Query Params: " + Request.QueryString.ToString());
                    var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                    var transactionId = paymentResult.PaymentId.ToString();
                    var paymentTransaction = await _paymentTransactionService.GetByTransactionIdAsync(transactionId);
                    var subscription = await _subscriptionService.GetByIdAsync(new List<string> { Roles.Admin }, paymentTransaction.SubscriptionId);
                    if (paymentResult.IsSuccess)
                    {
                        paymentTransaction.Status = PaymentStatus.Completed;
                        await _paymentTransactionService.UpdateAsync(paymentTransaction.Id, paymentTransaction);
                        subscription.Status = SubscriptionStatus.Active;
                        await _subscriptionService.UpdateAsync(subscription.Id, subscription);
                        await _authService.UpgradeToPremiumAsync(paymentTransaction.UserId);
                        return Ok();
                    }
                    else
                    {
                        paymentTransaction.Status = PaymentStatus.Failed;
                        await _paymentTransactionService.UpdateAsync(paymentTransaction.Id, paymentTransaction);
                        subscription.Status = SubscriptionStatus.Cancelled;
                        await _subscriptionService.UpdateAsync(subscription.Id, subscription);
                        return BadRequest("Thanh toán thất bại");
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            return NotFound("Không tìm thấy thông tin thanh toán.");
        }

        [HttpPost("sepay-webhook")]
        public async Task<IActionResult> SePayWebhook([FromBody] SePayWebhookDTO request)
        {
            var code = request.code;
            var paymentTransaction = await _paymentTransactionService.GetByTransactionIdAsync(code);
            if (paymentTransaction == null)
            {
                return NotFound("Payment transaction not found");
            }
            paymentTransaction.Status = PaymentStatus.Completed;
            await _paymentTransactionService.UpdateAsync(paymentTransaction.Id, paymentTransaction);
            var subscription = await _subscriptionService.GetByIdAsync(new List<string> { Roles.Admin }, paymentTransaction.SubscriptionId);
            if (subscription == null)
            {
                return NotFound("Subscription not found");
            }
            subscription.Status = SubscriptionStatus.Active;
            await _subscriptionService.UpdateAsync(subscription.Id, subscription);
            await _authService.UpgradeToPremiumAsync(paymentTransaction.UserId);
            return Ok();
        }

        [HttpGet("Callback")]
        public async Task<ActionResult> CallbackAsync()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                    var resultDescription = $"Payment Response: {paymentResult.PaymentResponse.Description} - Transaction Status: {paymentResult.TransactionStatus.Description}.";
                    if (paymentResult.IsSuccess)
                        return Ok(resultDescription);
                    else
                        return BadRequest(resultDescription);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            return NotFound("Không tìm thấy thông tin thanh toán.");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var paymentTransactions = await _paymentTransactionService.GetAllAsync(page, pageSize);
            var totalCount = await _paymentTransactionService.GetTotalCountAsync();
            return Ok(new
            {
                PaymentTransactions = paymentTransactions,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("my-payment-transactions")]
        public async Task<IActionResult> GetAllByUserIdAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var paymentTransactions = await _paymentTransactionService.GetByUserIdAsync(userId, page, pageSize);
            var totalCount = await _paymentTransactionService.GetTotalByUserIdCountAsync(userId);
            return Ok(new
            {
                PaymentTransactions = paymentTransactions,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("{paymentId}")]
        public async Task<IActionResult> GetByIdAsync(Guid paymentId)
        {
            var sub = await _paymentTransactionService.GetByIdAsync(paymentId);
            if (sub == null)
                return NotFound(new { Message = "Payment Transaction not found" });
            return Ok(sub);
        }

        [HttpDelete("{paymentId}")]
        public async Task<IActionResult> DeleteAsync(Guid paymentId)
        {
            var result = await _paymentTransactionService.DeleteAsync(paymentId);
            if (result)
                return Ok(new { Message = "Payment Transaction deleted successfully" });
            return BadRequest(new { Message = "Payment Transaction deleted failed" });
        }
    }
}
