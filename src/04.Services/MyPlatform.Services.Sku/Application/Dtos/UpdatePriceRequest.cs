namespace MyPlatform.Services.Sku.Application.Dtos;

/// <summary>
/// 更新价格请求
/// </summary>
public class UpdatePriceRequest
{
    /// <summary>
    /// 新价格
    /// </summary>
    public decimal NewPrice { get; set; }

    /// <summary>
    /// 变更原因
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
