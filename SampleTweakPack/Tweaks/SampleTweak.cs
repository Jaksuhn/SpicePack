namespace ComplexTweaks.Tweaks;

public class SampleConfiguration {
    [BoolConfig(Label = "Chat message")]
    public bool Announce = true;
}

public partial class SampleTweak : Tweak<SampleConfiguration> {
    public override string Name => "Sample";
    public override string Description => "Template tweak. Replace this class with your own.";

    public override void OnEnable() {
        if (Config.Announce)
            ModuleMessage("Sample tweak enabled.");
    }

    [CommandHandler("/sampletweak", "Help text", nameof(Config.Announce))]
    private void OnCommand(string command, string arguments)
        => ModuleMessage($"Echo: {arguments}");
}
