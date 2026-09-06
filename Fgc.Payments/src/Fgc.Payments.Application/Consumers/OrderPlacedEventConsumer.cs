using Fgc.MessageContracts.Events;
using Fgc.Payments.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Fgc.Payments.Application.Consumers
{
    public class OrderPlacedEventConsumer(
        IPaymentService paymentService,
        ILogger<OrderPlacedEventConsumer> logger) : IConsumer<OrderPlacedEvent>
    {
        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            var message = context.Message;
            logger.LogInformation("OrderPlacedEvent received: OrderId={OrderId}, GameId={GameId}",
                message.OrderId, message.GameId);

            await paymentService.ProcessAsync(message, context.CancellationToken);
        }
    }
}
