using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace ComplexTweaks.Tweaks;

public partial class MaxGCRank : Tweak {
    public override string Name => "Enforce Expert Delivery";
    public override string Description => "Forces the expert delivery window to show regardless of rank. Only in effect if you do not have expert delivery unlocked.";

    [AddressHook<PlayerState>(nameof(PlayerState.MemberFunctionPointers.GetGrandCompanyRank))]
    public unsafe byte GetGrandCompanyRank(PlayerState* thisPtr) {
        var ret = GetGrandCompanyRankHook.Original(thisPtr);
        return ret < 6 ? (byte)17 : ret;
    }
}
