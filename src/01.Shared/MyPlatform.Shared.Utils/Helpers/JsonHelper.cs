using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyPlatform.Shared.Utils.Helpers;

/// <summary>
/// Helper class for JSON serialization operations.
/// </summary>
public static class JsonHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    private static readonly JsonSerializerOptions IndentedOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    /// <summary>
    /// Serializes an object to JSON string.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="indented">Whether to use indented formatting.</param>
    /// <returns>The JSON string.</returns>
    public static string Serialize<T>(T obj, bool indented = false)
    {
        return JsonSerializer.Serialize(obj, indented ? IndentedOptions : DefaultOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to an object.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="json">The JSON string.</param>
    /// <returns>The deserialized object.</returns>
    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to an object.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="json">The JSON string.</param>
    /// <param name="result">The deserialized object if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryDeserialize<T>(string json, out T? result)
    {
        try
        {
            result = Deserialize<T>(json);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Creates a deep clone of an object using JSON serialization.
    /// </summary>
    /// <typeparam name="T">The type of object to clone.</typeparam>
    /// <param name="obj">The object to clone.</param>
    /// <returns>A deep clone of the object.</returns>
    public static T? DeepClone<T>(T obj)
    {
        var json = Serialize(obj);
        return Deserialize<T>(json);
    }
}
