using Azure;
using Highever.Amazon.Advertising.Common;
using Highever.SocialMedia.API.Model;
using Highever.SocialMedia.Application.Contracts;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.OpenAI;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json; 
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Text;

namespace Highever.SocialMedia.Admin.Controllers
{
    /// <summary>
    /// Temu 平台
    /// </summary>
    [EnableCors("AllowSpecificOrigins")] // 应用指定的 CORS 策略
    [ApiController]
    [ApiGroup(SwaggerApiGroup.FullSocialMedia)]
    [Route("Temu")]
    public class TemuController : Controller
    {
        IDistributionProductsAppService _distributionProductsAppService => _serviceProvider.GetRequiredService<IDistributionProductsAppService>();
        IChatGPTService _chatGPTService => _serviceProvider.GetRequiredService<IChatGPTService>();
        IProductPropertyRequestRecordService _productPropertyRequestRecordService => _serviceProvider.GetRequiredService<IProductPropertyRequestRecordService>();
        IHttpClientFactory _httpClientFactory => _serviceProvider.GetRequiredService<IHttpClientFactory>();
        INLogger _logger => _serviceProvider.GetRequiredService<INLogger>();

        /// <summary>
        /// 
        /// </summary>
        public readonly IServiceProvider _serviceProvider;
        /// <summary>
        /// 
        /// </summary>
        public TemuController(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }


        [HttpGet("Index")]
        [Obsolete]
        public async Task<IActionResult> Index(int id)
        {
            var result = await _distributionProductsAppService.GetByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// 获取【已发布站点】申报价表格
        /// </summary> 
        /// <param name="SearchForSemiSupplierModel"></param> 
        /// <returns></returns>
        [HttpPost("GetTemuSearchForSemiSupplierExcel")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TemuModel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> GetTemuSearchForSemiSupplierExcel([FromBody] List<SearchForSemiSupplierModel> SearchForSemiSupplierModel)
        {
            ConcurrentBag<TemuModel> TemuModels = new ConcurrentBag<TemuModel>();
            if (SearchForSemiSupplierModel == null || SearchForSemiSupplierModel.Count == 0)
            {
                return BadRequest("已发布站点数据不能为空");
            }
            try
            {
                var productsList = await _distributionProductsAppService.GetQueryListAsync(t => true);
                #region 主逻辑
                try
                {
                    if (productsList != null && productsList.Count > 0 && SearchForSemiSupplierModel != null && SearchForSemiSupplierModel.Count > 0)
                    {
                        foreach (var item in SearchForSemiSupplierModel)
                        {
                            //每个店铺5个线程跑
                            var parallelOptions = new ParallelOptions
                            {
                                MaxDegreeOfParallelism = 5
                            };
                            //decimal? _activityDiscount = item.activityDiscount;
                            decimal? _activityDiscount = 0.85m;
                            //集合
                            Parallel.ForEach(item.data, parallelOptions, item_detail =>
                            {
                                //货号
                                string extCode = string.Empty;
                                long skcId = 0;
                                //申报价
                                decimal _supplierPrice = 0m;
                                bool _isscope = false;
                                decimal _profit_rate = 0m;
                                decimal _freeShippingPrice_us = 0m;
                                decimal _freeShippingPrice_ca = 0m;
                                int _stockQuantity = 0;
                                string supplierPriceCurrencyType = string.Empty;
                                if (item_detail != null && item_detail?.skcList != null)
                                {
                                    var skcList = item_detail.skcList?.FirstOrDefault();
                                    if (skcList != null)
                                    {
                                        extCode = skcList.extCode;
                                        skcId = skcList.skcId;
                                        supplierPriceCurrencyType = skcList.supplierPriceCurrencyType;
                                    }
                                    var temp_supplierPrice = item_detail.supplierPrice;
                                    if (!string.IsNullOrEmpty(temp_supplierPrice))
                                    {
                                        if (temp_supplierPrice.Contains("~"))
                                        {
                                            _isscope = true;
                                            var _supplierPriceSplit = temp_supplierPrice.Split("~")[1];
                                            temp_supplierPrice = StringHelper.ExtractNumber<string>(_supplierPriceSplit.Trim());
                                            _supplierPrice = temp_supplierPrice.ToDecimal() ?? 0;
                                        }
                                        else
                                        {

                                            temp_supplierPrice = StringHelper.ExtractNumber<string>(temp_supplierPrice.Trim());
                                            _supplierPrice = temp_supplierPrice.ToDecimal() ?? 0;
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(extCode))
                                    {
                                        Func<string, string> removeLeadingZero = s => s.StartsWith("0") ? s.Substring(1) : s;
                                        extCode = removeLeadingZero(extCode);
                                        if (extCode.Contains("+"))
                                        {
                                            var _extCode = extCode.Split("+")[1];
                                            extCode = _extCode.ToString().Trim();
                                        }
                                        else
                                        {
                                            extCode = extCode.Trim();
                                        }
                                        var productsList_temp = productsList.Where(t => t.Sku == extCode).FirstOrDefault();
                                        if (productsList_temp != null)
                                        {
                                            _freeShippingPrice_us = productsList_temp?.FreeShippingPrice ?? 0;
                                            _stockQuantity = productsList_temp?.StockQuantity ?? 0;
                                            if (_freeShippingPrice_us > 0)
                                            {
                                                //币种
                                                if (supplierPriceCurrencyType == "CNY")
                                                {
                                                    _freeShippingPrice_ca = Math.Round(7.3m * _freeShippingPrice_us, 2);
                                                }
                                                else
                                                {
                                                    _freeShippingPrice_ca = _freeShippingPrice_us;
                                                }
                                            }
                                            if (_supplierPrice > 0 && _freeShippingPrice_ca > 0)
                                            {
                                                _profit_rate = Math.Round(Math.Round(_supplierPrice - _freeShippingPrice_ca, 2) / _supplierPrice, 2) * 100;
                                            }
                                        }
                                    }
                                }
                                decimal _discountPrice = 0m;
                                decimal _discountProfitRate = 0m;
                                if (_supplierPrice > 0)
                                {
                                    _discountPrice = Math.Round(_supplierPrice * (_activityDiscount.Value), 2);
                                }
                                if (_discountPrice > 0 && _freeShippingPrice_ca > 0)
                                {
                                    _discountProfitRate = Math.Round(Math.Round(_discountPrice - _freeShippingPrice_ca, 2) / _discountPrice, 2) * 100;
                                }
                                TemuModels.Add(new TemuModel()
                                {
                                    mallId = item.mallId.ToString(),
                                    extCode = extCode,
                                    productId = item_detail.productId,
                                    skcId = skcId,
                                    freeShippingPrice_us = _freeShippingPrice_us,
                                    freeShippingPrice_ca = _freeShippingPrice_ca,
                                    mallName = item.mallName,
                                    stockQuantity = _stockQuantity,
                                    supplierPrice = _supplierPrice,
                                    isscope = _isscope,
                                    _rate = _profit_rate,
                                    profit_rate = $"{_profit_rate}%",
                                    activityDiscount = _activityDiscount,
                                    discountPrice = _discountPrice,
                                    discountProfitRate = _discountProfitRate,
                                    profit_discountProfitRate = $"{_discountProfitRate}%",
                                });
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                #endregion
                byte[] excelBytes = new byte[0];
                if (TemuModels?.Count() > 0)
                {
                    var result = TemuModels.OrderByDescending(t => t.discountProfitRate).AsEnumerable();
                    Dictionary<string, string> headers = new Dictionary<string, string>
                    {
                        { "店铺ID", "mallId" },
                        { "货号", "extCode" },
                        { "店铺名称", "mallName" },
                        { "SPU", "productId" },
                        { "SKU", "skcId" },
                        { "仓库数量", "stockQuantity" },
                        { "货源包邮价格（US）", "freeShippingPrice_us" },
                        { "货源包邮价格（CN）", "freeShippingPrice_ca" },
                        { "申报价格", "supplierPrice" },
                        { "是否是最大申报价格", "isscope" },
                        { "利润率", "profit_rate" },
                        { "折扣", "activityDiscount" },
                        { "折扣后的价格（申报价格*折扣）", "discountPrice" },
                        { "折扣后的利润", "profit_discountProfitRate" }
                    };
                    excelBytes = ExcelHelper.ExportDataToExcel(result, headers, "TemuSearchForSemiSupplier", "TemuSearchForSemiSupplier.xlsx");
                }
                return File(
                    excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"{DateTime.Now.ToString("yyyyMMdd")}已发布站点核价结果.xlsx"
                );
            }
            catch (JsonException ex)
            {
                return BadRequest($"JSON解析错误: {ex.Message}");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"服务器错误: {ex.Message}");
            }
        }


        /// <summary>
        ///  获取【价格待确认】参考申报价格
        /// </summary>
        /// <param name="suggestSupplyPriceListModels"></param> 
        /// <returns></returns>
        [HttpPost("GetTemuFetchSuggestSupplyPriceSave")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SuggestSupplyPriceSave), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> GetTemuFetchSuggestSupplyPriceSave([FromBody] List<SuggestSupplyPriceRequest> suggestSupplyPriceListModels)
        {
            if (suggestSupplyPriceListModels == null || suggestSupplyPriceListModels.Count == 0)
            {
                return BadRequest("建议申报数据不能为空！");
            }
            try
            {
                var productsList = await _distributionProductsAppService.GetQueryListAsync(t => true);
                ConcurrentBag<SuggestSupplyPriceSave> temp_Results = new ConcurrentBag<SuggestSupplyPriceSave>();
                #region 主逻辑
                try
                {
                    if (productsList != null && productsList.Count > 0 && suggestSupplyPriceListModels != null && suggestSupplyPriceListModels.Count > 0)
                    {
                        productsList.ForEach(item =>
                        {
                            //saiying的特殊处理
                            if (!string.IsNullOrEmpty(item.Sku) && item.Sku.Length < 8 && !string.IsNullOrEmpty(item.Platform) && item.Platform.ToLower().Trim() == "saiying")
                            {
                                // 在 sku 前面补零，直到长度为 8
                                item.Sku = item.Sku.PadLeft(8, '0');
                            }
                        });
                        foreach (var item in suggestSupplyPriceListModels)
                        {
                            var extCode = item.productSkcExtCode;
                            long priceOrderId = item.priceOrderId;
                            var productId = item.productId;
                            //核价次数
                            int declareNumber = item.declareNumber;
                            //平台
                            var platform = "";
                            //利润率
                            decimal _profit_rate = 0m;
                            //成本价
                            decimal _freeShippingPrice_us = 0m;
                            decimal _freeShippingPrice_ca = 0m;
                            //库存
                            int _stockQuantity = 0;
                            if (!string.IsNullOrEmpty(item.productSkcExtCode))
                            {
                                Func<string, string> removeLeadingZero = s => s.StartsWith("0") ? s.Substring(1) : s;
                                item.productSkcExtCode = removeLeadingZero(item.productSkcExtCode);
                                if (item.productSkcExtCode.Contains("+"))
                                {
                                    var _tempSpilt = item.productSkcExtCode.Split("+");
                                    var _extCode = _tempSpilt[_tempSpilt.Count() - 1];
                                    extCode = _extCode.ToString().Trim();
                                }
                                else
                                {
                                    extCode = item.productSkcExtCode.Trim();
                                }
                                var productsList_temp = productsList.Where(t => t.Sku == extCode).FirstOrDefault();
                                if (productsList_temp != null)
                                {
                                    platform = productsList_temp.Platform.Trim();
                                    _freeShippingPrice_us = productsList_temp?.FreeShippingPrice ?? 0;
                                    _stockQuantity = productsList_temp?.StockQuantity ?? 0;
                                    #region 核价策略 
                                    var temp_SuggestSupplyPric = new SuggestSupplyPriceSave();
                                    temp_SuggestSupplyPric.priceOrderId = priceOrderId;
                                    //默认拒绝
                                    temp_SuggestSupplyPric.supplierResult = 3;
                                    temp_SuggestSupplyPric.items = new List<SuggestSupplyPriceSave_Detail>();
                                    if (item.skuList != null && item.skuList.Count > 0)
                                    {
                                        List<SuggestSupplyPriceSave_Detail> suggestSupplyPriceSaveDetailList = new List<SuggestSupplyPriceSave_Detail>();
                                        foreach (var skuItem in item.skuList)
                                        {
                                            var SuggestSupplyPriceSave_Detail = new SuggestSupplyPriceSave_Detail();
                                            if (_freeShippingPrice_us > 0)
                                            {
                                                //币种
                                                if (skuItem.suggestPriceCurrency == "CNY" && skuItem.ratio == null)
                                                {
                                                    _freeShippingPrice_ca = Math.Round(7.3m * _freeShippingPrice_us, 2);
                                                }
                                                else
                                                {
                                                    _freeShippingPrice_ca = _freeShippingPrice_us;
                                                }
                                            }
                                            SuggestSupplyPriceSave_Detail.price = null;
                                            long productSkuId = skuItem.productSkuId;
                                            //参考申报价
                                            decimal suggestSupplyPrice = skuItem.suggestSupplyPrice ?? 0m;
                                            if (suggestSupplyPrice > 0)
                                            {
                                                suggestSupplyPrice = Math.Round(suggestSupplyPrice / 100, 2);
                                            }
                                            //申报价
                                            decimal priceBeforeExchange = skuItem.priceBeforeExchange ?? 0m;
                                            if (priceBeforeExchange > 0)
                                            {
                                                priceBeforeExchange = Math.Round(priceBeforeExchange / 100, 2);
                                            }
                                            //算利润
                                            if (suggestSupplyPrice > 0 && _freeShippingPrice_ca > 0)
                                            {
                                                _profit_rate = Math.Round(Math.Round(suggestSupplyPrice - _freeShippingPrice_ca, 2) / suggestSupplyPrice, 2) * 100;
                                            }
                                            if (declareNumber > 3)
                                            {
                                                temp_SuggestSupplyPric.supplierResult = 3;
                                            }
                                            int? temp_price = null;
                                            //通过
                                            if (_profit_rate >= 18)
                                            {
                                                temp_SuggestSupplyPric.supplierResult = 1;
                                                temp_price = Convert.ToInt32(suggestSupplyPrice * 100);
                                            }
                                            else
                                            {
                                                //如果是第一次
                                                if (declareNumber == 1 && (platform != "giga" && !extCode.ToLower().Contains("b")))
                                                {
                                                    //反价格
                                                    temp_SuggestSupplyPric.supplierResult = 2;
                                                    temp_price = Convert.ToInt32((priceBeforeExchange - 1) * 100);
                                                }
                                            }
                                            SuggestSupplyPriceSave_Detail.price = temp_price;
                                            SuggestSupplyPriceSave_Detail.productSkuId = productSkuId;
                                            temp_SuggestSupplyPric.items.Add(SuggestSupplyPriceSave_Detail);
                                        }
                                    }
                                    temp_Results.Add(temp_SuggestSupplyPric);
                                    #endregion
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                #endregion  
                return this.Success(temp_Results);
            }
            catch (JsonException ex)
            {
                return BadRequest($"JSON解析错误: {ex.Message}");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"服务器错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 导出 【价格待确认】核价操作记录
        /// </summary>
        /// <param name="suggestSupplyPriceListModels"></param>
        /// <returns></returns>
        [HttpPost("TemuFetchSuggestSupplyPriceByExcel")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SuggestSupplyPriceResult_Excel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> TemuFetchSuggestSupplyPriceByExcel([FromBody] List<SuggestSupplyPriceRequest> suggestSupplyPriceListModels)
        {
            if (suggestSupplyPriceListModels == null || suggestSupplyPriceListModels.Count == 0)
            {
                return BadRequest("建议申报数据不能为空！");
            }
            try
            {

                ConcurrentBag<SuggestSupplyPriceSave> temu_Results = new ConcurrentBag<SuggestSupplyPriceSave>();
                ConcurrentBag<SuggestSupplyPriceResult_Excel> temp_excel_Results = new ConcurrentBag<SuggestSupplyPriceResult_Excel>();
                var productsList = await _distributionProductsAppService.GetQueryListAsync(t => true);
                // 读取TXT文件内容
                #region 主逻辑
                try
                {
                    if (productsList != null && productsList.Count > 0 && suggestSupplyPriceListModels != null && suggestSupplyPriceListModels.Count > 0)
                    {
                        productsList.ForEach(item =>
                        {
                            //saiying的特殊处理
                            if (!string.IsNullOrEmpty(item.Sku) && item.Sku.Length < 8 && !string.IsNullOrEmpty(item.Platform) && item.Platform.ToLower().Trim() == "saiying")
                            {
                                // 在 sku 前面补零，直到长度为 8
                                item.Sku = item.Sku.PadLeft(8, '0');
                            }
                        });
                        foreach (var item in suggestSupplyPriceListModels)
                        {
                            var extCode = item.productSkcExtCode;
                            long priceOrderId = item.priceOrderId;
                            var productId = item.productId;
                            //核价次数
                            int declareNumber = item.declareNumber;
                            //平台
                            var platform = "";
                            //利润率
                            decimal _profit_rate = 0m;
                            decimal _freeShippingPrice_us = 0m;
                            decimal _freeShippingPrice_ca = 0m;
                            //库存
                            int _stockQuantity = 0;
                            if (!string.IsNullOrEmpty(item.productSkcExtCode))
                            {
                                Func<string, string> removeLeadingZero = s => s.StartsWith("0") ? s.Substring(1) : s;
                                item.productSkcExtCode = removeLeadingZero(item.productSkcExtCode);
                                if (item.productSkcExtCode.Contains("+"))
                                {
                                    var _tempSpilt = item.productSkcExtCode.Split("+");
                                    var _extCode = _tempSpilt[_tempSpilt.Count() - 1];
                                    extCode = _extCode.ToString().Trim();
                                }
                                else
                                {
                                    extCode = item.productSkcExtCode.Trim();
                                }
                                var productsList_temp = productsList.Where(t => t.Sku == extCode).FirstOrDefault();
                                if (productsList_temp != null)
                                {
                                    platform = productsList_temp.Platform.Trim();
                                    _freeShippingPrice_us = productsList_temp?.FreeShippingPrice ?? 0;
                                    _stockQuantity = productsList_temp?.StockQuantity ?? 0;
                                    #region 核价策略 
                                    var temp_SuggestSupplyPric = new SuggestSupplyPriceSave();
                                    temp_SuggestSupplyPric.priceOrderId = priceOrderId;
                                    //默认拒绝
                                    temp_SuggestSupplyPric.supplierResult = 3;
                                    temp_SuggestSupplyPric.items = new List<SuggestSupplyPriceSave_Detail>();
                                    if (item.skuList != null && item.skuList.Count > 0)
                                    {
                                        List<SuggestSupplyPriceSave_Detail> SuggestSupplyPriceSave_Detail_Array = new List<SuggestSupplyPriceSave_Detail>();
                                        foreach (var skuItem in item.skuList)
                                        {
                                            var SuggestSupplyPriceSave_Detail = new SuggestSupplyPriceSave_Detail();
                                            if (_freeShippingPrice_us > 0)
                                            {
                                                //币种
                                                if (skuItem.suggestPriceCurrency == "CNY" && skuItem.ratio == null)
                                                {
                                                    _freeShippingPrice_ca = Math.Round(7.3m * _freeShippingPrice_us, 2);
                                                }
                                                else
                                                {
                                                    _freeShippingPrice_ca = _freeShippingPrice_us;
                                                }
                                            }
                                            SuggestSupplyPriceSave_Detail.price = null;
                                            long productSkuId = skuItem.productSkuId;
                                            //参考申报价
                                            decimal suggestSupplyPrice = skuItem.suggestSupplyPrice ?? 0m;
                                            if (suggestSupplyPrice > 0)
                                            {
                                                suggestSupplyPrice = suggestSupplyPrice / 100;
                                            }
                                            //申报价
                                            decimal priceBeforeExchange = skuItem.priceBeforeExchange ?? 0m;
                                            if (priceBeforeExchange > 0)
                                            {
                                                priceBeforeExchange = priceBeforeExchange / 100;
                                            }
                                            if (suggestSupplyPrice > 0 && _freeShippingPrice_ca > 0)
                                            {
                                                _profit_rate = Math.Round(Math.Round(suggestSupplyPrice - _freeShippingPrice_ca, 2) / suggestSupplyPrice, 2) * 100;
                                            }
                                            temp_SuggestSupplyPric.supplierResult = 3;
                                            int? temp_price = null;
                                            decimal? temp_price2 = null;
                                            //通过
                                            if (_profit_rate >= 18)
                                            {
                                                temp_SuggestSupplyPric.supplierResult = 1;
                                                temp_price = Convert.ToInt32(suggestSupplyPrice * 100);
                                                temp_price2 = suggestSupplyPrice * 100;
                                            }
                                            else
                                            {
                                                //如果是第一次
                                                if (declareNumber == 1 && (platform != "giga" && !extCode.ToLower().Contains("b")))
                                                {
                                                    //反价格
                                                    temp_SuggestSupplyPric.supplierResult = 2;
                                                    temp_price = Convert.ToInt32((priceBeforeExchange - 1) * 100);
                                                    temp_price2 = (priceBeforeExchange - 1) * 100;
                                                }
                                            }
                                            SuggestSupplyPriceSave_Detail.price = temp_price;
                                            SuggestSupplyPriceSave_Detail.productSkuId = productSkuId;
                                            temp_SuggestSupplyPric.items.Add(SuggestSupplyPriceSave_Detail);
                                            //导出的数据 
                                            temp_excel_Results.Add(new SuggestSupplyPriceResult_Excel()
                                            {
                                                coreNumber = declareNumber,
                                                free_shipping_price = _freeShippingPrice_ca,
                                                priceBeforeExchange = priceBeforeExchange,
                                                operation = temp_SuggestSupplyPric.supplierResult,
                                                sku = productSkuId,
                                                productSkcExtCode = extCode,
                                                price = temp_price != null ? temp_price.ToString() : string.Empty,
                                                price2 = temp_price2 != null ? (temp_price2 / 100).ToString() : string.Empty,
                                                profitRate = $"{_profit_rate}%",
                                                suggestSupplyPrice = suggestSupplyPrice.ToString(),
                                            });
                                        }
                                    }
                                    temu_Results.Add(temp_SuggestSupplyPric);
                                    #endregion
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
                #endregion 
                byte[] excelBytes = new byte[0];
                if (temp_excel_Results?.Count() > 0)
                {
                    var result = temp_excel_Results.OrderBy(t => t.operation).AsEnumerable();
                    Dictionary<string, string> headers = new Dictionary<string, string>
                    {
                        { "SKU", "sku" },
                        { "货号", "productSkcExtCode" },
                        { "成本价", "free_shipping_price" },
                        { "申报价格", "priceBeforeExchange" },
                        { "建议申报价", "suggestSupplyPrice" },
                        { "利润", "profitRate" },
                        { "核价次数", "coreNumber" },
                        { "操作(1:成功，2:反价，3:拒绝)", "operation" },
                        { "提交的核价1", "price" },
                        { "提交的核价2", "price2" }
                    };
                    excelBytes = ExcelHelper.ExportDataToExcel(result, headers, "TemuSearchForSemiSupplier", $"申请核价操作记录");
                }
                return File(
                   excelBytes,
                   "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                   $"申请核价操作记录.xlsx"
               );
            }
            catch (JsonException ex)
            {
                return BadRequest($"JSON解析错误: {ex.Message}");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"服务器错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 商品属性自动补充
        /// </summary>
        /// <param name="productPropertyRequests"></param>
        /// <returns></returns>
        [HttpPost("AutoReplenishOproductPropertyRequest")]
        [Produces("application/json")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ProductPropertyRequest>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AutoReplenishOproductPropertyRequest([FromBody] List<ProductPropertyRequest> productPropertyRequests)
        {
            if (productPropertyRequests == null || productPropertyRequests.Count == 0)
            {
                return this.Fail("参数错误");
            }

            _logger.ApiInfo($"开始处理商品属性请求，Request 数量：{productPropertyRequests.Count}");
            //店铺ID
            Request.Headers.TryGetValue("mallid", out var mallId);
            long _mallId = Convert.ToInt64(mallId.ToString());
            int maxThreads = 10;
            var tasks = new List<Task>();
            List<EditRequst> responses = new List<EditRequst>();
            ConcurrentBag<ProductPropertyRequestRecord> productPropertyRequestRecordList = new ConcurrentBag<ProductPropertyRequestRecord>();
            var batches = SplitList(productPropertyRequests, maxThreads);

            foreach (var batch in batches)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var result = await ProcessProductPropertyNewAsync(batch, productPropertyRequestRecordList);
                        lock (responses)
                        {
                            responses.AddRange(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.ApiError($"处理批次时发生错误：{ex.Message}");
                    }
                }));
            }

            // 等待所有组的任务完成
            await Task.WhenAll(tasks);
            // 插入记录到记录表
            var productIds = productPropertyRequestRecordList.Select(t => t.ProductId);
            var del_count = await _productPropertyRequestRecordService.DeleteAsync(t => productIds.Contains(t.ProductId));
            productPropertyRequestRecordList.ForEach(item =>
           {
               item.Mallid = _mallId;
           });
            var add_count = await _productPropertyRequestRecordService.CreateAsync(productPropertyRequestRecordList.ToList());
            _logger.ApiInfo($"所有批次任务处理完成，删除：{del_count}条，新增：{add_count}条，总响应数量：{responses.Count}");

            return this.Success(responses);
        }

        /// <summary>
        /// 递归处理商品属性，构建问题文本并调用 GPT 获取响应（多个问题请求一次）
        /// </summary>
        /// <param name="allProperties"></param>
        /// <param name="currentProperties"></param>
        /// <param name="_gptResponsesBag"></param>
        /// <param name="productPropertyRequest"></param>
        /// <param name="productPictureBase64"></param>
        /// <param name="systemContent"></param>
        /// <returns></returns>

        private async Task RecursiveProcessPropertiesNewAsync(
    List<ProductPicture> allProperties,
    List<ProductPicture> currentProperties,
    ConcurrentBag<GptResponses> _gptResponsesBag,
    ProductPropertyRequest productPropertyRequest,
    string productPictureBase64,
    SystemMessage systemContent)
        {
            // 存储所有问题和对应属性
            var questionsList = new ConcurrentBag<(ProductPicture item, string question)>();

            foreach (var item in currentProperties)
            {
                StringBuilder questionBuilder = new StringBuilder();
                StringBuilder optionsBuilder = new StringBuilder();

                if (item.controlType == 0) // 如果有父级的问题
                {
                    questionBuilder.AppendLine($"图片中的商品{item.name}是多少？");
                    questionBuilder.AppendLine($"请输入一个值，单位有：{item.valueUnit?.ToJsonSerializeObject()}");
                    questionBuilder.AppendLine($"请你根据图片回答以上问题， 输入的值不需要带上我提供的单位，回复 {{\"refPid\": {item.refPid},\"vid\":0,\"value\":\"#value\"}} 这样的JSON格式，需要把你输入的值填充到#value中并以{{\"refPid\": {item.refPid},\"vid\":0,\"value\":\"#value\"}}这样的JSON格式回复，只需要回复JSON格式，不需要回复多余的文字，如果图片识别不出来那就返回一个对应单位的默认值填充到#value！");
                }
                else
                {
                    if (item.templatePropertyValueParentList != null && item.templatePropertyValueParentList.Count > 0)
                    {
                        foreach (var template in item.templatePropertyValueParentList)
                        {
                            if (_gptResponsesBag.Any(t => template.parentVidList.Contains(t.vid)) && item.values != null)
                            {
                                var temp_value = item.values.Where(t => template.vidList.Contains(t.vid)).ToList();
                                if (temp_value != null && temp_value.Any())
                                {
                                    optionsBuilder.AppendLine($"选项：{temp_value?.ToJsonSerializeObject()}");
                                }
                            }
                        }
                    }
                    else
                    {
                        optionsBuilder.AppendLine($"选项：{item.values?.ToJsonSerializeObject()}");
                    }

                    questionBuilder.AppendLine($"图片中的商品{item.name}是以下哪个选项？");
                    questionBuilder.AppendLine(optionsBuilder.ToString());
                    if (item.chooseMaxNum > 0)
                    {
                        questionBuilder.AppendLine($"你可以选择：{item.chooseMaxNum}个选项！");
                    }
                    questionBuilder.AppendLine($"请你根据图片回答上述问题，回答的内容以{{\"refPid\": #refPid,\"vid\":#vid,\"value\":\"#value\"}}这样的JSON格式回复，如果一个问题有多个选项，格式应当是一个集合中包含多个对象的JSON格式，例如： [{{\"refPid\": #refPid,\"vid\":#vid,\"value\":\"#value\"}},{{\"refPid\": #refPid,\"vid\":#vid,\"value\":\"#value\"}}]，只需要回复JSON格式，不需要回复多余的文字，请注意：#refPid是选项中对应的refPid，#vid是选项中对应的vid，#value是选项中对应的value。如果图片识别不出来那就默认选择第一个选项！");
                }

                if (questionBuilder.Length > 0)
                {
                    questionsList.Add((item, questionBuilder.ToString()));
                }
            }
            if (questionsList.Count > 0)
            {
                // 将所有问题拼装到一个请求中，减少 GPT 的调用次数
                var combinedQuestions = string.Join("\n\n", questionsList.Select(q => q.question));
                combinedQuestions += $"最后把所有问题的答案都组装在一个集合返回一个正确的JSON格式的集合，例如：[{{\"refPid\": #refPid,\"vid\":#vid,\"value\":\"#value\"}},{{\"refPid\": #refPid,\"vid\":#vid,\"value\":\"#value\"}}]！";
                var responses = await ProcessSinglePropertyAsync(productPictureBase64, new StringBuilder(combinedQuestions), systemContent);

                if (responses != null && responses.Count > 0)
                {
                    foreach (var response in responses)
                    {
                        lock (response)
                        {
                            _gptResponsesBag.Add(response);
                        }
                    }
                }

                // 按照对应关系处理子级属性递归
                foreach (var (item, _) in questionsList)
                {
                    var subProperties = allProperties
                        .Where(t => t.parentTemplatePid == item.templatePid)
                        .ToList();

                    if (subProperties.Any())
                    {
                        await RecursiveProcessPropertiesNewAsync(allProperties, subProperties, _gptResponsesBag, productPropertyRequest, productPictureBase64, systemContent);
                    }
                }
            }
        }

        /// <summary>
        /// 成功回调
        /// </summary>
        /// <param name="editCallBacks"></param>
        /// <returns></returns>
        [HttpPost("EditCallBack")]
        [Produces("application/json")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<EditCallBack>), StatusCodes.Status200OK)]
        public async Task<IActionResult> EditCallBack([FromBody] List<EditCallBack> editCallBacks)
        {
            if (editCallBacks == null || editCallBacks.Count == 0)
            {
                return this.Fail();
            }
            _logger.ApiInfo($"属性补充执行回调，Request 数量：{editCallBacks.Count}");

            var productIds = editCallBacks.Select(t => t.productId).ToArray();
            Expression<Func<ProductPropertyRequestRecord, bool>> predicate = t => productIds.Contains(t.ProductId);
            var temp_excel_Results = await _productPropertyRequestRecordService.GetQueryListAsync(predicate);
            temp_excel_Results.ForEach(item =>
            {
                if (editCallBacks.Any(t => t.productId == item.ProductId))
                {
                    var temp_first = editCallBacks.FirstOrDefault(t => t.productId == item.ProductId);
                    item.IsSuccess = temp_first?.success ?? false;
                    item.ResultText = temp_first?.resultText ?? string.Empty;
                }
            });
            //暂时这么用
            await _productPropertyRequestRecordService.DeleteAsync(predicate);
            await _productPropertyRequestRecordService.CreateAsync(temp_excel_Results);
            return this.Success(temp_excel_Results.Select(t => t.ProductId).ToList());

        }
        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="productIds"></param>
        /// <returns></returns>
        [HttpPost("DownloadEditRecord")]
        [Produces("application/json")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadEditRecord([FromBody] List<long>? productIds = null)
        {
            byte[] excelBytes = new byte[0];
            Expression<Func<ProductPropertyRequestRecord, bool>> predicate = t => true;
            //店铺ID
            Request.Headers.TryGetValue("mallid", out var mallId);
            if (string.IsNullOrEmpty(mallId))
            {
                return BadRequest("缺少店铺 ID (mallid)。");
            }
            long _mallId;
            if (!long.TryParse(mallId.ToString(), out _mallId) || _mallId <= 0)
            {
                return BadRequest("无效的店铺 ID (mallid)。");
            }
            else
            {
                predicate = predicate.And(t => t.Mallid == _mallId);
            }

            if (productIds != null && productIds?.Count > 0)
            {
                predicate = predicate.And(t => productIds.Contains(t.ProductId));
            }
            var temp_excel_Results = await _productPropertyRequestRecordService.GetQueryListAsync(predicate);
            if (temp_excel_Results?.Count() > 0)
            {
                // 遍历每一项并处理 ProductProperties 字段
                foreach (var item in temp_excel_Results)
                {
                    if (!string.IsNullOrWhiteSpace(item.ProductProperties))
                    {
                        try
                        {
                            // 修复 JSON 字符串，转义
                            item.ProductProperties = item.ProductProperties.UnescapeJson();
                            // 将 ProductProperties 从 JSON 字符串反序列化为对象列表
                            var propertiesList = JsonConvert.DeserializeObject<List<ProductPropertyResponse>>(item.ProductProperties);

                            // 筛选出所需字段并重命名为更直观的字段名
                            var formattedProperties = propertiesList?.Select(p => new
                            {
                                属性ID = p.refPid.ToString(),                 // refPid -> 属性ID
                                商品属性问题 = p.propName ?? "未知问题",          // propName -> 问题
                                GPT给的答案 = p.propValue ?? "未填写",           // propValue -> 答案
                                属性单位 = p.valueUnit ?? ""                 // valueUnit -> 单位
                            }).ToList();

                            // 将筛选结果重新序列化为 JSON 字符串以便后续处理
                            item.ProductProperties = JsonConvert.SerializeObject(formattedProperties);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"处理 ProductProperties 时发生错误：{ex.Message}");
                            // 如果处理失败，可以选择清空 ProductProperties 或保留原值
                            item.ProductProperties = "[]"; // 默认设置为空数组字符串
                        }
                    }
                }
                // Excel 文件标题映射（筛选后的字段）
                Dictionary<string, string> headers = new Dictionary<string, string>
                {
                    { "商品ID", "ProductId" },
                    { "商品标题", "ProductName" },
                    { "商品图片", "ProductPicture" },
                    { "商品属性结果", "ProductProperties" },
                    { "GPT返回的内容", "GptResponses" },
                    { "属性是否补充成功", "IsSuccess" },
                    { "Temu修改接口返回的消息", "ResultText" }
                };

                excelBytes = ExcelHelper.ExportDataToExcel(temp_excel_Results.AsQueryable(), headers, "TemuSearchForSemiSupplier", $"申请核价操作记录");
            }
            return File(
               excelBytes,
               "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
               $"商品属性自动补充.xlsx"
           );
        }

        /// <summary>
        /// 处理单个属性的问题选项，并调用 GPT 获取答案
        /// </summary> 
        /// <param name="productPictureBase64">商品图片的 Base64 编码</param>
        /// <param name="stringBuilder">GPT 的系统消息</param>
        /// <param name="systemContent">GPT 的系统消息</param>
        /// <returns>GPT 返回的响应集合</returns>
        private async Task<List<GptResponses>> ProcessSinglePropertyAsync(
            string productPictureBase64,
            StringBuilder stringBuilder,
            SystemMessage systemContent)
        {
            ConcurrentBag<GptResponses> gptResponsesList = new ConcurrentBag<GptResponses>();

            var textContent = new ContentItem
            {
                type = "text",
                text = stringBuilder.ToString()
            };

            // 构建图片内容
            var imageContent = new ContentImageItem
            {
                type = "image_url",
                image_url = new ImageUrl() { url = $"{productPictureBase64}" }
            };

            // 构建用户消息
            var chatMessage = new ChatMessage
            {
                Role = "user",
                Content = new List<object> { textContent, imageContent }
            };

            // 调用 GPT 服务
            var gptResponses = await _chatGPTService.GetChatGPTResponseAsync(new List<dynamic>
                {
                    systemContent,
                    chatMessage
                });

            // 解析 GPT 响应
            if (!string.IsNullOrEmpty(gptResponses))
            {
                try
                {
                    gptResponses = gptResponses.Replace("```json", "").Replace("```", "").Trim();
                    var parsedResponses = JsonConvert.DeserializeObject<List<GptResponses>>(gptResponses);
                    if (parsedResponses != null)
                    {
                        parsedResponses.ForEach(item =>
                        {
                            lock (item)
                            {
                                gptResponsesList.Add(item);
                            }
                        });
                    }

                }
                catch (Exception ex)
                {
                    _logger.ApiError($"解析 GPT 响应时发生错误：{ex.Message}");
                }
            }
            return gptResponsesList.ToList();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="productPropertyRequestRecordList"></param>
        /// <returns></returns>
        private async Task<List<EditRequst>> ProcessProductPropertyNewAsync(List<ProductPropertyRequest> request, ConcurrentBag<ProductPropertyRequestRecord> productPropertyRequestRecordList)
        {

            _logger.ApiInfo($"开始处理商品属性，Request Count：{request.Count}");
            ConcurrentBag<EditRequst> editRequstList = new ConcurrentBag<EditRequst>();
            var systemContent = new SystemMessage
            {
                Role = "system",
                Content = "你是一个电商产品图片识别助手"
            };
            for (int i = 0; i < request.Count; i++)
            {
                var productPropertyRequest = request[i];
                _logger.ApiInfo($"正在处理第 {i + 1}/{request.Count} 个商品属性请求");

                int retryCount = 0;
                bool success = false;
                var textContent = new ContentItem
                {
                    type = "text",
                    text = $"图片中商品的描述：{productPropertyRequest.productName}，问题如下："
                };
                // 商品的图片
                var productPictureBase64 = await DownloadImageAsBase64Async(productPropertyRequest.productPicture);
                while (retryCount < 3 && !success) // 包括初次尝试，最多重试3次
                {
                    try
                    {
                        #region 构建问题 
                        // 存储所有问题
                        StringBuilder combinedQuestions = new StringBuilder();
                        var refpids = productPropertyRequest.requiredNotFilledInProperties.Select(t => t.refPid).ToList();
                        var propertiesList = productPropertyRequest.properties.Where(t => refpids.Contains(t.refPid)).ToList();

                        foreach (var prop_model in propertiesList)
                        {
                            if (prop_model.controlType == 1) // 下拉选项类型
                            {
                                #region 获取正确的下拉值
                                if ((prop_model.parentTemplatePid ?? 0) > 0
                                    && prop_model.templatePropertyValueParentList != null
                                    && prop_model.values != null)
                                {
                                    var model = productPropertyRequest.properties.FirstOrDefault(t => t.templatePid == prop_model.parentTemplatePid);
                                    var parentProperties = productPropertyRequest.productPropertyList?.FirstOrDefault(t => t.refPid == model.refPid);
                                    if (parentProperties != null)
                                    {
                                        continue;
                                    }
                                    var temp_vid = parentProperties.vid;
                                    var vids = prop_model.templatePropertyValueParentList
                                        .FirstOrDefault(t => t.parentVidList.Any(s => s == temp_vid))?.vidList;
                                    prop_model.values = prop_model.values.Where(t => vids != null && vids.Contains(t.vid)).ToList();
                                }
                                #endregion

                                combinedQuestions.AppendLine($"图片中的商品{prop_model.name}是以下哪个选项？");
                                combinedQuestions.AppendLine($"选项：{prop_model.values?.ToJsonSerializeObject()} ");
                                if (prop_model.chooseMaxNum > 0)
                                {
                                    combinedQuestions.AppendLine($"该问题你可以选择：{prop_model.chooseMaxNum}个选项！");
                                }
                                combinedQuestions.AppendLine($"请你根据图片回答上述问题，回答的内容以{{\"refPid\": #refPid,\"vid\":#vid,\"value\":\"#value\"}}这样的JSON格式回复，如果一个问题有多个选项，格式应当是一个集合中包含多个对象的JSON格式，例如： [{{\"refPid\": #refPid,\"vid\":#vid,\"value\":\"#value\"}},{{\"refPid\": #refPid,\"vid\":#vid,\"value\":\"#value\"}}]，只需要回复JSON格式，不需要回复多余的文字，请注意：#refPid是选项中对应的refPid，#vid是选项中对应的vid，#value是选项中对应的value。如果图片识别不出来那就默认选择第一个选项！");
                            }
                            else // 输入框类型
                            {
                                combinedQuestions.AppendLine($"图片中的商品{prop_model.name}是多少？");
                                combinedQuestions.AppendLine($"请输入一个值，单位有：{prop_model.valueUnit?.ToJsonSerializeObject()}！");
                                combinedQuestions.AppendLine($"请你根据图片回答以上问题， 输入的值不需要带上我提供的单位，回复 {{\"refPid\": {prop_model.refPid},\"vid\":0,\"value\":\"#value\"}} 这样的JSON格式，只需要回复JSON格式，不需要回复多余的文字，注意：需要把你输入的值填充到#value中以[{{\"refPid\": {prop_model.refPid},\"vid\":0,\"value\":\"#value\"}}] 这样的JSON格式回复 ，如果图片识别不出来那就返回一个对应单位的默认值填充到#value！");
                            }
                        }
                        combinedQuestions.AppendLine($"最后把所有问题的答案都组装在一个集合返回一个正确的JSON格式的集合，例如：[{{\"refPid\": #refPid,\"vid\":#vid,\"value\":\"#value\"}},{{\"refPid\": #refPid,\"vid\":#vid,\"value\":\"#value\"}}]！");

                        // 一次调用 GPT 处理所有问题
                        ConcurrentBag<GptResponses> _gptResponsesList = new ConcurrentBag<GptResponses>();
                        var gptResponses = await ProcessSinglePropertyAsync(productPictureBase64, combinedQuestions, systemContent);
                        gptResponses.ForEach(response =>
                        {
                            _gptResponsesList.Add(response);
                        });

                        // 查找子级属性并递归处理
                        var temp_refPids = _gptResponsesList.Select(t => t.refPid).ToList();
                        var temp_templatePids = productPropertyRequest.properties
                            .Where(t => temp_refPids.Contains(t.refPid))
                            .Select(t => t.templatePid)
                            .ToList();
                        var remp_productPropertyRequest = productPropertyRequest.properties
                            .Where(t => temp_templatePids.Contains(t.parentTemplatePid))
                            .ToList();

                        await RecursiveProcessPropertiesNewAsync(productPropertyRequest.properties, remp_productPropertyRequest, _gptResponsesList, productPropertyRequest, productPictureBase64, systemContent);

                        #endregion

                        #region 组装edit修改接口参数
                        var editRequest = new EditRequst();
                        editRequest.productProperties = productPropertyRequest.productPropertyList?.Cast<object>().ToList();

                        if (_gptResponsesList != null && _gptResponsesList.Count > 0)
                        {
                            foreach (var item in _gptResponsesList)
                            {
                                var temp_product = productPropertyRequest.properties.FirstOrDefault(t => t.refPid == item.refPid);
                                if (temp_product != null)
                                {
                                    var valueUnit = string.Empty;
                                    // 1：下拉选项，0：输入框
                                    if (temp_product.controlType == 0)
                                    {
                                        valueUnit = temp_product.valueUnit != null ? temp_product.valueUnit[0] : string.Empty;
                                    }
                                    var jsonResponse = new ProductPropertyResponse()
                                    {
                                        controlType = temp_product.controlType,
                                        pid = temp_product.pid,
                                        propName = temp_product.name,
                                        propValue = item.value,
                                        refPid = item.refPid,
                                        templatePid = temp_product.templatePid ?? 0,
                                        valueExtendInfo = temp_product.valueExtendInfo,
                                        valueUnit = valueUnit,
                                        vid = item.vid,
                                    };
                                    editRequest.productProperties.Add(jsonResponse);
                                }
                            }
                        }

                        editRequest.productId = productPropertyRequest.productId;
                        editRequest.editScene = 1;
                        editRequstList.Add(editRequest);
                        #endregion

                        #region 记录到请求记录表
                        productPropertyRequestRecordList.Add(new ProductPropertyRequestRecord
                        {
                            RequestId = Guid.NewGuid().ToString(),
                            ProductId = productPropertyRequest.productId,
                            EditScene = 1,
                            CreateDate = DateTime.Now,
                            ProductName = productPropertyRequest.productName,
                            ProductQuestions = JsonConvert.SerializeObject(productPropertyRequest.properties),
                            ProductPicture = productPropertyRequest.productPicture,
                            TextContent = textContent?.ToJsonSerializeObject(),
                            ProductProperties = JsonConvert.SerializeObject(editRequest.productProperties),
                            GptResponses = _gptResponsesList.ToJsonSerializeObject()
                        });
                        #endregion

                        success = true;
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        _logger.ApiError($"处理第 {i + 1}/{request.Count} 个商品：{productPropertyRequest.productId} 属性时发生错误：{ex.Message}，第 {retryCount}/3 次重试");
                        if (retryCount >= 3)
                        {
                            _logger.ApiError($"商品：{productPropertyRequest.productId}，超过最大重试次数，跳过当前请求");
                            break;
                        }
                    }
                }
            }

            return editRequstList.ToList();
        }
        private List<ProductPicture> SortProperties(List<ProductPicture> properties)
        {
            // 创建一个字典，方便快速查找 parentTemplatePid 对应的对象
            var ProductPictureDict = properties.ToDictionary(p => (p.templatePid ?? 0));

            // 定义返回结果集合
            var result = new List<ProductPicture>();

            // 使用 HashSet 记录已经处理过的节点，防止重复添加
            var processed = new System.Collections.Generic.HashSet<int?>();

            // 递归函数：将当前对象及其子级按顺序添加到结果集合
            void AddProductPictureAndChildren(ProductPicture ProductPicture)
            {
                if (processed.Contains(ProductPicture.templatePid))
                    return;

                // 添加当前对象到结果集合
                result.Add(ProductPicture);
                processed.Add(ProductPicture.templatePid);

                // 查找当前对象的所有子级对象
                var children = properties.Where(p => p.parentTemplatePid == ProductPicture.templatePid).ToList();

                // 对每个子级递归调用
                foreach (var child in children)
                {
                    AddProductPictureAndChildren(child);
                }
            }

            // 找到所有没有父级的根对象（parentTemplatePid 为 null 或不在 templatePid 列表中的对象）
            var rootProperties = properties.Where(p => !p.parentTemplatePid.HasValue || !ProductPictureDict.ContainsKey(p.parentTemplatePid.Value)).ToList();

            // 先处理根对象及其子级
            foreach (var root in rootProperties)
            {
                AddProductPictureAndChildren(root);
            }

            // 如果还有未处理的对象，说明可能存在循环引用，单独处理
            var remainingProperties = properties.Where(p => !processed.Contains(p.templatePid)).ToList();
            foreach (var ProductPicture in remainingProperties)
            {
                AddProductPictureAndChildren(ProductPicture);
            }

            return result;
        }
        /// <summary>
        /// 商品属性补充 【测试】
        /// </summary> 
        /// <returns></returns>
        [HttpPost("AutoReplenishOproductPropertyRequestTest")]
        [Produces("application/json")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ProductPropertyRequest>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AutoReplenishOproductPropertyRequestTest()
        {
            var textContent = new ContentItem
            {
                type = "text",
                text = $"图上商品的连接器材质是以下哪个选项，只需要回复答案{{\r\n  \"金属\": \"Golden\",\r\n  \"塑料\": \"Plastic\",\r\n  \"木质\": \"Wooden\",\r\n  \"竹子\": \"Bamboo\",\r\n  \"无\": \"None\",\r\n  \"其他\": \"Other\"\r\n}}\r\n"
            };
            var imgBase64 = await DownloadImageAsBase64Async("https://img.cdnfe.com/product/fancy/073106d6-0175-4aac-8bd1-601f098cc12f.jpg?imageMogr2/thumbnail");
            var imageContent = new ContentImageItem
            {
                type = "image_url",
                image_url = new ImageUrl() { url = $"{imgBase64}" }
            };
            var systemContent = new SystemMessage
            {
                Role = "system",
                Content = $"You are a helpful assistant."
            };
            var chatMessage = new ChatMessage
            {
                Role = "user",
                Content = new List<object> { textContent, imageContent }
            };
            var gptResponses = await _chatGPTService.GetChatGPTResponseAsync(new List<dynamic>
            {
                systemContent,
                chatMessage
            });
            return Ok(new { Responses = gptResponses });
        }



        #region Action 
        /// <summary>
        /// 下载图片并将其转换为 Base64 字符串
        /// </summary>
        /// <param name="imageUrl">图片的 URL 地址</param>
        /// <returns>以 Base64 格式编码的图片数据</returns>
        private async Task<string> DownloadImageAsBase64Async(string imageUrl)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    // 下载图片数据为字节数组
                    var response = await httpClient.GetAsync(imageUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"无法下载图片，状态码: {response.StatusCode}");
                    }

                    var imageBytes = await response.Content.ReadAsByteArrayAsync();

                    // 将字节数组转换为 Base64 字符串
                    string base64String = Convert.ToBase64String(imageBytes);

                    // 根据文件类型拼接 MIME 类型前缀（假设为 JPEG 格式）
                    return $"data:image/jpeg;base64,{base64String}";
                }
                catch (Exception ex)
                {
                    throw new Exception($"图片转换失败: {ex.Message}", ex);
                }
            }
        }
        /// <summary>
        /// 将列表拆分为指定数量的组
        /// </summary>
        /// <typeparam name="T">列表元素类型</typeparam>
        /// <param name="list">需要拆分的原始列表</param>
        /// <param name="groupCount">组的数量</param>
        /// <returns>拆分后的列表集合</returns>
        private List<List<T>> SplitList<T>(List<T> list, int groupCount)
        {
            var result = new List<List<T>>();
            int groupSize = Math.Min(100, (int)Math.Ceiling((double)list.Count / groupCount));
            for (int i = 0; i < list.Count; i += groupSize)
            {
                result.Add(list.GetRange(i, Math.Min(groupSize, list.Count - i))); // 防止越界
            }

            _logger.Info($"列表已拆分为 {result.Count} 个组，每组大约包含 {groupSize} 条数据。");
            return result;
        }
        /// <summary>
        /// 控制并发执行的方法
        /// </summary>
        /// <typeparam name="TInput">输入类型</typeparam>
        /// <typeparam name="TOutput">返回值类型</typeparam>
        /// <param name="inputs">需要处理的输入集合</param>
        /// <param name="processor">处理逻辑的委托</param>
        /// <param name="maxDegreeOfConcurrency">最大并发数</param>
        /// <returns>按顺序返回的结果集合</returns>
        private async Task<List<TOutput>> ExecuteConcurrentlyAsync<TInput, TOutput>(
            List<TInput> inputs,
            Func<TInput, Task<TOutput>> processor,
            int maxDegreeOfConcurrency)
        {
            var semaphore = new SemaphoreSlim(maxDegreeOfConcurrency); // 控制并发线程数
            var tasks = inputs.Select(async input =>
            {
                await semaphore.WaitAsync(); // 等待信号量可用
                try
                {
                    return await processor(input); // 执行业务处理
                }
                finally
                {
                    semaphore.Release(); // 释放信号量
                }
            });

            return await Task.WhenAll(tasks).ContinueWith(task => task.Result.ToList());
        }
        #endregion

    }
}
