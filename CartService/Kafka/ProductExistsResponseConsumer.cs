using CartService.Application.Events;
using CartService.Application.Kafka.Utils;
using Confluent.Kafka;
using System.Linq.Expressions;
using System.Text.Json;

namespace CartService.Kafka;

public class ProductExistsResponseConsumer
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly ProductCheckResponseAwaiter _awaiter;

    public ProductExistsResponseConsumer(ProductCheckResponseAwaiter awaiter, string bootstrapServers)
    {
        _awaiter = awaiter;

        var config = new ConsumerConfig
        {
            GroupId = "cart-service-response-consumer",
            BootstrapServers = bootstrapServers,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        _consumer.Subscribe("product-exists-response");
    }

    public void StartConsuming(CancellationToken cancellationToken)
    {
        Task.Run(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try 
                {
                var result = _consumer.Consume(cancellationToken);

                Console.WriteLine($"Odebrano wiadomość z topicu: {result.Topic}, offset: {result.Offset}");

                var message = JsonSerializer.Deserialize<ProductExistsEvent>(result.Message.Value);

                Console.WriteLine($"Odpowiedź z ProductService: ProductId = {message.ProductId}, Exists = {message.Exists}");

                _awaiter.SetResponse(message.ProductId, message.Exists);
                Console.WriteLine($"Odpowiedź dla produktu {message.ProductId}: exists = {message.Exists}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd podczas przetwarzania odpowiedzi: {ex.Message}");
                }
            }
        }, cancellationToken);
    }
}
