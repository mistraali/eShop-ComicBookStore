using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using ProductService.Domain.Repositories;
using ProductService.Application.Events;
using ProductService.Application.Kafka.Producers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;


namespace ProductService.Application.Kafka.Consumers;

public class CheckIfProductExistsConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ProductKafkaProducer _producer;
    private readonly IConsumer<Ignore, string> _consumer;

    public CheckIfProductExistsConsumer(IServiceScopeFactory scopeFactory, ProductKafkaProducer producer, IOptions<KafkaSettings> options)
    {
        _scopeFactory = scopeFactory;
        _producer = producer;

        var config = new ConsumerConfig
        {
            GroupId = "product-service-group",
            BootstrapServers = options.Value.BootstrapServers,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        _consumer.Subscribe("check-product-exists");
    }

    public void StartConsuming(CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(cancellationToken);
                var message = JsonSerializer.Deserialize<CheckIfProductExistsEvent>(result.Message.Value);

                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IBookRepository>();

                var exists = await repo.ExistsAsync(message.ProductId);

                var response = new ProductExistsEvent
                {
                    ProductId = message.ProductId,
                    Exists = exists
                };

                Console.WriteLine($"Product ID {message.ProductId} " +
                                  (exists ? "exists" : "does NOT exist"));

                // Tu możesz wysłać odpowiedź Kafka, jeśli chcesz
            }
        }, cancellationToken);
    }
}
