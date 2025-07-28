namespace Highever.SocialMedia.Application.Contracts
{
    public class CreateDistributionProductDto
    {
        public string Sku { get; set; }
        public string Platform { get; set; }
        public decimal SelfPickupPrice { get; set; }
        public decimal FreeShippingPrice { get; set; }
        public int StockQuantity { get; set; }
    }
    public class DistributionProductDto
    {
        public int Id { get; set; }
        public string Sku { get; set; }
        public string Platform { get; set; }
        public decimal SelfPickupPrice { get; set; }
        public decimal FreeShippingPrice { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
