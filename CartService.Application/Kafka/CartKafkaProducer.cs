using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CartService.Application.Events;
using Confluent.Kafka;

namespace CartService.Application.Kafka;

public class CartKafkaProducer
{
    private readonly IProducer<Null, string> _producer;
    private const string Topic = "check-product-exists";

    public CartKafkaProducer(string bootstrapServers)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task SendCheckProductExistsAsync(CheckIfProductExistsEvent message)
    {
        var json = JsonSerializer.Serialize(message);
        await _producer.ProduceAsync(Topic, new Message<Null, string> { Value = json });
    }
}
