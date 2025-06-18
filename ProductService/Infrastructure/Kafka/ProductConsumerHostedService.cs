namespace ProductService.Infrastructure.Kafka;
using Microsoft.Extensions.Hosting;
using ProductService.Application.Kafka.Consumers;
using ProductService.Application.Kafka;
using Microsoft.Extensions.Options;

public class ProductConsumerHostedService : IHostedService
{
    private readonly CheckIfProductExistsConsumer _consumer;
    private readonly KafkaSettings _settings;
    private CancellationTokenSource _cts;

    public ProductConsumerHostedService(CheckIfProductExistsConsumer consumer, IOptions<KafkaSettings> options)
    {
        _consumer = consumer;
        _settings = options.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = new CancellationTokenSource();
        _consumer.StartConsuming(_cts.Token);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        return Task.CompletedTask;
    }
}
