
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace MyPlatform.SDK.DataExchange.Csv
{
    /// <summary>
    /// CSV Data Writer using CsvHelper.
    /// 使用 CsvHelper 的 CSV 数据写入器。
    /// </summary>
    public class CsvDataWriter<T> where T : class
    {
        private readonly CsvConfiguration _config;

        public CsvDataWriter(CsvConfiguration? config = null)
        {
            _config = config ?? new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };
        }

        /// <summary>
        /// Writes data to a CSV stream.
        /// 将数据写入 CSV 流。
        /// </summary>
        public async Task<Stream> WriteAsync(IAsyncEnumerable<T> data, CancellationToken cancellationToken = default)
        {
            var memoryStream = new MemoryStream();
            await using var writer = new StreamWriter(memoryStream, leaveOpen: true);
            await using var csv = new CsvWriter(writer, _config);

            // Write header
            csv.WriteHeader<T>();
            await csv.NextRecordAsync();

            // Write records
            await foreach (var record in data.WithCancellation(cancellationToken))
            {
                csv.WriteRecord(record);
                await csv.NextRecordAsync();
            }

            await csv.FlushAsync();
            await writer.FlushAsync(cancellationToken);
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
