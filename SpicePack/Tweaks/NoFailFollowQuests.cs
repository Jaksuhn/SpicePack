namespace ComplexTweaks.Tweaks;

public partial class NoFailFollowQuests : Tweak {
    public override string Name => "Easier Follow Quests";
    public override string Description => "Prevents being seen during follow quests (you can still be too far away).";

    [SigHook("E8 ?? ?? ?? ?? 84 C0 74 0F 8B 53 30")] // from atmo
    private bool FollowTargetRecast(nint a1, nint a2, nint a3, nint a4, nint a5, nint a6) => false;
}
