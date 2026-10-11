using ComplexTweaks.UI;
using Dalamud.Game.Gui.Toast;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace ComplexTweaks.Tweaks;

public partial class DebugTools : Tweak<DebugToolsConfiguration> {
    [CommandHandler("/tpclick", "Teleport to your mouse location on click while CTRL is held.", nameof(Config.EnableTPClick))]
    private void OnTeleportClick(string command, string arguments) {
        tpActive ^= true;
        if (tpActive)
            WindowsService.Get().AddWindow(new MousePositionOverlay());
        else
            WindowsService.Get().RemoveWindow<MousePositionOverlay>();
        IToastGui.Get().ShowNormal($"TPClick {(tpActive ? "Enabled" : "Disabled")}", new ToastOptions() { Speed = ToastSpeed.Fast });
    }

    [CommandHandler("/noclip", "Enable NoClip", nameof(Config.EnableNoClip))]
    private void OnNoClip(string command, string arguments) {
        if (GameMain.IsInPvPInstance()) return;
        ncActive ^= true;
        Config.NoClipSpeed = float.TryParse(arguments, out var speed) ? speed : Config.NoClipSpeed;
    }

    [CommandHandler(["/move", "/speed"], "Modify your movement speed", nameof(Config.EnableMoveSpeed))]
    private void OnMoveSpeed(string command, string arguments) {
        if (GameMain.IsInPvPInstance() || IObjectTable.Get().LocalPlayer is not { } player) return;
        player.Speed = float.TryParse(arguments, out var speed) ? speed : 1.0f;
    }

    [CommandHandler("/ada", "Call actions directly.", nameof(Config.EnableDirectActions))]
    private unsafe void OnDirectAction(string command, string arguments) {
        if (GameMain.IsInPvPInstance()) return;
        try {
            var args = arguments.Split(' ');
            var actionType = ParseActionType(args[0]);
            var actionID = uint.Parse(args[1]);
            ActionManager.Instance()->UseActionLocation(actionType, actionID);
        }
        catch (Exception e) { Error(e, "Error executing direct action"); }
    }

    private static ActionType ParseActionType(string input) {
        if (Enum.TryParse(input, true, out ActionType result))
            return result;

        if (byte.TryParse(input, out var intValue))
            if (Enum.IsDefined(typeof(ActionType), intValue))
                return (ActionType)intValue;

        throw new ArgumentException("Invalid ActionType", nameof(input));
    }

    [CommandHandler("/tpmarker", "Teleport to a given marker", nameof(Config.EnableTPMarker))]
    private unsafe void OnTeleportMarker(string command, string arguments) {
        if (GameMain.IsInPvPInstance() || IObjectTable.Get().LocalPlayer is not { } player) return;
        if (int.TryParse(arguments, out var i)) {
            var m = MarkingController.Instance()->FieldMarkers[i];
            Vector3? markerPos = m.Active ? new(m.X / 1000.0f, m.Y / 1000.0f, m.Z / 1000.0f) : null;
            if (markerPos is { } pos)
                player.SetPosition(pos);
        }
    }

    [CommandHandler("/tpoff", "Teleport from your current position, offset by arguments", nameof(Config.EnableTPOffset))]
    private void OnTeleportOffset(string command, string arguments) {
        if (GameMain.IsInPvPInstance() || IObjectTable.Get().LocalPlayer is not { } player) return;
        if (arguments.TryParseVector3(out var v))
            player.SetPosition(player.Position + v);
    }

    [CommandHandler("/tpabs", "Teleport to a given absolute position", nameof(Config.EnableTPAbsolute))]
    private void OnTeleportAbsolute(string command, string arguments) {
        if (GameMain.IsInPvPInstance() || IObjectTable.Get().LocalPlayer is not { } player) return;
        if (arguments.TryParseVector3(out var v))
            player.SetPosition(v);
    }
}

