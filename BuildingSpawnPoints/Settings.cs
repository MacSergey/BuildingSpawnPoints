using ColossalFramework;
using ModsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModsCommon.Settings;

namespace BuildingSpawnPoints
{
    public class Settings : BaseSettings<Mod>
    {
        public static SavedBool ShowDebugProperties { get; } = new SavedBool(nameof(ShowDebugProperties), SettingsFile, false);

        protected override void FillSettings()
        {
            base.FillSettings();

            AddLanguage(GeneralTab);

            var generalSection = GeneralTab.AddOptionsSection(CommonLocalize.Settings_General);
            generalSection.AddKeyMappingButton(SpawnPointsTool.ActivationShortcut);
#if DEBUG
            var otherSection = DebugTab.AddOptionsSection("Properties");
            otherSection.AddToggle("Show debug properties", ShowDebugProperties);
#endif
            AddNotifications(GeneralTab);
        }
    }
}
