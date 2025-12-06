
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace MyPlatform.SDK.DataExchange.Excel
{
    /// <summary>
    /// Excel Data Writer using ClosedXML.
    /// 使用 ClosedXML 的 Excel 数据写入器。
    /// </summary>
    public class ExcelDataWriter<T> where T : class
    {
        /// <summary>
        /// Writes data to an Excel stream.
        /// 将数据写入 Excel 流。
        /// </summary>
        public async Task<Stream> WriteAsync(IAsyncEnumerable<T> data, CancellationToken cancellationToken = default)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sheet1");
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Write header row / 写入标题行
            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = properties[i].Name;
            }

            // Write data rows / 写入数据行
            int rowIndex = 2;
            await foreach (var record in data.WithCancellation(cancellationToken))
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    var value = properties[i].GetValue(record);
                    worksheet.Cell(rowIndex, i + 1).Value = value?.ToString() ?? string.Empty;
                }
                rowIndex++;
            }

            var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
