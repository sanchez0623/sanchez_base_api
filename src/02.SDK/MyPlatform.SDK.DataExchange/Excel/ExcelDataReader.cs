
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using ClosedXML.Excel;

namespace MyPlatform.SDK.DataExchange.Excel
{
    /// <summary>
    /// Excel Data Reader using ClosedXML.
    /// 使用 ClosedXML 的 Excel 数据读取器。
    /// Note: ClosedXML loads the entire workbook into memory.
    /// For very large files (>100MB), consider streaming solutions like Open XML SDK directly or SAX-style parsing.
    /// 注意：ClosedXML 会将整个工作簿加载到内存中。对于非常大的文件(>100MB)，请考虑直接使用 Open XML SDK 或 SAX 风格的解析。
    /// </summary>
    public class ExcelDataReader<T> where T : class, new()
    {
        /// <summary>
        /// Reads Excel data as an async enumerable stream.
        /// 将 Excel 数据读取为异步可枚举流。
        /// </summary>
        public async IAsyncEnumerable<T> ReadAsync(Stream stream, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // ClosedXML loads synchronously, but we yield row by row for consistent API.
            // ClosedXML 是同步加载的，但我们逐行 yield 以保持 API 一致性。
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed()?.RowsUsed().Skip(1); // Skip header row / 跳过标题行

            if (rows == null) yield break;

            var headerRow = worksheet.Row(1);
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var columnMap = new Dictionary<int, PropertyInfo>();

            // Build column mapping from header names to properties
            // 根据标题名称到属性构建列映射
            foreach (var cell in headerRow.CellsUsed())
            {
                var headerName = cell.Value.ToString();
                var prop = properties.FirstOrDefault(p => 
                    string.Equals(p.Name, headerName, StringComparison.OrdinalIgnoreCase));
                if (prop != null)
                {
                    columnMap[cell.Address.ColumnNumber] = prop;
                }
            }

            foreach (var row in rows)
            {
                if (cancellationToken.IsCancellationRequested) yield break;

                var entity = new T();
                foreach (var cell in row.CellsUsed())
                {
                    if (columnMap.TryGetValue(cell.Address.ColumnNumber, out var prop))
                    {
                        try
                        {
                            var value = Convert.ChangeType(cell.Value.ToString(), prop.PropertyType);
                            prop.SetValue(entity, value);
                        }
                        catch
                        {
                            // Ignore conversion errors for robustness
                            // 为了健壮性忽略转换错误
                        }
                    }
                }
                yield return entity;
            }

            await System.Threading.Tasks.Task.CompletedTask; // Make compiler happy for async
        }
    }
}
