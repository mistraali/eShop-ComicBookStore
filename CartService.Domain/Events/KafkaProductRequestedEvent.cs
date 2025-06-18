namespace CartService.Domain.Events;

public class KafkaProductRequestedEvent
{
    public int RequestedId { get; set; }
    public int ProductId { get; set; }
}
