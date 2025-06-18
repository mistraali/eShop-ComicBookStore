using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using ProductService.Application.Events;

namespace ProductService.Application.Kafka.Producers;

public class ProductKafkaProducer
{
    private readonly IProducer<Null, string> _producer;
    private const string Topic = "product-exists-response";

    public ProductKafkaProducer(string bootstrapServers)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task SendProductExistsResponseAsync(ProductExistsEvent message)
    {
        var json = JsonSerializer.Serialize(message);
        var kafkaMessage = new Message<Null, string> { Value = json };

        await _producer.ProduceAsync(Topic, kafkaMessage);
    }
}
