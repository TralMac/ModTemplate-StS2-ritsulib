using System.Collections.Concurrent;
using Godot;

namespace RitsuChar.RitsuCharCode.Extensions;

//Mostly utilities to get asset paths inside the RitsuChar folder.
public static class StringExtensions
{
    private static readonly ConcurrentDictionary<string, bool> ExistingResources = new();

    /// <summary>
    /// "relics/my_relic.png" -> "res://RitsuChar/images/relics/my_relic.png"
    /// </summary>
    public static string ImagePath(this string path)
    {
        return $"{MainFile.ResPath}/images/{path}";
    }

    /// <summary>
    /// Returns the image path if the image exists, otherwise the fallback image path
    /// (or null when there is no fallback, which lets RitsuLib use its own placeholder).
    /// </summary>
    public static string? ImagePathOr(this string path, string? fallbackPath)
    {
        var fullPath = path.ImagePath();
        if (ResourceExists(fullPath)) return fullPath;

        return fallbackPath?.ImagePath();
    }

    private static bool ResourceExists(string path)
    {
        return ExistingResources.GetOrAdd(path, static p =>
        {
            var exists = ResourceLoader.Exists(p);
            if (!exists) MainFile.Logger.Info($"Could not find image: {p}");
            return exists;
        });
    }
}
