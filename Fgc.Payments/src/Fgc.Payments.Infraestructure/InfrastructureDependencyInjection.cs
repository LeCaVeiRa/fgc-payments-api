using Fgc.Payments.Application.Consumers;
using Fgc.Payments.Application.Interfaces;
using Fgc.Payments.Application.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fgc.Payments.Infraestructure
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IPaymentService, PaymentService>();

            // Local (docker-compose): RabbitMq:UseSsl ausente -> broker self-hosted em texto plano.
            // Produção: RabbitMq:UseSsl=true apontaria para o endpoint AMQPS do Amazon MQ (porta 5671).
            var rabbitHost = configuration["RabbitMq:Host"] ?? "localhost";
            var rabbitUseSsl = configuration.GetValue<bool>("RabbitMq:UseSsl");
            var rabbitPort = configuration.GetValue<int?>("RabbitMq:Port") ?? (rabbitUseSsl ? 5671 : 5672);
            var rabbitVirtualHost = configuration["RabbitMq:VirtualHost"] ?? "/";
            var rabbitUsername = configuration["RabbitMq:Username"] ?? "admin";
            var rabbitPassword = configuration["RabbitMq:Password"] ?? "admin";

            services.AddMassTransit(x =>
            {
                x.AddConsumer<OrderPlacedEventConsumer>();
                x.UsingRabbitMq((context, cfg) =>
                {
                    if (rabbitUseSsl)
                    {
                        cfg.Host(new Uri($"rabbitmqs://{rabbitHost}:{rabbitPort}{rabbitVirtualHost}"), h =>
                        {
                            h.Username(rabbitUsername);
                            h.Password(rabbitPassword);
                        });
                    }
                    else
                    {
                        cfg.Host(rabbitHost, rabbitVirtualHost, h =>
                        {
                            h.Username(rabbitUsername);
                            h.Password(rabbitPassword);
                        });
                    }

                    cfg.ReceiveEndpoint("payments-order-placed-queue", e =>
                    {
                        e.ConfigureConsumer<OrderPlacedEventConsumer>(context);
                    });
                });
            });

            return services;
        }
    }
}
