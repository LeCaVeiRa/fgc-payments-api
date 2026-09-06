using Fgc.MessageContracts.Events;
using Fgc.Payments.Application.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fgc.Payments.UnitTests.Services
{
    public class PaymentServiceTests
    {
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<ILogger<PaymentService>> _loggerMock;
        private readonly PaymentService _service;

        public PaymentServiceTests()
        {
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _loggerMock = new Mock<ILogger<PaymentService>>();
            _service = new PaymentService(_publishEndpointMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ProcessAsync_ValidOrder_PublishesApprovedPaymentProcessedEvent()
        {
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var orderPlaced = new OrderPlacedEvent(orderId, userId, gameId, 59.90m);

            await _service.ProcessAsync(orderPlaced);

            _publishEndpointMock.Verify(p => p.Publish(
                It.Is<PaymentProcessedEvent>(e =>
                    e.OrderedId == orderId &&
                    e.UserId == userId &&
                    e.GameId == gameId &&
                    e.Price == 59.90m &&
                    e.Status == "Approved"),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
