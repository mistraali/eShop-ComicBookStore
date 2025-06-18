using CartService.Kafka;

namespace CartService.Infrastructure.Kafka;

public class CartConsumerHostedService : IHostedService
{
    private readonly ProductExistsResponseConsumer _consumer;
    private CancellationTokenSource _cts;

    public CartConsumerHostedService(ProductExistsResponseConsumer consumer)
    {
        _consumer = consumer;
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
