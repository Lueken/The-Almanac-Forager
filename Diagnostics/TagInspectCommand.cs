using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace AlmanacForager.Diagnostics;

public static class TagInspectCommand
{
    public static void Register(ICoreClientAPI capi)
    {
        capi.ChatCommands.Create("almanac")
            .WithDescription("The Almanac diagnostics")
            .BeginSubCommand("tags")
                .WithDescription("Print tags of the looked-at block, or fall back to the held item")
                .HandleWith(_ => Inspect(capi))
            .EndSubCommand();
    }

    private static TextCommandResult Inspect(ICoreClientAPI capi)
    {
        var sb = new StringBuilder();

        var sel = capi.World.Player.CurrentBlockSelection;
        if (sel?.Position != null)
        {
            var block = capi.World.BlockAccessor.GetBlock(sel.Position);
            if (block != null && block.Code?.Path != "air")
            {
                sb.AppendLine($"Block at {sel.Position}:");
                sb.AppendLine($"  code: {block.Code}");
                sb.AppendLine($"  tags: {ResolveTags(capi, block.Tags)}");
                return TextCommandResult.Success(sb.ToString());
            }
        }

        var heldStack = capi.World.Player.InventoryManager.ActiveHotbarSlot?.Itemstack;
        if (heldStack?.Collectible != null)
        {
            var c = heldStack.Collectible;
            sb.AppendLine($"Held item:");
            sb.AppendLine($"  code: {c.Code}");
            sb.AppendLine($"  tags: {ResolveTags(capi, c.Tags)}");
            return TextCommandResult.Success(sb.ToString());
        }

        return TextCommandResult.Success("Nothing to inspect — look at a block, or hold an item in the active hotbar slot.");
    }

    private static string ResolveTags(ICoreClientAPI capi, TagSet tags)
    {
        if (tags.IsEmpty) return "(none)";
        var names = capi.CollectibleTagRegistry.SlowEnumerateTagNames(tags).ToArray();
        return names.Length == 0 ? $"(unresolved handles: {tags})" : string.Join(", ", names);
    }
}
