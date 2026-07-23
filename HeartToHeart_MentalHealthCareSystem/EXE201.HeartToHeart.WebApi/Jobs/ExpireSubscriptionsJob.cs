using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.Common.Constants;
using Quartz;

namespace EXE201.HeartToHeart.WebApi.Jobs
{
    public class ExpireSubscriptionsJob : IJob
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IAuthService _authService;
        private readonly IPaymentTransactionService _paymentTransactionService;
        private readonly ILogger<ExpireSubscriptionsJob> _logger;

        public ExpireSubscriptionsJob(ISubscriptionService subscriptionService, IAuthService authService, IPaymentTransactionService paymentTransactionService, ILogger<ExpireSubscriptionsJob> logger)
        {
            _subscriptionService = subscriptionService;
            _authService = authService;
            _paymentTransactionService = paymentTransactionService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("Running ExpireSubscriptionsJob...");

                var expiredSubscriptions = await _subscriptionService.GetAllExpiredButStillActiveAsync(DateTime.UtcNow);
                foreach (var subscription in expiredSubscriptions)
                {
                    subscription.Status = SubscriptionStatus.Expired;
                    await _subscriptionService.UpdateAsync(subscription.Id, subscription);
                    var latestSub = await _subscriptionService.GetLatestByUserIdAsync(subscription.UserId);
                    if (latestSub == null)
                        await _authService.DowngradeToMemberAsync(subscription.UserId);
                    _logger.LogInformation($"Expired subscription {subscription.Id}");
                }
                var pendingSubscriptions = await _subscriptionService.GetPendingSubscriptionsFromYesterdayAsync();
                foreach (var subscription in pendingSubscriptions)
                {
                    subscription.Status = SubscriptionStatus.Cancelled;
                    await _subscriptionService.UpdateAsync(subscription.Id, subscription);
                    var paymentTransaction = await _paymentTransactionService.GetPaymentBySubscriptionIdAsync(subscription.Id);
                    paymentTransaction.Status = PaymentStatus.Failed;
                    await _paymentTransactionService.UpdateAsync(paymentTransaction.Id, paymentTransaction);
                    _logger.LogInformation($"Updated pending subscription {subscription.Id}, payment transaction {paymentTransaction.Id}");
                }
                _logger.LogInformation("ExpireSubscriptionsJob finished.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running ExpireSubscriptionsJob");
            }
        }
    }
}
