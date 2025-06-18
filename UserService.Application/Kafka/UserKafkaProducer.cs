using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UserService.Domain.Events;
using Confluent.Kafka;

namespace UserService.Application.Kafka;

public class UserKafkaProducer
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topic = "user-events";

    public UserKafkaProducer(string bootstrapServers)
    {
        var config = new ProducerConfig { BootstrapServers = bootstrapServers };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public virtual async Task PublishUserLoggedAsync(UserLoggedEvent @event)
    {
        var json = JsonSerializer.Serialize(@event);
        Console.WriteLine($"[KafkaProducer] Sending event: {json}");

        await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = json });

        Console.WriteLine("[KafkaProducer] Event sent.");
    }
}
