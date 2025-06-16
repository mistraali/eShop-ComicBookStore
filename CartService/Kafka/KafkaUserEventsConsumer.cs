using CartService.Domain.Models;
using System.Text.Json;
using Confluent.Kafka;
using UserService.Domain.Events;
using CartService.Domain.Repositories;

namespace CartService.Kafka;

public class KafkaUserEventsConsumer : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<KafkaUserEventsConsumer> _logger;

    public KafkaUserEventsConsumer(IServiceProvider services, ILogger<KafkaUserEventsConsumer> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "cart-service",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
        {
            consumer.Subscribe("user-events");
            
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(stoppingToken);
                    var message = JsonSerializer.Deserialize<UserLoggedEvent>(cr.Message.Value);

                    using var scope = _services.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<CartDataContext>();

                    context.Carts.Add(new Cart { UserId = message.UserId });
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka consumer error");
            }
        }
    }
}
