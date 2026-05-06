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
            ownerModId: ModId)
        {
            HintKey = "almanacforager:codex-process-hint.knap",
            FlavorKey = "almanacforager:codex-process-flavor.knap",
        });

        var defs = CodexEntryLoader.Load(api);

        var registry = api.CollectibleTagRegistry;
        var err = registry.TryCreateTagSetAndLogIssues(out var almanacTags, AlmanacTagCodes);
        AlmanacLogger.Info(api, "codex-integration",
            $"built lookup TagSet for {AlmanacTagCodes.Length} known almanac-* tags (result={err})");

        int registered = 0;
        int withMeta = 0;
        foreach (var collectible in api.World.Collectibles)
        {
            if (collectible?.Code == null) continue;
            if (collectible.Tags.IsEmpty) continue;
            if (!collectible.Tags.Overlaps(almanacTags)) continue;

            var meta = CodexEntryLoader.Lookup(defs, collectible.Code.Path);
            if (meta != null) withMeta++;

            string? classKey = meta != null && !string.IsNullOrEmpty(meta.Class)
                ? $"almanacforager:codex-class.{meta.Class}"
                : null;
            string? habitatKey = meta != null && !string.IsNullOrEmpty(meta.Slug)
                ? $"almanacforager:codex-habitat.{meta.Slug}"
                : null;
            string? descKey = meta != null && !string.IsNullOrEmpty(meta.Slug)
                ? $"almanacforager:codex-description.{meta.Slug}"
                : null;

            CodexAPI.RegisterEntry(api, new AlmanacEntry(collectible.Code, ModId)
            {
                LatinName = !string.IsNullOrEmpty(meta?.Latin) ? meta.Latin : null,
                ClassificationKey = classKey,
                HabitatKey = habitatKey,
                DescriptionKey = descKey,
            });
            registered++;
        }

        AlmanacLogger.Info(api, "codex-integration",
            $"registered {registered} collectibles with The Almanac: Codex ({withMeta} with metadata)");
    }
}
