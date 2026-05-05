using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;

namespace AlmanacForager.SurfaceGate;

public static class HardSurfaceRecipeIndex
{
    private static readonly HashSet<string> requiresHardSurface = new();

    public static void Build(ICoreAPI api)
    {
        requiresHardSurface.Clear();

        var files = api.Assets.GetMany<JToken>(api.Logger, "recipes/knapping");
        int scanned = 0, gated = 0;

        foreach (var (loc, token) in files)
        {
            void Inspect(JToken t)
            {
                scanned++;
                var marker = t["attributes"]?["almanac"]?["requiresHardSurface"];
                if (marker == null || !marker.Value<bool>()) return;

                var name = t["name"]?.Value<string>();
                var key = string.IsNullOrEmpty(name)
                    ? loc.ToShortString()
                    : (name!.Contains(':') ? name : $"{loc.Domain}:{name}");
                requiresHardSurface.Add(key.ToLowerInvariant());
                gated++;
                AlmanacLogger.Info(api, "recipe-loader",
                    $"gate registered: file='{loc}' key='{key.ToLowerInvariant()}'");
            }

            if (token is JArray arr) { foreach (var t in arr) Inspect(t); }
            else if (token is JObject) Inspect(token);
        }

        AlmanacLogger.Info(api, "recipe-loader",
            $"hard-surface recipe index built: {gated}/{scanned} knapping recipes gated");
    }

    public static bool RequiresHardSurface(IRecipeBase recipe)
    {
        var name = recipe.Name?.ToShortString()?.ToLowerInvariant();
        return name != null && requiresHardSurface.Contains(name);
    }

    public static string[] Snapshot() => requiresHardSurface.ToArray();

    public static void Populate(ICoreAPI api, string[] names)
    {
        requiresHardSurface.Clear();
        foreach (var n in names) requiresHardSurface.Add(n);
        AlmanacLogger.Info(api, "recipe-loader",
            $"index populated from network: {names.Length} gated recipe(s)");
    }
}
