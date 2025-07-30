using Highever.SocialMedia.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Highever.SocialMedia.API.Controllers
{
    /// <summary>
    /// Temu 平台
    /// </summary>
    [EnableCors("AllowSpecificOrigins")] // 应用指定的 CORS 策略
    [ApiController]
    [Route("File")]
    public class FileController : Controller
    {

        public readonly IServiceProvider _serviceProvider;
        public readonly HttpClientHelper _httpClientHelper;

        public FileController(IServiceProvider serviceProvider, HttpClientHelper httpClientHelper)
        {
            this._serviceProvider = serviceProvider;
            this._httpClientHelper = httpClientHelper;
        }
        [HttpGet]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        } 
        /// <summary>
        ///上传图片到服务器，调用后台管理的上传图片接口
        /// </summary>
        /// <remark>
        /// 这是接口介绍：
        /// .....
        /// .....
        /// .....
        /// </remark>
        /// <param name="files">表单文件信息</param>
        /// <returns></returns> 
        [ApiGroup(SwaggerApiGroup.Login)]
        [Consumes("application/json")]
        [Produces("text/plain")]
        [ProducesResponseType(typeof(AjaxResult<object>), 200)]
        [HttpPost]
        [AllowAnonymous]
        [Route("UploadImage")]
        public IActionResult UploadImage(IFormFile files)
        {
            var result = new AjaxResult<object>();
            try
            {
                if (files.Length <= 0)
                {
                    result.msg = "请选择要上传的图片！";
                }
                else
                {
                    var fileBytes = ReadFileBytes(files);
                    var fileExtension = Path.GetExtension(files.FileName);//获取文件格式，拓展名
                    var httpresult = _httpClientHelper.PostImageAsync("上传图片URL", fileBytes, fileExtension, files.FileName);
                    var resultObj = JsonConvert.DeserializeObject<UploadResult>(httpresult.Result);
                    if (resultObj != null)
                    {
                        if (resultObj.code == 200)
                        {
                            var img_res = new { ImgUrl = resultObj.data, ImgName = resultObj.imgname };
                            result.success = true;
                            result.httpCode = HttpCode.成功;
                            result.data = img_res;
                            result.msg = resultObj.Msg;
                        }
                        else
                        {
                            result.msg = resultObj.Msg;
                        }
                    }
                    else
                    {
                        result.msg = "上传失败，请检查HttpClient日志！";
                    }
                }
            }
            catch (Exception e)
            {
                result.msg = e.Message;
            }
            return Json(result);
        }
        /// <summary>
        /// 文件流类型转化字节类型
        /// </summary>
        /// <param name="fileData">表单文件信息</param>
        /// <returns></returns>

        [HiddenAPI]
        private byte[] ReadFileBytes(IFormFile fileData)
        {
            byte[] data;
            using (Stream inputStream = fileData.OpenReadStream())//读取上传文件的请求流
            {
                MemoryStream? memoryStream = inputStream as MemoryStream;
                if (memoryStream == null)
                {
                    memoryStream = new MemoryStream();
                    inputStream.CopyTo(memoryStream);
                }
                data = memoryStream.ToArray();
            }
            return data;
        }
        /// <summary>
        ///上传图片到服务器，调用后台管理的上传图片接口
        /// </summary>
        /// <remark>
        /// 这是接口介绍：
        /// .....
        /// .....
        /// .....
        /// </remark>
        /// <param name="formData">表单文件信息</param>
        /// <returns></returns> 
        [ApiGroup(SwaggerApiGroup.Login)]
        [Consumes("application/json")]
        [Produces("text/plain")]
        [ProducesResponseType(typeof(AjaxResult<object>), 200)]
        [HttpPost]
        [AllowAnonymous]
        [Route("UploadImageByCollection")]
        public IActionResult UploadImageByCollection([FromForm] IFormCollection formData)
        {
            var result = new AjaxResult<object>();
            try
            {
                var files = Request.Form.Files;
                if (files.Count == 0)
                {
                    result.msg = "没有选择的图片！";
                    return Json(result);
                }
                var file = files[0];
                string fileName = file.FileName;
                if (string.IsNullOrEmpty(fileName))//服务器是否存在该文件
                {
                    result.msg = "服务器上已存在该图片！";
                    return Json(result);
                }
                // 获取上传的图片名称和扩展名称
                string fileFullName = Path.GetFileName(file.FileName);
                string fileExtName = Path.GetExtension(fileFullName);
                var fileExtNames = AppSettingConifgHelper.ReadAppSettings("UploadImgURL:FileExtName").ToString().Split(',');
                if (!fileExtNames.Contains(fileExtName))
                {
                    result.msg = "选择的文件不是图片的格式！";
                    return Json(result);
                }
                //获取当前项目所在的路径
                string imgPath = AppSettingConifgHelper.ReadAppSettings("UploadImgURL:ImgURLAPI");
                //生成随机数
                Random rd = new Random();
                int num = rd.Next(1000, 10000);
                var newPath = "LKImages" + num + System.DateTime.UtcNow.Ticks + fileExtName;
                var src = imgPath + newPath;
                // 如果目录不存在则要先创建
                if (!Directory.Exists(imgPath))
                {
                    Directory.CreateDirectory(imgPath);
                }
                using (FileStream fs = System.IO.File.Create(src))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }

                string PathSrc = AppSettingConifgHelper.ReadAppSettings("UploadImgURL:ImgURL") + newPath;
                result.msg = "上传成功！";
                result.success = true;
                result.data = new
                {
                    data = PathSrc,
                    imgname = newPath
                };
                return Json(result);
            }
            catch (Exception e)
            {
                result.msg = "上传失败！" + e.Message;
                return Json(result);
            }
        }
    }
}
