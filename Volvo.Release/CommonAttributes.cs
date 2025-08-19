using System.Collections.Generic;

namespace Volvo.Release
{
    public class CommonAttributes
    {
        public CommonAttributes(string ASIN, string UPC,string SKU)
        {
            ExternalProductIdentifier = ASIN;
            ExternalProductId = ASIN;
            PhotoItemStoreWupc = UPC;
            PhotoAccessoryItemSku = SKU;
        }
        // ===== 基础信息 =====
        /// 品牌名
        public string BrandName { get; set; } = "Molenia";

        /// 运输重量（磅）
        public decimal ShippingWeightLbs { get; set; } = 0.11m;

        /// 原产地（实质性转变发生地）
        public string CountryOfOriginSubstantialTransformation { get; set; } = "China";

        /// 物流中心ID
        public string FulfillmentCenterId { get; set; } = "860342303840428033";

        /// 库存数量
        public int InventoryQuantity { get; set; } = 15;

        /// 每包数量（注意：耳饰可能为 2）
        public int CountPerPack { get; set; } = 1;

        /// 总数量（注意：耳饰可能为 2）
        public int TotalCount { get; set; } = 1;

        /// 多件装数量
        public int MultipackQuantity { get; set; } = 1;

        /// 是否需要 Prop 65 警告（你原表为 “No”，此处用 bool 表达）
        public bool IsProp65WarningRequired { get; set; } = false;

        /// 年龄组
        public string AgeGroup { get; set; } = "Adult";

        /// 新旧成色
        public string Condition { get; set; } = "New";

        /// 珠宝类型（精工/时尚）
        public string FineOrFashion { get; set; } = "Fine";

        /// 性别
        public string Gender { get; set; } = "Female";

        /// 是否有书面质保（表中 “No”）
        public bool HasWrittenWarranty { get; set; } = false;

        /// 小零件警告代码
        public string SmallPartsWarningCode { get; set; } = "0 - No warning applicable";

        /// 字母数字字符（用途依业务约定）
        public string AlphanumericCharacter { get; set; } = "1";

        /// 品牌授权
        public string BrandLicense { get; set; } = "Molenia";

        /// 加州 Prop 65 警告文本
        public string CaliforniaProp65WarningText { get; set; } = "No";

        /// 认证机构
        public string CertifyingAgent { get; set; } = "GRA";

        /// 角色（如 Women）
        public string Character { get; set; } = "Women";

        /// 角色组（如 Female）
        public string CharacterGroup { get; set; } = "Female";

        /// 系列
        public string Collection { get; set; } = "Jewelry Series";

        /// 设计师
        public string Designer { get; set; } = "Molenia";

        // ===== 钻石 / 宝石参数 =====
        public string DiamondClarity { get; set; } = "VVS1";
        public string DiamondColor { get; set; } = "D";
        public string DiamondCut { get; set; } = "Brilliant Cut";
        public string DiamondCutGrade { get; set; } = "Excellent";

        /// 镶框颜色配置
        public string FrameColorConfiguration { get; set; } = "Gold";

        /// 前端照片合作渠道
        public string FrontEndPhotoPartner { get; set; } = "MobileWeb Print to Retail";

        /// 宝石特性类型
        public string GemstoneCharacteristicType { get; set; } = "Inclusion";

        /// 宝石净度
        public string GemstoneClarity { get; set; } = "VVS";

        /// 宝石颜色
        public string GemstoneColor { get; set; } = "Clear";

        /// 宝石切工
        public string GemstoneCut { get; set; } = "Brilliant Cut";

        /// 铭文（如 S925）
        public string Inscription { get; set; } = "S925";

        /// 主要石重量（克拉）——数值；原表“默认不填”
        public decimal? PrimaryStoneWeightCaratsMeasure { get; set; } = null;

        /// 主要石重量单位（建议：ct）
        public string? PrimaryStoneWeightCaratsUnit { get; set; } = null;

        /// 宝石深度百分比（数值）
        public decimal StoneDepthPercentageMeasure { get; set; } = 60m;

        /// 宝石深度百分比单位
        public string StoneDepthPercentageUnit { get; set; } = "%";

        /// 宝石荧光
        public string StoneFluorescence { get; set; } = "Strong";

        /// 宝石抛光
        public string StonePolish { get; set; } = "Excellent";

        /// 宝石处理方式
        public string StoneTreatment { get; set; } = "Heat-Treated";

        /// 总宝石重量（数值）
        public decimal TotalGemstoneWeightMeasure { get; set; } = 4m;

        /// 总宝石重量单位
        public string TotalGemstoneWeightUnit { get; set; } = "ct";

        /// 总石重（克拉）（数值）
        public decimal TotalStoneWeightCaratsMeasure { get; set; } = 4m;

        /// 总石重单位
        public string TotalStoneWeightCaratsUnit { get; set; } = "ct";

        /// 石头生成方式
        public string StoneCreationMethod { get; set; } = "Simulated";

        /// 尖底（Culet）
        public string StoneCulet { get; set; } = "Small";

        // ===== 珍珠相关 =====
        /// 每颗珍珠尺寸（数值）
        public decimal SizePerPearlMeasure { get; set; } = 4m;

        /// 每颗珍珠尺寸单位
        public string SizePerPearlUnit { get; set; } = "mm";

        /// 珍珠类型
        public string PearlType { get; set; } = "Cultured";

        /// 珍珠一致性
        public string PearlUniformity { get; set; } = "Excellent";

        /// 珍珠珠层厚度（数值）
        public decimal PearlNacreThicknessMeasure { get; set; } = 3m;

        /// 珍珠珠层厚度单位
        public string PearlNacreThicknessUnit { get; set; } = "mm";

        /// 珍珠形状（根据你给的取值推断）
        public string PearlShape { get; set; } = "Round";

        /// 珍珠穿线方式
        public string PearlStringing { get; set; } = "Knotted On Silk";

        /// 珍珠表面质量/净度
        public string PearlSurfaceQuality { get; set; } = "Clean";

        /// （可选）表面工艺/效果
        public string Finish { get; set; } = "Matte";

        /// （可选）颜色（若与上游 Color 字段不同可保留）
        public string Color { get; set; } = "White";

        /// 珍珠光泽
        public string PearlLuster { get; set; } = "High Luster";

        // ===== 适用场景（多选） =====
        public string JewelryOccasion { get; set; } = "Anniversary Bridal Christmas Graduation Prom Everyday Birthday Cocktail Party Mother's Day Engagement Wedding Father's Day Valentine's Day";

        // ===== 关系人（多选；原表注释“选择该类型”，默认留空） =====
        public string PersonalRelationship { get; set; } = "Baby Boy Baby Girl Boy Boyfriend Bridal Party Bride Bride and Groom Bridesmaid Brother Couple Daughter Family Father Friend Girl Girl His and Hers Husband Mother Mother-Daughter Parents Pet Sister Son Spouse Wife";

        // ===== 生产厂商 / 包装 / 编号 =====
        /// 生产厂商名
        public string ManufacturerName { get; set; } = "Molenia";

        /// 厂商零件号
        public string ManufacturerPartNumber { get; set; } = "1";

        /// 零售包装
        public string RetailPackaging { get; set; } = "Gift Set";

        // ===== 图片/照片相关 =====
        /// 照片配件项 SKU（需要用你的 SKU 回填）
        public string PhotoAccessoryItemSku { get; set; } = string.Empty;

        /// 照片配置属性名
        public string PhotoConfigurationAttributeNames { get; set; } = "photoPaperFinishConfiguration";

        /// 照片门店 WUPC（需要用你的 UPC 回填）
        public string PhotoItemStoreWupc { get; set; } = string.Empty;

        /// 照片订单数量阶梯
        public int PhotoOrderQuantityTier { get; set; } = 1;

        // ===== 外部标识 =====
        /// 外部产品标识类型（默认选 ASIN）
        public string ExternalProductIdentifierIdType { get; set; } = "ASIN";

        /// 外部产品标识ID（用你的 ASIN 回填）
        public string ExternalProductIdentifier { get; set; } = string.Empty;
        public string ExternalProductId { get; set; }= string.Empty;
    }

}
