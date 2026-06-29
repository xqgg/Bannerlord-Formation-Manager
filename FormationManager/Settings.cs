using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace FormationManager
{
    public class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => "FormationManager";
        public override string DisplayName => "Stop Shuffling, You Fools - Formation Manager";
        public override string FolderName => "FormationManager";
        public override string FormatType => "json";

        [SettingPropertyBool(
            "Enable Formation Manager",
            RequireRestart = false,
            HintText = "When enabled, troops are assigned to formations according to your configured rules. Disable to restore vanilla behaviour.",
            Order = 0)]
        [SettingPropertyGroup("General")]
        public bool ModEnabled { get; set; } = true;
    }
}
