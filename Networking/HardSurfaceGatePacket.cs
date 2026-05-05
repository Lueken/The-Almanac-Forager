using ProtoBuf;

namespace AlmanacForager.Networking;

[ProtoContract]
public class HardSurfaceGatePacket
{
    [ProtoMember(1)]
    public string[] RecipeNames { get; set; } = System.Array.Empty<string>();
}
