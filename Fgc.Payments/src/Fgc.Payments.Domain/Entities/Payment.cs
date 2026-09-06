using Fgc.Payments.Domain.Exceptions;

namespace Fgc.Payments.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; private set; }
        public Guid UserId { get; private set; }
        public Guid GameId { get; private set; }
        public decimal Price { get; private set; }
        public string Status { get; private set; } = null!;
        public DateTime ProcessedAt { get; private set; }

        private Payment() { }

        // Processamento simulado: sempre aprova. Não há regra de negócio real por trás disso -
        // Payments é um scaffold de simulação (ver README).
        public static Payment Approve(Guid orderId, Guid userId, Guid gameId, decimal price)
        {
            Validate(orderId, userId, gameId);

            return new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                UserId = userId,
                GameId = gameId,
                Price = price,
                Status = "Approved",
                ProcessedAt = DateTime.UtcNow
            };
        }

        private static void Validate(Guid orderId, Guid userId, Guid gameId)
        {
            if (orderId == Guid.Empty)
                throw new PaymentDomainException("OrderId is required.");

            if (userId == Guid.Empty)
                throw new PaymentDomainException("UserId is required.");

            if (gameId == Guid.Empty)
                throw new PaymentDomainException("GameId is required.");
        }
    }
}
