using SqlSugar;

namespace Highever.SocialMedia.Domain
{
    /// <summary>
    /// 货源信息
    /// </summary>
    [SugarTable("distribution_products")]
    public class DistributionProducts
    {
        public DistributionProducts() { }

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        [SugarColumn(ColumnName = "sku", Length = 255, IsNullable = false)]
        public string Sku { get; set; }
        /// <summary>
        /// 平台
        /// </summary>

        [SugarColumn(ColumnName = "platform", Length = 255, IsNullable = false)]
        public string Platform { get; set; }
        /// <summary>
        /// 自提价格
        /// </summary>

        [SugarColumn(ColumnName = "self_pickup_price", DecimalDigits = 2, IsNullable = false)]
        public decimal SelfPickupPrice { get; set; }
        /// <summary>
        /// 包邮金额
        /// </summary>

        [SugarColumn(ColumnName = "free_shipping_price", DecimalDigits = 2, IsNullable = false)]
        public decimal FreeShippingPrice { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        [SugarColumn(ColumnName = "stock_quantity", IsNullable = false)]
        public int StockQuantity { get; set; }

        [SugarColumn(ColumnName = "updated_at", IsNullable = false, IsOnlyIgnoreInsert = true)]
        public DateTime UpdatedAt { get; set; }

        [SugarColumn(ColumnName = "created_at", IsNullable = false, IsOnlyIgnoreUpdate = true)]
        public DateTime CreatedAt { get; set; }


        /// <summary>
        /// 构造函数，用于强制初始化必要属性
        /// </summary>
        public DistributionProducts(
            string sku,
            string platform,
            decimal selfPickupPrice,
            decimal freeShippingPrice,
            int stockQuantity)
        {
            Sku = !string.IsNullOrWhiteSpace(sku) ? sku : throw new ArgumentException("SKU 不能为空");
            Platform = !string.IsNullOrWhiteSpace(platform) ? platform : throw new ArgumentException("平台不能为空");
            SelfPickupPrice = selfPickupPrice >= 0 ? selfPickupPrice : throw new ArgumentException("自提价格不能为负数");
            FreeShippingPrice = freeShippingPrice >= 0 ? freeShippingPrice : throw new ArgumentException("包邮金额不能为负数");
            StockQuantity = stockQuantity >= 0 ? stockQuantity : throw new ArgumentException("库存数量不能为负数");

            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// 更新库存
        /// </summary>
        public void UpdateStock(int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("库存数量不能为负数");

            StockQuantity = quantity;
            UpdatedAt = DateTime.Now; // 每次修改时更新更新时间
        }
    }
}
