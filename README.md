# 💳 FCG Payments API

Serviço em **.NET 8** que faz parte do Tech Challenge da pós-graduação **Arquitetura de Sistemas .NET – FIAP**.

Representa o microsserviço de **Pagamentos** da plataforma de games educacionais (**FCG – FIAP Cloud Games**). Processa (simula) o pagamento de um pedido de forma assíncrona via mensageria.

> **Este serviço não expõe nenhum endpoint HTTP.** É um consumidor/publicador puro de RabbitMQ: o host ASP.NET Core existe apenas para hospedar o Swagger (hoje vazio) e o healthcheck do container — não há controllers implementados.

---

## 📌 Objetivo do Projeto

- Consumir `OrderPlacedEvent` via RabbitMQ.
- Simular o processamento do pagamento (hoje sempre aprova — não há caminho de rejeição implementado).
- Publicar `PaymentProcessedEvent` com o resultado.
- Containerização com Docker.

---

## 🛠️ Tecnologias Utilizadas

- **.NET 8** / ASP.NET Core (apenas como host, sem controllers)
- **MassTransit** + **RabbitMQ** (Amazon MQ/AMQPS em produção)
- **Docker**
- **Swagger / OpenAPI** (sem endpoints documentados)
- **ILogger** para logs estruturados

Sem banco de dados: o `Payment` é um objeto construído em memória apenas para montar o evento de saída, e descartado em seguida.

---

## 🧱 Arquitetura

Clean Architecture, quatro projetos sob `Fgc.Payments/src/` (a camada de infraestrutura é `Fgc.Payments.Infraestructure`, com essa grafia):

- **`Fgc.Payments.Api`** — `Program.cs` (composição, `AddInfrastructure`), Swagger. `Controllers/` existe como pasta vazia; não há endpoints.
- **`Fgc.Payments.Application`** — `Consumers/OrderPlacedEventConsumer.cs` (consome `OrderPlacedEvent`), `Services/PaymentService.cs` (monta o `Payment` e publica `PaymentProcessedEvent`).
- **`Fgc.Payments.Domain`** — Entidade `Payment` (`Payment.Approve(...)`, sempre aprova), `PaymentDomainException`.
- **`Fgc.Payments.Infraestructure`** — `InfrastructureDependencyInjection.cs`: wiring do MassTransit/RabbitMQ (`AddConsumer<OrderPlacedEventConsumer>`, fila `payments-order-placed-queue`, seleção de host AMQP/AMQPS via `RabbitMq:UseSsl`).

---

## 📁 Estrutura de Pastas

```text
Fgc.Payments/
├── src/
│   ├── Fgc.Payments.Api/
│   │   ├── Controllers/        # pasta vazia, sem endpoints implementados
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── Fgc.Payments.Application/
│   │   ├── Consumers/
│   │   └── Services/
│   ├── Fgc.Payments.Domain/
│   │   └── Entities/
│   └── Fgc.Payments.Infraestructure/
│       └── InfrastructureDependencyInjection.cs
└── tests/
    ├── Fgc.Payments.UnitTests/
    └── Fgc.Payments.IntegrationTests/
```

---

## 📨 Mensageria e Eventos

* **Consome:** `Fgc.MessageContracts.Events.OrderPlacedEvent`, fila `payments-order-placed-queue`.
* **Publica:** `Fgc.MessageContracts.Events.PaymentProcessedEvent`:

```csharp
public record PaymentProcessedEvent(
    Guid OrderedId,   // nome de campo real do contrato (não "OrderId")
    Guid UserId,
    Guid GameId,
    decimal Price,
    string Status,     // hoje sempre "Approved"
    DateTime ProcessedAt);
```

Não há autenticação/JWT neste serviço (não tem endpoints HTTP).

---

## 📦 Pacote `Fgc.MessageContracts`

Referenciado via NuGet local (`nuget.config` aponta para `./LocalPackages`), versão **1.0.3** em `Fgc.Payments.Api` e `Fgc.Payments.Application`. `LocalPackages/` contém os `.nupkg` de 1.0.1 (não usado) e 1.0.3. O `Dockerfile` copia `LocalPackages/` e `nuget.config` antes do `dotnet restore`.

---

## 📘 Testes

* `Fgc.Payments.UnitTests` — `PaymentTests`, `PaymentServiceTests`, `OrderPlacedEventConsumerTests` (xUnit + Moq).
* `Fgc.Payments.IntegrationTests` — `PaymentFlowTests`, usando `MassTransit.Testing` (harness em memória, sem broker real): publica `OrderPlacedEvent` e verifica que `PaymentProcessedEvent` com `Status == "Approved"` é publicado.

```bash
dotnet test
dotnet test tests/Fgc.Payments.UnitTests
dotnet test tests/Fgc.Payments.IntegrationTests
```

---

## ▶️ Como Executar o Projeto

### Pré-requisitos

* .NET SDK 8+
* RabbitMQ rodando (via `fgc-orchestration/` ou standalone)

### Variáveis de Ambiente (`appsettings.json`)

* `RabbitMq:Host`, `RabbitMq:Port`, `RabbitMq:VirtualHost`, `RabbitMq:Username`, `RabbitMq:Password`, `RabbitMq:UseSsl`

### Execução via Docker

Não há `docker-compose.yml` neste repositório; suba a stack completa a partir de `fgc-orchestration/`:

```bash
cd ../fgc-orchestration
docker-compose up -d --build
```

### Execução Local

```bash
dotnet restore
dotnet run --project Fgc.Payments/src/Fgc.Payments.Api
```

Como não há endpoints HTTP, verificar o funcionamento é feito via log (`ILogger`) ao publicar um `OrderPlacedEvent`, ou rodando os testes de integração.

---

## 👥 Squad 8 – Turma 12NETT

**Integrantes**

* Yan Santos Wendt
* Ronnam de Lima da Silva
