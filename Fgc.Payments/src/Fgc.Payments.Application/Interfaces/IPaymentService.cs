using Fgc.MessageContracts.Events;

namespace Fgc.Payments.Application.Interfaces
{
    public interface IPaymentService
    {
        Task ProcessAsync(OrderPlacedEvent orderPlaced, CancellationToken cancellationToken = default);
    }
}
