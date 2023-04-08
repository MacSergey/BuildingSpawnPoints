using ColossalFramework;
using ModsCommon;
using ModsCommon.Settings;

namespace BuildingSpawnPoints
{
    public class Settings : BaseSettings<Mod>
    {
        public static SavedBool ShowDebugProperties { get; } = new SavedBool(nameof(ShowDebugProperties), SettingsFile, false);
        public static SavedBool ColorTags { get; } = new SavedBool(nameof(ColorTags), SettingsFile, true);
        public static SavedBool Marker3D { get; } = new SavedBool(nameof(Marker3D), SettingsFile, true);

        protected override void FillSettings()
        {
            base.FillSettings();

            AddLanguage(GeneralTab);

            var generalSection = GeneralTab.AddOptionsSection(CommonLocalize.Settings_General);
            generalSection.AddKeyMappingButton(SpawnPointsTool.ActivationShortcut);
            generalSection.AddKeyMappingButton(SpawnPointsTool.AddPointShortcut);
            generalSection.AddToggle(Localize.Settings_Marker3D, Marker3D);
            generalSection.AddToggle(Localize.Settings_ColorTags, ColorTags);
#if DEBUG
            var otherSection = DebugTab.AddOptionsSection("Properties");
            otherSection.AddToggle("Show debug properties", ShowDebugProperties);
#endif
            AddNotifications(GeneralTab);
        }
    }
}
