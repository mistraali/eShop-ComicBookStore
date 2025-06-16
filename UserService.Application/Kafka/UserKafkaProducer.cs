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
    private readonly string _topic = "user.logged";

    public UserKafkaProducer(string bootstrapServers)
    {
        var config = new ProducerConfig { BootstrapServers = bootstrapServers };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task PublishUserLoggedAsync(UserLoggedEvent @event)
    {
        var json = JsonSerializer.Serialize(@event);
        await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = json });
    }
}
