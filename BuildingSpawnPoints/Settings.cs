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
        protected override void FillSettings()
        {
            base.FillSettings();

            AddLanguage(GeneralTab);


            var generalGroup = GeneralTab.AddGroup(CommonLocalize.Settings_General);

            var keymappings = AddKeyMappingPanel(generalGroup);
            keymappings.AddKeymapping(SpawnPointsTool.ActivationShortcut);

            AddNotifications(GeneralTab);
        }
    }
}
