using AlmanacForager.Networking;
using AlmanacForager.SurfaceGate;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

[assembly: ModInfo("The Almanac: Forager", "almanacforager",
    Authors = new string[] { "Lueken Good Design" },
    Description = "Substrate-gated knapping shim, trait-tag system over vanilla flora, preparation/preservation blocks.",
    Version = "0.1.0")]

namespace AlmanacForager;

public class AlmanacForagerModSystem : ModSystem
{
    private const string ChannelName = "almanacforager.gates";
    private SubstrateRecipeFilter? filter;

    public override void Start(ICoreAPI api)
    {
        AlmanacLogger.Info(api, "mod-system", $"loading The Almanac: Forager v0.1.0 (side={api.Side})");

        SubstrateClassifier.Init(api);

        filter = new SubstrateRecipeFilter(api);

        api.Network.RegisterChannel(ChannelName)
            .RegisterMessageType<HardSurfaceGatePacket>();

        AlmanacLogger.Info(api, "mod-system",
            $"network channel '{ChannelName}' registered (filter subscription deferred to AssetsFinalize)");
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        api.Event.PlayerJoin += player =>
        {
            var names = HardSurfaceRecipeIndex.Snapshot();
            api.Network.GetChannel(ChannelName)
                .SendPacket(new HardSurfaceGatePacket { RecipeNames = names }, player);
            AlmanacLogger.Info(api, "mod-system",
                $"sent gate index to player '{player.PlayerName}' ({names.Length} gated recipes)");
        };
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        api.Network.GetChannel(ChannelName)
            .SetMessageHandler<HardSurfaceGatePacket>(packet =>
            {
                AlmanacLogger.Info(api, "mod-system",
                    $"received gate index from server ({packet.RecipeNames.Length} gated recipes)");
                HardSurfaceRecipeIndex.Populate(api, packet.RecipeNames);
            });
    }

    public override void AssetsFinalize(ICoreAPI api)
    {
        if (api.Side == EnumAppSide.Server)
        {
            HardSurfaceRecipeIndex.Build(api);
        }

        if (filter != null)
        {
            api.Event.MatchesRecipe += filter.OnMatchesRecipe;
            AlmanacLogger.Info(api, "mod-system",
                "MatchesRecipe handler subscribed in AssetsFinalize (after vanilla, so our veto wins)");
        }
    }
}
