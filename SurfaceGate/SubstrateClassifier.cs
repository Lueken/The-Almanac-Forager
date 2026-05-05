using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace AlmanacForager.SurfaceGate;

public static class SubstrateClassifier
{
    public const string HardRockTagCode = "almanac-hardrock";

    private static TagSet hardRockTag = TagSet.Empty;
    private static bool initialized;

    public static void Init(ICoreAPI api)
    {
        var err = api.CollectibleTagRegistry.TryRegisterAndCreateTagSetAndLogIssues(out hardRockTag, HardRockTagCode);
        initialized = true;
        AlmanacLogger.Info(api, "substrate-gate",
            $"classifier init: tag '{HardRockTagCode}' registry result={err}, isEmpty={hardRockTag.IsEmpty}");
    }

    public static bool IsHardRock(Block? block)
    {
        if (!initialized || block == null) return false;
        return block.Tags.Overlaps(hardRockTag);
    }
}
