
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MyPlatform.SDK.DataExchange.Abstractions
{
    /// <summary>
    /// Interface for exporting data to files (Excel/CSV).
    /// 将数据导出到文件（Excel/CSV）的接口。
    /// </summary>
    /// <typeparam name="T">The type of entity to export.</typeparam>
    public interface IDataExporter<T> where T : class
    {
        /// <summary>
        /// Exports data to a stream.
        /// 将数据导出到流。
        /// </summary>
        /// <param name="data">The data to export (can be streamed via IAsyncEnumerable).</param>
        /// <param name="format">The format of the file (Excel or CSV).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A stream containing the exported file.</returns>
        Task<Stream> ExportAsync(IAsyncEnumerable<T> data, DataFormat format, CancellationToken cancellationToken = default);
    }
}
