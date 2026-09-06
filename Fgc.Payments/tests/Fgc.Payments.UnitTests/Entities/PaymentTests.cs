using Fgc.Payments.Domain.Entities;
using Fgc.Payments.Domain.Exceptions;

namespace Fgc.Payments.UnitTests.Entities
{
    public class PaymentTests
    {
        [Fact]
        public void Approve_ValidData_ReturnsApprovedPayment()
        {
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            var payment = Payment.Approve(orderId, userId, gameId, 59.90m);

            Assert.NotEqual(Guid.Empty, payment.Id);
            Assert.Equal(orderId, payment.OrderId);
            Assert.Equal(userId, payment.UserId);
            Assert.Equal(gameId, payment.GameId);
            Assert.Equal(59.90m, payment.Price);
            Assert.Equal("Approved", payment.Status);
        }

        [Fact]
        public void Approve_EmptyOrderId_ThrowsPaymentDomainException()
        {
            Assert.Throws<PaymentDomainException>(
                () => Payment.Approve(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), 10m));
        }

        [Fact]
        public void Approve_EmptyUserId_ThrowsPaymentDomainException()
        {
            Assert.Throws<PaymentDomainException>(
                () => Payment.Approve(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), 10m));
        }

        [Fact]
        public void Approve_EmptyGameId_ThrowsPaymentDomainException()
        {
            Assert.Throws<PaymentDomainException>(
                () => Payment.Approve(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 10m));
        }
    }
}
