namespace MyPlatform.SDK.Storage.Exceptions;

/// <summary>
/// 文件不存在异常
/// </summary>
public class StorageFileNotFoundException : StorageException
{
    /// <summary>
    /// 初始化文件不存在异常
    /// </summary>
    /// <param name="fileKey">文件键</param>
    public StorageFileNotFoundException(string fileKey)
        : base($"File not found: {fileKey}", fileKey) { }
}
