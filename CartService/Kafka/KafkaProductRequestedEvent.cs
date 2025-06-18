namespace CartService.Kafka;

public class KafkaProductRequestedEvent
{
    public int RequestedId { get; set; }
    public int ProductId { get; set; }
}
