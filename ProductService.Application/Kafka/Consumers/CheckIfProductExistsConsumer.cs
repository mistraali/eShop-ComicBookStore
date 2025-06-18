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


namespace ProductService.Application.Kafka.Consumers;

public class CheckIfProductExistsConsumer
{
    private readonly IBookRepository _bookRepository;
    private readonly ProductKafkaProducer _producer;
    private readonly IConsumer<Ignore, string> _consumer;

    public CheckIfProductExistsConsumer(IBookRepository bookRepository, ProductKafkaProducer producer, string bootstrapServers)
    {
        _bookRepository = bookRepository;
        _producer = producer;

        var config = new ConsumerConfig
        {
            GroupId = "product-service-group",
            BootstrapServers = bootstrapServers,
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

                var exists = await _bookRepository.ExistsAsync(message.ProductId);

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
