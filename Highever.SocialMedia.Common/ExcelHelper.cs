using Npoi.Mapper;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Dynamic;

namespace Highever.SocialMedia.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class ExcelHelper
    {  
        #region 泛型
        /// <summary>
        /// List转Excel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">数据</param>
        /// <param name="sheetName">表名</param>
        /// <param name="overwrite">true,覆盖单元格，false追加内容(list和创建的excel或excel模板)</param>
        /// <param name="xlsx">true-xlsx，false-xls</param>
        /// <returns>返回文件</returns>
        public static MemoryStream ParseListToExcel<T>(List<T> list, string sheetName = "sheet1", bool overwrite = true, bool xlsx = true) where T : class
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var mapper = new Mapper();
                mapper.Save<T>(ms, list, sheetName, overwrite, xlsx);
                return ms;
            }
        }
        /// <summary>
        /// Excel转为List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileStream"></param>
        /// <param name="sheetname"></param>
        /// <returns></returns>
        public static List<T> ParseExcelToList<T>(Stream fileStream, string sheetname = "") where T : class
        {
            List<T> ModelList = new List<T>();
            var mapper = new Mapper(fileStream);
            List<RowInfo<T>> DataList = new List<RowInfo<T>>();
            if (!string.IsNullOrEmpty(sheetname))
            {
                DataList = mapper.Take<T>(sheetname).ToList();
            }
            else
            {
                DataList = mapper.Take<T>().ToList();
            }

            if (DataList != null && DataList.Count > 0)
            {
                foreach (var item in DataList)
                {
                    ModelList.Add(item.Value);
                }
            }
            return ModelList;
        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static List<T> ReadExcelFile<T>(string filePath) where T : new()
        {
            var entities = new List<T>();

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook =null;

                if (Path.GetExtension(filePath).ToLower() == ".xls")
                {
                    workbook = new HSSFWorkbook(fileStream);
                }
                else if (Path.GetExtension(filePath).ToLower() == ".xlsx")
                {
                    workbook = new XSSFWorkbook(fileStream);
                }
                else
                {
                    throw new Exception("文件格式不支持，仅支持 .xls 和 .xlsx");
                }

                ISheet sheet = workbook.GetSheetAt(0);
                if (sheet == null)
                {
                    throw new Exception("Excel文件没有可用的工作表");
                }

                var headerRow = sheet.GetRow(0); // 假设第一行为标题
                var properties = typeof(T).GetProperties();

                // 映射列到属性
                var columnMapping = new Dictionary<int, System.Reflection.PropertyInfo>();
                for (int colIndex = 0; colIndex < headerRow.LastCellNum; colIndex++)
                {
                    var headerValue = headerRow.GetCell(colIndex)?.StringCellValue;
                    if (!string.IsNullOrEmpty(headerValue))
                    {
                        var property = properties.FirstOrDefault(p => p.Name.Equals(headerValue, StringComparison.OrdinalIgnoreCase));
                        if (property != null)
                        {
                            columnMapping[colIndex] = property;
                        }
                    }
                }
                try
                {
                    for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
                    {
                        var row = sheet.GetRow(rowIndex);
                        if (row == null) continue;

                        var entity = new T();
                        foreach (var mapping in columnMapping)
                        {
                            int colIndex = mapping.Key;
                            var property = mapping.Value;

                            var cell = row.GetCell(colIndex);
                            if (cell != null)
                            {
                                object cellValue = GetCellValue(cell, property.PropertyType);
                                property.SetValue(entity, cellValue);
                            }
                        }

                        entities.Add(entity);
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
                finally {

                    fileStream.Close();
                    fileStream.Dispose();
                }

            }
            return entities;
        }

        #region 自定义 
        /// <summary>
        /// 使用 NPOI 的 SXSSFWorkbook 将给定的数据集导出为 Excel 文件。
        /// 处理大数据量时，会自动拆分到多个工作表中。
        /// </summary>
        /// <typeparam name="T">数据集合的元素类型。</typeparam>
        /// <param name="data">要导出的数据集合。</param>
        /// <param name="headers">表头映射字典，键为表头显示名称（中文），值为属性名称。</param>
        /// <param name="folder">文件夹。</param>
        /// <param name="filename">导出的 Excel 文件名。</param>
        /// <returns>表示 Excel 文件的字节数组。</returns>
        public static byte[] ExportDataToExcel<T>(
            IEnumerable<T> data,
            Dictionary<string, string> headers,
            string folder,
            string filename = "ExportedData.xlsx")
        {
            if (data == null || !data.Any())
                throw new ArgumentException("数据不能为空。", nameof(data));

            if (headers == null || headers.Count == 0)
                throw new ArgumentException("表头映射不能为空。", nameof(headers));

            if (string.IsNullOrEmpty(folder))
                throw new ArgumentException("文件夹不能为空。", nameof(folder));

            #region 文件夹检查和构建路径
            // 获取项目根目录
            string rootPath = Directory.GetCurrentDirectory();
            // 构建 Export 文件夹路径
            string exportFolderPath = Path.Combine(rootPath, "Export", folder);

            // 检查文件夹是否存在，如果不存在则创建
            if (!Directory.Exists(exportFolderPath))
            {
                Directory.CreateDirectory(exportFolderPath);
            }
            #endregion

            // 构建完整文件路径
            string filePath = Path.Combine(exportFolderPath, filename);
            // Excel 每个工作表的最大行数
            const int MaxRowsPerSheet = 1048576;
            // 减去表头行
            const int DataRowsPerSheet = MaxRowsPerSheet - 1;

            var workbook = new NPOI.XSSF.Streaming.SXSSFWorkbook();

            ISheet sheet = null;
            int currentSheetIndex = 1;
            int currentRowIndex = 0;

            // 创建第一个工作表
            sheet = workbook.CreateSheet($"Report{currentSheetIndex}");
            // 创建样式
            ICellStyle headerStyle = CreateHeaderCellStyle(workbook);
            ICellStyle dataStyle = CreateDataCellStyle(workbook);
            // 插入表头
            InsertHeaderRow(sheet, headers, ref currentRowIndex, headerStyle);

            // 遍历数据插入到 Excel 中
            foreach (var record in data)
            {
                // 检查当前工作表是否已满
                if (currentRowIndex >= DataRowsPerSheet)
                {
                    // 创建新的工作表
                    currentSheetIndex++;
                    sheet = workbook.CreateSheet($"Report{currentSheetIndex}");
                    currentRowIndex = 0;
                    // 插入表头
                    InsertHeaderRow(sheet, headers, ref currentRowIndex, headerStyle);
                }

                // 创建新行
                IRow row = sheet.CreateRow(currentRowIndex++);
                int cellIndex = 0;

                foreach (var header in headers)
                {
                    ICell cell = row.CreateCell(cellIndex++);
                    object value = GetPropertyValue(record, header.Value); // 根据属性名称获取值
                    SetCellValue(cell, value, dataStyle); // 设置单元格值和样式
                }
            }

            // 手动设置列宽（根据需要调整宽度值）
            SetColumnWidths(sheet, headers);

            // 将工作簿写入文件
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(fs);
            }
            workbook.Dispose();
            // 验证文件是否成功创建
            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException("导出的文件未找到。", filePath);

            // 读取保存的文件并返回字节数组
            try
            {
                return System.IO.File.ReadAllBytes(filePath);
            }
            catch (Exception ex)
            {
                // 可以在此处记录异常日志
                throw new IOException("读取导出文件失败。", ex);
            }
        }
        #endregion

        #endregion 

        #region 样式/辅助 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="numericValue"></param>
        /// <returns></returns>
        public static object ConvertExcelNumericToDate(Type targetType, double numericValue)
        {
            // Excel 起始日期：1900-01-01
            DateTime baseDate = new DateTime(1900, 1, 1);

            // 检查是否在 1900 年 2 月 28 日之前，因为 Excel 算错了 1900 年是闰年
            if (numericValue < 61)
            {
                numericValue -= 1; // 修正 Excel 的错误
            }

            // 将数值拆分成日期和时间
            int days = (int)Math.Floor(numericValue); // 整数部分 -> 天数
            double fractionalDay = numericValue - days; // 小数部分 -> 时间

            // 计算日期和时间
            DateTime result = baseDate.AddDays(days - 1).AddDays(fractionalDay);

            // 根据目标类型返回结果
            if (targetType == typeof(string))
            {
                return result.ToString("yyyy-MM-dd HH:mm:ss"); // 返回字符串格式
            }
            else if (targetType == typeof(DateTime))
            {
                return result; // 返回 DateTime 类型
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 从 Excel 单元格中获取值并转换为目标类型。
        /// </summary>
        /// <param name="cell">Excel 单元格。</param>
        /// <param name="targetType">目标类型。</param>
        /// <returns>解析后的值，类型与目标类型一致。</returns>
        public static object GetCellValue(ICell cell, Type targetType)
        {
            if (cell == null || cell.CellType == CellType.Blank)
            {
                // 如果为空，根据目标类型返回默认值
                if (targetType == typeof(int)) return 0;
                if (targetType == typeof(decimal)) return 0m;
                if (targetType == typeof(double)) return 0d;
                if (targetType == typeof(bool)) return false;
                if (targetType == typeof(DateTime)) return DateTime.MinValue; // 返回最小时间
                if (targetType == typeof(string)) return string.Empty; // 返回空字符串
                return null;
            }

            try
            {
                switch (cell.CellType)
                {
                    case CellType.Numeric:
                        // 尝试访问 DateCellValue，如果无异常则认为有效
                        // 检查是否是日期类型单元格
                        if (cell != null && DateUtil.IsCellDateFormatted(cell))
                        {
                            // 如果目标类型是字符串，格式化返回
                            return ConvertExcelNumericToDate(targetType, cell.NumericCellValue);
                        }
                        else
                        {
                            // 非日期类型处理
                            if (targetType == typeof(int))
                                return (int)cell.NumericCellValue; // 转换为 int 类型
                            if (targetType == typeof(decimal))
                                return Convert.ToDecimal(cell.NumericCellValue); // 转换为 decimal 类型
                            if (targetType == typeof(double))
                                return cell.NumericCellValue; // 默认 double 类型
                            // 如果 targetType 类型未被支持，抛出异常
                            return cell.NumericCellValue; // 返回原始的数值类型
                        } 
                    case CellType.String:
                        var rawValue = cell.StringCellValue.Trim();

                        // 如果目标类型是 Decimal，尝试解析数字
                        if (targetType == typeof(decimal))
                        {
                            if (decimal.TryParse(rawValue.Replace("元", "").Trim(), out var decimalValue))
                                return decimalValue;

                            throw new FormatException($"无法将值 '{rawValue}' 转换为 Decimal。");
                        }

                        // 如果目标是 DateTime，尝试解析日期
                        if (targetType == typeof(DateTime) && DateTime.TryParse(rawValue, out var dateValue))
                            return dateValue;

                        return rawValue; // 默认返回字符串

                    case CellType.Boolean:
                        return cell.BooleanCellValue;

                    default:
                        throw new Exception($"无法处理的单元格类型: {cell.CellType}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"处理单元格出错，值: '{cell}', 类型: '{cell.CellType}'", ex);
            }
        }
        private static void SetColumnWidths(ISheet sheet, Dictionary<string, string> headers)
        {
            for (int i = 0; i < headers.Count; i++)
            {
                string headerKey = headers.Keys.ElementAt(i);
                string headerValue = headers.Values.ElementAt(i);

                // 设置固定列宽，例如 20 个字符宽度
                int width = 20 * 256; // 默认值
                switch (headerValue)
                {
                    case "Date":
                        width = 20 * 256;
                        break;
                    case "Asin":
                        width = 15 * 256;
                        break;
                    case "SellerName":
                        width = 30 * 256;
                        break;
                    // 其他特殊情况...
                    default:
                        width = 20 * 256;
                        break;
                }
                sheet.SetColumnWidth(i, width);
            }
        }
        /// <summary>
        /// 创建表头的单元格样式（加粗、背景色、居中）。
        /// </summary>
        /// <param name="workbook">工作簿对象。</param>
        /// <returns>表头的单元格样式。</returns>
        private static ICellStyle CreateHeaderCellStyle(IWorkbook workbook)
        {
            ICellStyle style = workbook.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;

            // 设置背景色
            style.FillForegroundColor = IndexedColors.PaleBlue.Index;
            style.FillPattern = FillPattern.SolidForeground;

            // 设置字体加粗
            NPOI.SS.UserModel.IFont font = workbook.CreateFont();
            font.IsBold = true;
            style.SetFont(font);

            return style;
        }
        /// <summary>
        /// 创建数据单元格的样式（居中）。
        /// </summary>
        /// <param name="workbook">工作簿对象。</param>
        /// <returns>数据单元格的样式。</returns>
        private static ICellStyle CreateDataCellStyle(IWorkbook workbook)
        {
            ICellStyle style = workbook.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;
            return style;
        }
        /// <summary>
        /// 插入表头行到指定的工作表，并更新行索引。
        /// </summary>
        /// <param name="sheet">目标工作表。</param>
        /// <param name="headers">表头映射字典。</param>
        /// <param name="rowIndex">当前行索引，方法内会更新。</param>
        /// <param name="headerStyle">表头样式。</param>
        private static void InsertHeaderRow(ISheet sheet, Dictionary<string, string> headers, ref int rowIndex, ICellStyle headerStyle)
        {
            IRow headerRow = sheet.CreateRow(rowIndex++);
            int headerCellIndex = 0;
            foreach (var header in headers)
            {
                ICell cell = headerRow.CreateCell(headerCellIndex++);
                cell.SetCellValue(header.Key); // 设置表头显示名称（中文）
                cell.CellStyle = headerStyle; // 应用表头样式
            }
        }
        /// <summary>
        /// 根据属性名称从动态对象中获取属性值。
        /// </summary>
        /// <param name="record">动态对象。</param>
        /// <param name="propertyName">属性名称。</param>
        /// <returns>属性值，如果属性不存在则返回 null。</returns>
        private static object GetPropertyValue(dynamic record, string propertyName)
        {
            if (record is ExpandoObject expando)
            {
                var dict = (IDictionary<string, object>)expando;
                if (dict.TryGetValue(propertyName, out var value))
                {
                    return value;
                }
            }
            else
            {
                var dict = record as IDictionary<string, object>;
                if (dict != null)
                {
                    if (dict.TryGetValue(propertyName, out var value))
                    {
                        return value;
                    }
                }
                else
                {
                    var prop = record.GetType().GetProperty(propertyName);
                    if (prop != null)
                    {
                        return prop.GetValue(record, null);
                    }
                }
            }
            return null; // 或者其他默认值
        }

        /// <summary>
        /// 根据值的类型设置 Excel 单元格的值，并应用数据样式。
        /// </summary>
        /// <param name="cell">Excel 单元格。</param>
        /// <param name="value">要设置的值。</param>
        /// <param name="dataStyle">数据单元格样式。</param>
        private static void SetCellValue(ICell cell, object value, ICellStyle dataStyle)
        {
            if (value == null)
            {
                cell.SetCellValue(string.Empty);
                cell.CellStyle = dataStyle;
                return;
            }

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Boolean:
                    cell.SetCellValue((bool)value);
                    break;
                case TypeCode.DateTime:
                    DateTime dateValue;
                    if (value is DateTime)
                    {
                        dateValue = (DateTime)value;
                    }
                    else
                    {
                        // 尝试解析日期时间字符串
                        DateTime.TryParse(value.ToString(), out dateValue);
                    }
                    cell.SetCellValue(dateValue);
                    cell.CellStyle = dataStyle;
                    return; // 返回以避免重新设置样式
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    if (double.TryParse(value.ToString(), out double doubleValue))
                    {
                        cell.SetCellValue(doubleValue);
                    }
                    else
                    {
                        cell.SetCellValue(value.ToString());
                    }
                    cell.CellStyle = dataStyle;
                    break;
                default:
                    cell.SetCellValue(value.ToString());
                    cell.CellStyle = dataStyle;
                    break;
            }
        }
        #endregion
    }
}
