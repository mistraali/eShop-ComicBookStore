namespace ProductService.Kafka;

public class KafkaProductResponseEvent
{
    public int RequestedId { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; }
    public bool Exists { get; set; }
}
