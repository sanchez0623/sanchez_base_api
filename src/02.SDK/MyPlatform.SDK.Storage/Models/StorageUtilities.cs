namespace MyPlatform.SDK.Storage.Models;

/// <summary>
/// 存储工具类
/// </summary>
public static class StorageUtilities
{
    /// <summary>
    /// 根据文件扩展名获取内容类型
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns>内容类型</returns>
    public static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".ico" => "image/x-icon",
            ".svg" => "image/svg+xml",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".csv" => "text/csv",
            ".txt" => "text/plain",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".html" or ".htm" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".zip" => "application/zip",
            ".rar" => "application/vnd.rar",
            ".7z" => "application/x-7z-compressed",
            ".tar" => "application/x-tar",
            ".gz" => "application/gzip",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            ".webm" => "video/webm",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// 生成对象键
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="options">上传选项</param>
    /// <returns>对象键</returns>
    public static string GenerateKey(string fileName, UploadOptions? options)
    {
        var actualFileName = options?.CustomFileName ?? fileName;
        var key = string.IsNullOrEmpty(options?.Folder)
            ? actualFileName
            : $"{options.Folder.TrimEnd('/')}/{actualFileName}";

        return key;
    }
}
