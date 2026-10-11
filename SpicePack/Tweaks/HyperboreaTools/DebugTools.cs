using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Interop;
using Lumina.Excel.Sheets;

namespace ComplexTweaks.Tweaks;

public class DebugToolsConfiguration {
    [BoolConfig] public bool AutoVoidIslandRest = false;
    [BoolConfig] public bool EnableTPClick = false;
    [BoolConfig] public bool EnableNoClip = false;

    [FloatConfig(DependsOn = nameof(EnableNoClip))]
    public float NoClipSpeed = 0.05f;

    [BoolConfig] public bool EnableMoveSpeed = false;
    [BoolConfig] public bool EnableDirectActions = false;
    [BoolConfig] public bool EnableTPMarker = false;
    [BoolConfig] public bool EnableTPOffset = false;
    [BoolConfig] public bool EnableTPAbsolute = false;
}

public partial class DebugTools : Tweak<DebugToolsConfiguration> {
    public override string Name => "Debug Tools";
    public override string Description => "Debug tools for use in hyperborea/firewall";

    public override void OnEnable() {
        _keys = Sheets.ConfigKey.Where(x => x.RowId is >= 12 and <= 18).ToDictionary(x => x.Label.ToString(), x => x);
        IAddonLifecycle.Get().RegisterListener(AddonEvent.PostSetup, "MJICraftSchedule", OnSetup);
        IClientState.Get().EnterPvP += OnEnterPvP;
        IFramework.Get().Update += OnUpdate;
    }

    public override void OnDisable() {
        IAddonLifecycle.Get().UnregisterListener(OnSetup);
        IClientState.Get().EnterPvP -= OnEnterPvP;
        IFramework.Get().Update -= OnUpdate;
    }

    private unsafe void OnUpdate(IFramework framework) {
        if (IObjectTable.Get().LocalPlayer is not { } player || ICondition.Get().IsUnavailable()) return;

        ShowMouseOverlay = false;

        if (tpActive) {
            if (!Framework.Instance()->WindowInactive && SeVirtualKey.CONTROL.IsDown() && Utils.IsClickingInGameWorld()) {
                ShowMouseOverlay = true;
                var pos = ImGui.GetMousePos();
                if (IGameGui.Get().ScreenToWorld(pos, out var res)) {
                    if (MouseButtonFlags.LBUTTON.IsPressed()) {
                        if (!IsLButtonPressed)
                            player.SetPosition(res);
                        IsLButtonPressed = true;
                    }
                    else
                        IsLButtonPressed = false;
                }
            }
        }

        if (ncActive && !Framework.Instance()->WindowInactive) {
            var cx = player.Position.X;
            var cy = player.Position.Z;
            var angle = MathF.PI - CameraManager.Instance()->GetActiveCamera()->DirH;
            if (_keys["JUMP"].IsHeldRaw())
                player.SetPosition((player.Position.X, player.Position.Y + Config.NoClipSpeed, player.Position.Z).ToVector3());
            if (SeVirtualKey.SHIFT.IsDown())
                player.SetPosition((player.Position.X, player.Position.Y - Config.NoClipSpeed, player.Position.Z).ToVector3());
            if (_keys["MOVE_FORE"].IsHeldRaw())
                player.SetPosition(player.Position.AddZ(Config.NoClipSpeed).RotatePoint(cx, cy, angle));
            if (_keys["MOVE_BACK"].IsHeldRaw())
                player.SetPosition(player.Position.AddZ(-Config.NoClipSpeed).RotatePoint(cx, cy, angle));
            if (_keys["MOVE_LEFT"].IsHeldRaw() || _keys["MOVE_STRIFE_L"].IsHeldRaw())
                player.SetPosition(player.Position.AddX(Config.NoClipSpeed).RotatePoint(cx, cy, angle));
            if (_keys["MOVE_RIGHT"].IsHeldRaw() || _keys["MOVE_STRIFE_R"].IsHeldRaw())
                player.SetPosition(player.Position.AddX(-Config.NoClipSpeed).RotatePoint(cx, cy, angle));
        }
    }

    public override void OnConfigChange(string fieldName) {
        if (fieldName == nameof(Config.EnableTPClick) && !Config.EnableTPClick) {
            tpActive = false;
            ShowMouseOverlay = false;
        }

        if (fieldName == nameof(Config.EnableNoClip) && !Config.EnableNoClip)
            ncActive = false;
    }

    private unsafe void OnSetup(AddonEvent type, AddonArgs args) {
        if (!Config.AutoVoidIslandRest) return;
        if (AgentMJICraftSchedule.Instance()->Data->RestCycles.Hex() != 8321u) {
            Debug($"Setting rest: {8321u:X}");
            AgentMJICraftSchedule.Instance()->Data->NewRestCycles = 8321u;
            var eventData = stackalloc int[] { 0, 0, 0 };
            var atkvalues = new Span<AtkValue>([new() { Type = AtkValueType.Int, Int = 0 }]);
            AgentMJICraftSchedule.Instance()->AgentInterface.ReceiveEvent((AtkValue*)eventData, atkvalues.GetPointer(0), (uint)atkvalues.Length, 5); // 5 = eventKind
        }
    }

    // prevent entering pvp with debug options enabled
    private void OnEnterPvP() {
        IObjectTable.Get().LocalPlayer.Speed = 1.0f;
        tpActive = false;
        ncActive = false;
        ShowMouseOverlay = false;
    }

    public static bool ShowMouseOverlay;
    private bool IsLButtonPressed;
    private bool tpActive;
    private bool ncActive;
    private Dictionary<string, ConfigKey> _keys = null!;
}
