namespace Donatyk2.Server.Dto;

public class AddCartItemRequest
{
    public Guid LotId { get; set; }
    public int Quantity { get; set; }
}
