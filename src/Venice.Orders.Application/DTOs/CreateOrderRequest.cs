namespace Venice.Orders.Application.DTOs;

public class CreateOrderRequest
{
    public Guid ClientId { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();

    public class CreateOrderItemDto
    {
        public string Product { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
