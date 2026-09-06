using Fgc.MessageContracts.Events;
using Fgc.Payments.Application.Consumers;
using Fgc.Payments.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fgc.Payments.UnitTests.Consumers
{
    public class OrderPlacedEventConsumerTests
    {
        private readonly Mock<IPaymentService> _paymentServiceMock;
        private readonly Mock<ILogger<OrderPlacedEventConsumer>> _loggerMock;
        private readonly OrderPlacedEventConsumer _consumer;

        public OrderPlacedEventConsumerTests()
        {
            _paymentServiceMock = new Mock<IPaymentService>();
            _loggerMock = new Mock<ILogger<OrderPlacedEventConsumer>>();
            _consumer = new OrderPlacedEventConsumer(_paymentServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Consume_OrderPlacedEvent_DelegatesToPaymentService()
        {
            var message = new OrderPlacedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 59.90m);
            var context = Mock.Of<ConsumeContext<OrderPlacedEvent>>(c => c.Message == message);

            await _consumer.Consume(context);

            _paymentServiceMock.Verify(
                s => s.ProcessAsync(message, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
