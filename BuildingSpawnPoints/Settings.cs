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
            AddLanguage(GeneralTab);


            var generalGroup = GeneralTab.AddGroup(CommonLocalize.Settings_General);

            var keymappings = AddKeyMappingPanel(generalGroup);
            keymappings.AddKeymapping(SpawnPointsTool.ActivationShortcut);

            AddNotifications(GeneralTab);

            var reportGroup = GeneralTab.AddGroup("Report");
            AddHarmonyReport(reportGroup);
#if DEBUG
            var debugTab = CreateTab("Debug");
            AddDebug(debugTab);
#endif
        }
#if DEBUG

        private void AddDebug(UIAdvancedHelper helper)
        {

        }
#endif
    }
}
