using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using ProductService.Domain.Repositories;
using ProductService.Application.Events;


namespace ProductService.Application.Kafka.Consumers;

public class CheckIfProductExistsConsumer
{
    private readonly IBookRepository _bookRepository;
    private readonly IConsumer<Ignore, string> _consumer;

    public CheckIfProductExistsConsumer(IBookRepository bookRepository, string bootstrapServers)
    {
        _bookRepository = bookRepository;

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
        Task.Run(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(cancellationToken);
                var message = JsonSerializer.Deserialize<CheckIfProductExistsEvent>(result.Message.Value);

                var exists = _bookRepository.ExistsAsync(message.ProductId); // zakładamy, że masz Exists(int)

                Console.WriteLine($"Product ID {message.ProductId} " +
                                  (exists ? "exists" : "does NOT exist"));

                // Tu możesz wysłać odpowiedź Kafka, jeśli chcesz
            }
        }, cancellationToken);
    }
}
