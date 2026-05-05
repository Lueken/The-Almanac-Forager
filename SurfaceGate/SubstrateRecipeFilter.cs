using Vintagestory.API.Common;

namespace AlmanacForager.SurfaceGate;

public class SubstrateRecipeFilter
{
    private readonly ICoreAPI api;

    public SubstrateRecipeFilter(ICoreAPI api)
    {
        this.api = api;
    }

    public bool OnMatchesRecipe(IPlayer player, IRecipeBase recipe, ItemSlot[] ingredients)
    {
        if (!HardSurfaceRecipeIndex.RequiresHardSurface(recipe)) return true;

        var surfacePos = player.CurrentBlockSelection?.Position;
        if (surfacePos == null)
        {
            AlmanacLogger.Debug(api, "substrate-gate",
                $"VETO recipe='{recipe.Name}' reason=no-block-selection");
            return false;
        }

        var substratePos = surfacePos.DownCopy();
        var substrateBlock = api.World.BlockAccessor.GetBlock(substratePos);
        bool pass = SubstrateClassifier.IsHardRock(substrateBlock);

        AlmanacLogger.Debug(api, "substrate-gate",
            $"{(pass ? "PASS" : "VETO")} recipe='{recipe.Name}' " +
            $"substratePos={substratePos} substrateBlock='{substrateBlock?.Code}'");

        return pass;
    }
}
