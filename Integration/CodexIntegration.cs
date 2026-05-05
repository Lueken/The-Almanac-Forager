using AlmanacCodex;
using AlmanacCodex.Registry;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace AlmanacForager.Integration;

public static class CodexIntegration
{
    public const string ModId = "almanacforager";
    public const string ProcessKnap = "knap";

    private static readonly string[] AlmanacTagCodes =
    {
        "almanac-aromatic", "almanac-medicinal", "almanac-decorative", "almanac-toxic",
        "almanac-culinary", "almanac-psychoactive", "almanac-fibrous",
        "almanac-fruity", "almanac-sweet", "almanac-acidic",
        "almanac-starchy", "almanac-leafy", "almanac-seedy",
    };

    public static void RegisterEntriesAndProcesses(ICoreAPI api)
    {
        CodexAPI.RegisterProcess(api, new ProcessDefinition(
            code: ProcessKnap,
            displayKey: "almanacforager:process-knap",
            ownerModId: ModId));

        var registry = api.CollectibleTagRegistry;
        var err = registry.TryCreateTagSetAndLogIssues(out var almanacTags, AlmanacTagCodes);
        AlmanacLogger.Info(api, "codex-integration",
            $"built lookup TagSet for {AlmanacTagCodes.Length} known almanac-* tags (result={err})");

        int registered = 0;
        foreach (var collectible in api.World.Collectibles)
        {
            if (collectible?.Code == null) continue;
            if (collectible.Tags.IsEmpty) continue;
            if (!collectible.Tags.Overlaps(almanacTags)) continue;
            CodexAPI.RegisterEntry(api, new AlmanacEntry(collectible.Code, ModId));
            registered++;
        }

        AlmanacLogger.Info(api, "codex-integration",
            $"registered {registered} collectibles with The Almanac: Codex");
    }
}
