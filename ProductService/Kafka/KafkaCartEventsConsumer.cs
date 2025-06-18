using Microsoft.AspNetCore.Cors.Infrastructure;
using Confluent.Kafka;
using System.Text.Json;
using CartService.Domain.Events;
using CartService.Application.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductService.Domain.Events;
using ProductService.Application.Services;

namespace ProductService.Kafka;

public class KafkaCartEventsConsumer : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<KafkaCartEventsConsumer> _logger;

    public KafkaCartEventsConsumer(IServiceProvider services, ILogger<KafkaCartEventsConsumer> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Retry logic przez PRODUCENTA
        bool kafkaReady = false;
        int retries = 10;

        var producerConfig = new ProducerConfig { BootstrapServers = "kafka:9092" };
        using var producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        
        {
            while (!kafkaReady && retries-- > 0 && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await producer.ProduceAsync("health-check", new Message<Null, string> { Value = "ping" });
                    kafkaReady = true;
                    _logger.LogInformation("Kafka dostępna. Rozpoczynam nasłuchiwanie.");
                }
                catch (Exception)
                {
                    _logger.LogWarning($"Kafka niedostępna... retry ({retries} pozostało)");
                    await Task.Delay(3000, stoppingToken);
                }
            }
        }

        if (!kafkaReady)
        {
            _logger.LogError("Kafka nadal niedostępna. Przerywam start konsumenta.");
            return;
        }

        var config = new ConsumerConfig
        {
            BootstrapServers = "kafka:9092",
            GroupId = "product-service",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe("cart-events");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                _logger.LogInformation($"Odebrano event: {result.Message.Value}");

                var @event = JsonSerializer.Deserialize<KafkaProductRequestedEvent>(result.Message.Value);

                using (var scope = _services.CreateScope())
                {
                    var productService = scope.ServiceProvider.GetRequiredService<IBookService>();
                    var book = await productService.GetBookByIdAsync(@event.ProductId);

                    var response = new KafkaProductResponseEvent
                    {
                        RequestedId = @event.RequestedId,
                        ProductId = book.Id,
                        Name = book.Name,
                        Exists = book != null
                    };

                    var json = JsonSerializer.Serialize(response);
                    await producer.ProduceAsync("product-response", new Message<Null, string> { Value = json });

                    _logger.LogInformation($"Wysłano odpowiedź o produkcie o Id {@event.ProductId}: {response.Exists}");
                }


            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Błąd podczas przetwarzania eventów z Kafka.");

                if (ex.Error.Code == ErrorCode.UnknownTopicOrPart || ex.Error.Code == ErrorCode.LeaderNotAvailable)
                {
                    await Task.Delay(5000, stoppingToken);
                    continue;
                }

                throw;
            }
        }
    }
}
