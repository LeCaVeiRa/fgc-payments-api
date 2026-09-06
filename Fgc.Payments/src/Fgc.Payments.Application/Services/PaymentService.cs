using Fgc.MessageContracts.Events;
using Fgc.Payments.Application.Interfaces;
using Fgc.Payments.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Fgc.Payments.Application.Services
{
    public class PaymentService(
        IPublishEndpoint publishEndpoint,
        ILogger<PaymentService> logger) : IPaymentService
    {
        public async Task ProcessAsync(OrderPlacedEvent orderPlaced, CancellationToken cancellationToken = default)
        {
            var payment = Payment.Approve(orderPlaced.OrderId, orderPlaced.UserId, orderPlaced.GameId, orderPlaced.Price);

            logger.LogInformation(
                "Payment {PaymentId} for order {OrderId} processed with status {Status}",
                payment.Id, payment.OrderId, payment.Status);

            await publishEndpoint.Publish(
                new PaymentProcessedEvent(
                    payment.OrderId,
                    payment.UserId,
                    payment.GameId,
                    payment.Price,
                    payment.Status,
                    payment.ProcessedAt),
                cancellationToken);
        }
    }
}
