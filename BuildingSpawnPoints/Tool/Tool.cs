using BuildingSpawnPoints.UI;
using BuildingSpawnPoints.Utilities;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using ModsCommon;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using static ToolBase;

namespace BuildingSpawnPoints
{
    public class SpawnPointsTool : BaseTool<Mod, SpawnPointsTool, ToolModeType>
    {
        public static SpawnPointsShortcut ActivationShortcut { get; } = new SpawnPointsShortcut(nameof(ActivationShortcut), nameof(CommonLocalize.Settings_ShortcutActivateTool), SavedInputKey.Encode(KeyCode.P, true, false, false));

        public static SpawnPointsShortcut AddPointShortcut { get; } = new SpawnPointsShortcut(nameof(AddPointShortcut), nameof(Localize.Settings_ShortcutAddNewPoint), SavedInputKey.Encode(KeyCode.A, true, true, false), () => SingletonTool<SpawnPointsTool>.Instance.Panel.AddPoint(), ToolModeType.Edit);

        public override IEnumerable<Shortcut> Shortcuts
        {
            get
            {
                yield return AddPointShortcut;
            }
        }

        public override Shortcut Activation => ActivationShortcut;
        protected override IToolMode DefaultMode => ToolModes[ToolModeType.Select];
        public BuildingSpawnPointsPanel Panel => SingletonItem<BuildingSpawnPointsPanel>.Instance;

        public BuildingData Data { get; private set; }

        private XElement Buffer { get; set; }
        public bool IsBufferEmpty => Buffer == null;

        protected override UITextureAtlas UUIAtlas => SpawnPointsTextures.Atlas;
        protected override string UUINormalSprite => SpawnPointsTextures.UUIButtonNormal;
        protected override string UUIHoveredSprite => SpawnPointsTextures.UUIButtonHovered;
        protected override string UUIPressedSprite => SpawnPointsTextures.UUIButtonPressed;
        protected override string UUIDisabledSprite => string.Empty;

        protected override IEnumerable<IToolMode<ToolModeType>> GetModes()
        {
            yield return CreateToolMode<SelectBuildingMode>();
            yield return CreateToolMode<EditSpawnPointsMode>();
            yield return CreateToolMode<DragSpawnPointsMode>();
        }

        protected override void InitProcess()
        {
            base.InitProcess();
            BuildingSpawnPointsPanel.CreatePanel();
        }

        public void SetDefaultMode() => SetMode(ToolModeType.Edit);
        protected override void SetModeNow(IToolMode mode)
        {
            base.SetModeNow(mode);
            Panel.Active = (Mode as SpawnPointsToolMode)?.ShowPanel == true;
        }
        public void SetData(BuildingData data)
        {
            Data = data;
            Panel.SetData(Data);
        }

        public void Copy()
        {
            SingletonMod<Mod>.Logger.Debug($"Copy data");
            Buffer = Data.ToXml();
            Panel?.RefreshHeader();
        }
        public void Paste()
        {
            SingletonMod<Mod>.Logger.Debug($"Paste data");

            if (Buffer == null)
                return;

            Data.FromXml(SingletonMod<Mod>.Version, Buffer);
            Panel.RefreshPanel();
        }
        public void ResetToDefault()
        {
            SingletonMod<Mod>.Logger.Debug($"Reset to default");

            Data.ResetToDefault();
            Panel.RefreshPanel();
        }
        public void ApplyToAll()
        {
            SingletonMod<Mod>.Logger.Debug($"Apply to all");

            var messageBox = MessageBox.Show<YesNoMessageBox>();
            messageBox.CaptionText = Localize.Tool_ApplyToAllCaption;
            messageBox.MessageText = Localize.Tool_ApplyToAllMessage;
            messageBox.OnButton1Click = Apply;

            bool Apply()
            {
                var config = Data.ToXml();
                var info = Data.Id.GetBuilding().Info;
                var version = SingletonMod<Mod>.Version;

                var buildings = BuildingManager.instance.m_buildings.m_buffer;
                for (ushort i = 0; i < buildings.Length; i += 1)
                {
                    if (i == Data.Id)
                        continue;

                    if (buildings[i].Info == info && buildings[i].m_flags.IsSet(Building.Flags.Created) && SingletonManager<Manager>.Instance[i, Options.Create] is BuildingData data)
                        data.FromXml(version, config);
                }

                return true;
            }
        }

        public static new bool RayCast(RaycastInput input, out RaycastOutput output) => ToolBase.RayCast(input, out output);
    }
}
