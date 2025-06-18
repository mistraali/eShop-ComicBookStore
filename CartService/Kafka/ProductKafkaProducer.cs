using Confluent.Kafka;
using System.Text.Json;
using CartService.Domain.Events;


namespace CartService.Kafka;

public class ProductKafkaProducer
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topic = "cart-events";

    public ProductKafkaProducer(string bootstrapServers)
    {
        var config = new ProducerConfig { BootstrapServers = bootstrapServers };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task PublishProductRequestedAsync(KafkaProductRequestedEvent @event)
    {
        var json = JsonSerializer.Serialize(@event);
        Console.WriteLine($"[KafkaProducer] Sending event: {json}");

        await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = json });

        Console.WriteLine("[KafkaProducer] Event sent.");
    }
}
