
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MyPlatform.SDK.DataExchange.Abstractions
{
    /// <summary>
    /// Interface for importing data from files (Excel/CSV).
    /// 从文件（Excel/CSV）导入数据的接口。
    /// Uses IAsyncEnumerable for streaming large files row-by-row.
    /// 使用 IAsyncEnumerable 逐行流式处理大文件。
    /// </summary>
    /// <typeparam name="T">The type of entity to import.</typeparam>
    public interface IDataImporter<T> where T : class, new()
    {
        /// <summary>
        /// Imports data from a file stream.
        /// 从文件流导入数据。
        /// </summary>
        /// <param name="fileStream">The file stream to read from.</param>
        /// <param name="format">The format of the file (Excel or CSV).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An async enumerable of entities.</returns>
        IAsyncEnumerable<T> ImportAsync(Stream fileStream, DataFormat format, CancellationToken cancellationToken = default);
    }
}
