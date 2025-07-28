namespace Highever.SocialMedia.API.Model
{
    public class ProductPropertyRequest
    {
        public long productId { get; set; }
        public long productCreateTime { get; set; }
        public string productName { get; set; }
        public string productPicture { get; set; }
        public List<RequiredNotFilledInProperties> requiredNotFilledInProperties { get; set; }
        public List<ProductPicture> properties { get; set; }
        public List<Property>? productPropertyList { get; set; }
    }

    public class Property
    {
        public int templatePid { get; set; }
        public int pid { get; set; }
        public int refPid { get; set; }
        public string propName { get; set; }
        public int vid { get; set; }
        public string propValue { get; set; }
        public string valueUnit { get; set; }
        public string valueExtendInfo { get; set; }
        public string numberInputValue { get; set; }
    }

    public class RequiredNotFilledInProperties
    {
        public int refPid { get; set; }
        public string propName { get; set; }
    }

    public class ValueItem // 用于表示 values 数组中的值对象
    {
        public int Vid { get; set; } // 值 ID
        public string Value { get; set; } // 值内容
    }
    public class ProductPicture
    {
        /// <summary>
        /// 控件类型（1：下拉选项）
        /// </summary>
        public int controlType { get; set; }
        public int refPid { get; set; }
        public string? numberInputTitle { get; set; }
        public int? templatePid { get; set; }
        public int? parentTemplatePid { get; set; }
        public string? valueExtendInfo { get; set; }
        public List<string>? valueUnit { get; set; }
        public int pid { get; set; }
        public string name { get; set; }
        public bool required { get; set; }
        /// <summary>
        /// 可以选择几个值
        /// </summary>
        public int chooseMaxNum { get; set; }
        public List<ParentChildMapping>? templatePropertyValueParentList { get; set; } 
        public List<Values>? values { get; set; }

    }
    public class ParentChildMapping
    {
        public List<int> vidList { get; set; } // 子项值的 ID 列表
        public List<int> parentVidList { get; set; } // 父级值的 ID 列表
    }
    public class Values
    {
        public int vid { get; set; }
        public string value { get; set; } 
        public int refPid { get; set; }
    }

    public class GptResponses
    { 
        public int refPid { get; set; }
        public int vid { get; set; }
        public string value { get; set; }
    }

    public class ProductPropertyResponse
    {
        public string valueUnit { get; set; }
        public string propValue { get; set; }
        public string propName { get; set; }
        public int refPid { get; set; }
        public int vid { get; set; }
        public int controlType { get; set; }
        public int pid { get; set; }
        public int templatePid { get; set; }
        public string valueExtendInfo { get; set; }
    }


    public class EditRequst
    {
        public int editScene { get; set; }
        public long productId { get; set; }
        public List<object> productProperties { get; set; }
    }
    public class EditCallBack
    {
        public bool success { get; set; }
        public long productId { get; set; }
        public string? resultText { get; set; }
    }
}
