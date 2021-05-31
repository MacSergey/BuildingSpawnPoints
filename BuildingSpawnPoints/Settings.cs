using ColossalFramework;
using ModsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ModsCommon.SettingsHelper;

namespace BuildingSpawnPoints
{
    public class Settings : BaseSettings<Mod>
    {
        protected override void OnSettingsUI()
        {
            //AddLanguage(GeneralTab);


            var generalGroup = GeneralTab.AddGroup("General");

            var keymappings = AddKeyMappingPanel(generalGroup);
            keymappings.AddKeymapping(SpawnPointsTool.ActivationShortcut);

#if DEBUG
            var debugTab = CreateTab("Debug");
            AddDebug(debugTab);
#endif
        }
#if DEBUG

        public static SavedFloat ZOffset { get; } = new SavedFloat(nameof(ZOffset), SettingsFile, 2f, false);

        private void AddDebug(UIAdvancedHelper helper)
        {
            var group = helper.AddGroup("Debug");

            AddFloatField(group, "Z Offset", ZOffset, 2f);
        }
#endif
    }
}
