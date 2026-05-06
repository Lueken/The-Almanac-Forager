using System.Collections.Generic;
using Vintagestory.API.Common;

namespace AlmanacForager.Integration;

/// <summary>
/// Schema for the data file at <c>assets/almanacforager/config/codex-entries.json</c>
/// (the file lives under <c>config/</c> because VS's asset loader only scans known
/// subdirectory categories — <c>codex/</c> is not one):
///   {
///     "herb-mint":          { "latin": "Mentha spicata",   "class": "herb",     "slug": "mint" },
///     "mushroom-reishi":    { "latin": "Ganoderma lucidum","class": "polypore", "slug": "reishi" },
///     "fruitingbush-*-blackberry": { ..., "slug": "blackberry" }
///   }
///
/// JSON keys may contain '*' wildcards for state-in-middle codes (e.g. fruitingbush variants).
/// The <c>slug</c> field is used to derive lang keys for habitat + description so wildcarded
/// entries collapse to a single shared lang slot.
/// </summary>
public sealed class CodexEntryDef
{
    public string Latin { get; set; } = "";
    public string Class { get; set; } = "";
    public string Slug { get; set; } = "";
}

internal static class CodexEntryLoader
{
    public static Dictionary<string, CodexEntryDef> Load(ICoreAPI api)
    {
        var asset = api.Assets.TryGet(new AssetLocation("almanacforager", "config/codex-entries.json"));
        if (asset == null)
        {
            AlmanacLogger.Warn(api, "codex-entries",
                "no config/codex-entries.json found; entries will register without metadata");
            return new Dictionary<string, CodexEntryDef>();
        }

        try
        {
            var data = asset.ToObject<Dictionary<string, CodexEntryDef>>();
            AlmanacLogger.Info(api, "codex-entries",
                $"loaded {data?.Count ?? 0} entry definitions from config/codex-entries.json");
            return data ?? new Dictionary<string, CodexEntryDef>();
        }
        catch (System.Exception ex)
        {
            AlmanacLogger.Error(api, "codex-entries",
                $"failed to parse config/codex-entries.json: {ex.GetType().Name}: {ex.Message}");
            return new Dictionary<string, CodexEntryDef>();
        }
    }

    /// <summary>
    /// Resolves a collectible's path to the matching <see cref="CodexEntryDef"/>:
    ///   1. Exact-match the full code path
    ///   2. Progressive prefix shortening (handles orientation/state suffixes)
    ///   3. Pattern matching (handles state-in-middle codes like fruitingbush)
    /// Returns the matched key (used as suffix base) and the def, or (null, null) if no match.
    /// </summary>
    public static CodexEntryDef? Lookup(Dictionary<string, CodexEntryDef> defs, string codePath)
    {
        if (defs.TryGetValue(codePath, out var m)) return m;

        var parts = codePath.Split('-');
        for (int i = parts.Length - 1; i >= 1; i--)
        {
            var prefix = string.Join("-", parts, 0, i);
            if (defs.TryGetValue(prefix, out m)) return m;
        }

        foreach (var kvp in defs)
        {
            if (kvp.Key.Contains('*') && MatchesPattern(kvp.Key, codePath)) return kvp.Value;
        }

        return null;
    }

    private static bool MatchesPattern(string pattern, string path)
    {
        var pParts = pattern.Split('-');
        var aParts = path.Split('-');
        if (aParts.Length < pParts.Length) return false;
        for (int i = 0; i < pParts.Length; i++)
        {
            if (pParts[i] == "*") continue;
            if (pParts[i] != aParts[i]) return false;
        }
        return true;
    }
}
