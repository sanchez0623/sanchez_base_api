using MyPlatform.Shared.Kernel.Domain;

namespace MyPlatform.Services.Sku.Domain.Entities;

/// <summary>
/// 库存实体 - 按SkuId取模分4表
/// </summary>
public class SkuStock : AggregateRoot
{
    /// <summary>
    /// SKU ID
    /// </summary>
    public long SkuId { get; private set; }

    /// <summary>
    /// 可用库存数量
    /// </summary>
    public int AvailableQuantity { get; private set; }

    /// <summary>
    /// 预留库存数量
    /// </summary>
    public int ReservedQuantity { get; private set; }

    /// <summary>
    /// 安全库存数量
    /// </summary>
    public int SafetyStock { get; private set; }

    /// <summary>
    /// 用于EF Core
    /// </summary>
    protected SkuStock() { }

    /// <summary>
    /// 创建库存记录
    /// </summary>
    /// <param name="skuId">SKU ID</param>
    /// <param name="tenantId">租户ID</param>
    /// <param name="availableQuantity">初始可用库存</param>
    /// <param name="safetyStock">安全库存</param>
    public SkuStock(long skuId, string tenantId, int availableQuantity = 0, int safetyStock = 0)
    {
        SkuId = skuId;
        AvailableQuantity = availableQuantity;
        ReservedQuantity = 0;
        SafetyStock = safetyStock;
        SetTenant(tenantId);
    }

    /// <summary>
    /// 预留库存
    /// </summary>
    /// <param name="quantity">预留数量</param>
    /// <returns>是否预留成功</returns>
    /// <exception cref="InvalidOperationException">库存不足时抛出</exception>
    public bool Reserve(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("预留数量必须大于0", nameof(quantity));
        }

        if (AvailableQuantity < quantity)
        {
            return false;
        }

        AvailableQuantity -= quantity;
        ReservedQuantity += quantity;
        return true;
    }

    /// <summary>
    /// 确认扣减预留库存
    /// </summary>
    /// <param name="quantity">扣减数量</param>
    /// <exception cref="InvalidOperationException">预留库存不足时抛出</exception>
    public void ConfirmDeduction(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("扣减数量必须大于0", nameof(quantity));
        }

        if (ReservedQuantity < quantity)
        {
            throw new InvalidOperationException($"预留库存不足，当前预留：{ReservedQuantity}，需要扣减：{quantity}");
        }

        ReservedQuantity -= quantity;
    }

    /// <summary>
    /// 释放预留库存
    /// </summary>
    /// <param name="quantity">释放数量</param>
    /// <exception cref="InvalidOperationException">预留库存不足时抛出</exception>
    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("释放数量必须大于0", nameof(quantity));
        }

        if (ReservedQuantity < quantity)
        {
            throw new InvalidOperationException($"预留库存不足，当前预留：{ReservedQuantity}，需要释放：{quantity}");
        }

        ReservedQuantity -= quantity;
        AvailableQuantity += quantity;
    }

    /// <summary>
    /// 增加库存
    /// </summary>
    /// <param name="quantity">增加数量</param>
    public void AddStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("增加数量必须大于0", nameof(quantity));
        }

        AvailableQuantity += quantity;
    }

    /// <summary>
    /// 设置安全库存
    /// </summary>
    /// <param name="safetyStock">安全库存数量</param>
    public void SetSafetyStock(int safetyStock)
    {
        if (safetyStock < 0)
        {
            throw new ArgumentException("安全库存不能为负数", nameof(safetyStock));
        }

        SafetyStock = safetyStock;
    }

    /// <summary>
    /// 检查是否低于安全库存
    /// </summary>
    /// <returns>是否低于安全库存</returns>
    public bool IsBelowSafetyStock()
    {
        return AvailableQuantity < SafetyStock;
    }

    /// <summary>
    /// 获取总库存（可用+预留）
    /// </summary>
    /// <returns>总库存数量</returns>
    public int GetTotalQuantity()
    {
        return AvailableQuantity + ReservedQuantity;
    }
}
