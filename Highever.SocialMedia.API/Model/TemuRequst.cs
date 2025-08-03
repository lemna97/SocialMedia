namespace Highever.SocialMedia.API.Model
{
    #region 【已发布站点】核价
    /// <summary>
    /// 【已发布站点】核价结果
    /// </summary>
    public class TemuModel
    {
        public string userName { get; set; }
        public string mallId { get; set; }
        public string mallName { get; set; }
        public long productId { get; set; }
        public long skcId { get; set; }
        public string extCode { get; set; }
        public decimal supplierPrice { get; set; }
        public bool isscope { get; set; }
        public decimal freeShippingPrice_us { get; set; }
        public decimal freeShippingPrice_ca { get; set; }
        public decimal _rate { get; set; }
        public string profit_rate { get; set; }
        public int stockQuantity { get; set; }
        /// <summary>
        /// 活动折扣
        /// </summary>
        public decimal? activityDiscount { get; set; }
        /// <summary>
        /// 折扣后的价格
        /// </summary>
        public decimal? discountPrice { get; set; }
        /// <summary>
        /// 折扣后的利润
        /// </summary>
        public decimal? discountProfitRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? profit_discountProfitRate { get; set; }
    }
    public class SearchForSemiSupplierModel
    {
        /// <summary>
        /// 店铺ID
        /// </summary>
        public long mallId { get; set; }
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string mallName { get; set; }
        /// <summary>
        /// 折扣
        /// </summary>
        public decimal? activityDiscount { get; set; }
        public List<SearchForSemiSupplierDetailModel> data { get; set; }
    }
    public class SearchForSemiSupplierDetailModel
    {
        /// <summary>
        /// supplierId
        /// </summary>
        public long supplierId { get; set; }
        /// <summary>
        /// 申报价格
        /// </summary>
        public string supplierPrice { get; set; }
        /// <summary>
        /// 币种
        /// </summary>
        public string supplierPriceCurrencyType { get; set; }
        /// <summary>
        /// 产品ID
        /// </summary>
        public long productId { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string productName { get; set; }
        /// <summary>
        /// goodsId
        /// </summary>
        public long goodsId { get; set; }
        public List<SearchForSemiSupplierDetailModel_SkuList> skcList { get; set; }
    }
    public class SearchForSemiSupplierDetailModel_SkuList
    {
        /// <summary>
        /// skcId
        /// </summary>
        public long skcId { get; set; }
        /// <summary>
        /// 货号
        /// </summary>
        public string extCode { get; set; }
        /// <summary>
        /// 申报价格
        /// </summary>
        public string supplierPrice { get; set; }
        /// <summary>
        /// 币种
        /// </summary>
        public string supplierPriceCurrencyType { get; set; }
    }
    #endregion

    #region 【价格待确认】获取建议申报价格
    /// <summary>
    /// 
    /// </summary>
    public class SuggestSupplyPriceRequest
    {
        /// <summary>
        /// 申报次数
        /// </summary>
        public int declareNumber { get; set; }
        /// <summary>
        /// 货号
        /// </summary>
        public string productSkcExtCode { get; set; }
        /// <summary>
        /// priceOrderId
        /// </summary>
        public long priceOrderId { get; set; }
        /// <summary>
        /// 产品ID
        /// </summary>
        public long productId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<SuggestSupplyPriceRequestDetail> skuList { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class SuggestSupplyPriceRequestDetail
    {
        /// <summary>
        /// SKUId
        /// </summary>
        public long productSkuId { get; set; }

        /// <summary>
        /// 参考申报价格
        /// </summary>
        public long? suggestSupplyPrice { get; set; }
        /// <summary>
        /// 币种
        /// </summary>
        public string suggestPriceCurrency { get; set; }
        /// <summary>
        /// 汇率
        /// </summary>
        public decimal? ratio { get; set; }
        /// <summary>
        /// 当前申报价格
        /// </summary>
        public long? priceBeforeExchange { get; set; }
    }
    /// <summary>
    ///  反价操作
    /// </summary>
    public class SuggestSupplyPriceSave
    {
        /// <summary>
        /// 
        /// </summary>
        public long priceOrderId { get; set; }
        /// <summary>
        ///  1:同意，2：返价。3：拒绝
        /// </summary>
        public int supplierResult { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<SuggestSupplyPriceSave_Detail> items { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class SuggestSupplyPriceSave_Detail
    {
        /// <summary>
        /// productSkuId
        /// </summary>
        public long productSkuId { get; set; }
        /// <summary>
        /// 反价价格
        /// </summary>
        public int? price { get; set; } = null;
    }
    /// <summary>
    /// 
    /// </summary>
    public class SuggestSupplyPriceResult_Excel
    {
        /// <summary>
        /// sku
        /// </summary>
        public long sku { get; set; }
        /// <summary>
        ///  货号
        /// </summary>
        public string productSkcExtCode { get; set; }
        /// <summary>
        /// 参考申报价格
        /// </summary>
        public string suggestSupplyPrice { get; set; }
        /// <summary>
        /// 申报价格
        /// </summary>
        public decimal priceBeforeExchange { get; set; }
        /// <summary>
        /// 成本价
        /// </summary>
        public decimal free_shipping_price { get; set; }
        /// <summary>
        ///  1:同意，2：返价。3：拒绝
        /// </summary>
        public int operation { get; set; }
        /// <summary>
        ///  价格 （分）
        /// </summary>
        public string price { get; set; }
        /// <summary>
        ///  价格
        /// </summary>
        public string price2 { get; set; }
        /// <summary>
        ///  利润
        /// </summary>
        public string profitRate { get; set; }
        /// <summary>
        ///  核价次数
        /// </summary>
        public int coreNumber { get; set; }
    }
    #endregion
}
