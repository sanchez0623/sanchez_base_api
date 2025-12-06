
using Microsoft.Extensions.DependencyInjection;
using MyPlatform.SDK.DataExchange.Csv;
using MyPlatform.SDK.DataExchange.Excel;

namespace MyPlatform.SDK.DataExchange.Extensions
{
    /// <summary>
    /// Extension methods for registering DataExchange services.
    /// 注册 DataExchange 服务的扩展方法。
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds DataExchange services (CSV/Excel readers and writers).
        /// 添加 DataExchange 服务（CSV/Excel 读写器）。
        /// </summary>
        public static IServiceCollection AddDataExchange(this IServiceCollection services)
        {
            // Register generic readers/writers as open generics
            // Note: For generic types, consumers will need to resolve CsvDataReader<T> directly.
            // 注意：对于泛型类型，使用者需要直接解析 CsvDataReader<T>。
            
            // Alternative: Register specific types as needed in the consuming application.
            // 替代方案：在使用应用中根据需要注册特定类型。
            
            return services;
        }
    }
}
