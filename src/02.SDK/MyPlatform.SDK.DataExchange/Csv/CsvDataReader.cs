
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using CsvHelper;
using CsvHelper.Configuration;

namespace MyPlatform.SDK.DataExchange.Csv
{
    /// <summary>
    /// CSV Data Reader using CsvHelper.
    /// 使用 CsvHelper 的 CSV 数据读取器。
    /// Streams data row by row for memory efficiency.
    /// 逐行流式传输数据以提高内存效率。
    /// </summary>
    public class CsvDataReader<T> where T : class, new()
    {
        private readonly CsvConfiguration _config;

        public CsvDataReader(CsvConfiguration? config = null)
        {
            _config = config ?? new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null, // Ignore missing fields
                HeaderValidated = null    // Don't throw on header mismatch
            };
        }

        /// <summary>
        /// Reads CSV data as an async enumerable stream.
        /// 将 CSV 数据读取为异步可枚举流。
        /// </summary>
        public async IAsyncEnumerable<T> ReadAsync(Stream stream, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, _config);

            await foreach (var record in csv.GetRecordsAsync<T>(cancellationToken))
            {
                yield return record;
            }
        }
    }
}
