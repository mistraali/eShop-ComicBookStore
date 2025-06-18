using CartService.Application.Services;
using Confluent.Kafka;
using ProductService.Domain.Events;
using System.Text.Json;
using UserService.Domain.Events;

namespace CartService.Kafka;

public class KafkaProductEventsConsumer : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<KafkaProductEventsConsumer> _logger;

    public KafkaProductEventsConsumer(IServiceProvider services, ILogger<KafkaProductEventsConsumer> logger)
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
        using (var producer = new ProducerBuilder<Null, string>(producerConfig).Build())
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
            GroupId = "cart-product-response",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe("product-response");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                _logger.LogInformation($"Odebrano event: {result.Message.Value}");

                var response = JsonSerializer.Deserialize<KafkaProductResponseEvent>(result.Message.Value);

                if (response.Exists)
                {
                    using (var scope = _services.CreateScope())
                    {
                        var cartService = scope.ServiceProvider.GetRequiredService<ICartService>();
                        await cartService.AttachProductNameToPendingCartItemAsync(
                            response.RequestedId,
                            response.Name);
                    }
                }
                else
                {
                    _logger.LogWarning($"Produkt o ID {response.RequestedId} nie istnieje.");
                    // Mozna dodać obsługę błędu...
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


