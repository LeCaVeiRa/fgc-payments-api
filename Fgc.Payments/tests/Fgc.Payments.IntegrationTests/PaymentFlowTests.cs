using Fgc.MessageContracts.Events;
using Fgc.Payments.Application.Consumers;
using Fgc.Payments.Application.Interfaces;
using Fgc.Payments.Application.Services;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Fgc.Payments.IntegrationTests
{
    public class PaymentFlowTests : IAsyncLifetime
    {
        private ServiceProvider _provider = null!;
        private ITestHarness _harness = null!;

        public async Task InitializeAsync()
        {
            _provider = new ServiceCollection()
                .AddLogging()
                .AddScoped<IPaymentService, PaymentService>()
                .AddMassTransitTestHarness(config =>
                {
                    config.SetTestTimeouts(
                        testTimeout: TimeSpan.FromSeconds(5),
                        testInactivityTimeout: TimeSpan.FromSeconds(3));

                    config.AddConsumer<OrderPlacedEventConsumer>();
                })
                .BuildServiceProvider(true);

            _harness = await _provider.StartTestHarness();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            if (_provider is not null)
                await _provider.DisposeAsync();
        }

        [Fact]
        public async Task Should_Consume_OrderPlacedEvent_And_Publish_ApprovedPaymentProcessedEvent()
        {
            var orderEvent = new OrderPlacedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 59.90m);

            await _harness.Bus.Publish(orderEvent);

            Assert.True(await _harness.Consumed.Any<OrderPlacedEvent>());
            Assert.True(await _harness.Published.Any<PaymentProcessedEvent>(
                x => x.Context.Message.Status == "Approved"
                     && x.Context.Message.OrderedId == orderEvent.OrderId));
        }
    }
}
