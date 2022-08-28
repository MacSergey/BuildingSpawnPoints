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

            Data.FromXml(Buffer);
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

                var buildings = BuildingManager.instance.m_buildings.m_buffer;
                for (ushort i = 0; i < buildings.Length; i += 1)
                {
                    if (i == Data.Id)
                        continue;

                    if (buildings[i].Info == info && buildings[i].m_flags.IsSet(Building.Flags.Created) && SingletonManager<Manager>.Instance[i, Options.Create] is BuildingData data)
                        data.FromXml(config);
                }

                return true;
            }
        }

        public static new bool RayCast(RaycastInput input, out RaycastOutput output) => ToolBase.RayCast(input, out output);
    }
    public abstract class SpawnPointsToolMode : BaseToolMode<SpawnPointsTool>, IToolMode<ToolModeType>, IToolModePanel
    {
        public abstract ToolModeType Type { get; }
        public virtual bool ShowPanel => true;
    }
    public enum ToolModeType
    {
        None = 0,
        Select = 1,
        Edit = 2,
    }
    public class SpawnPointsShortcut : ModShortcut<Mod>
    {
        public SpawnPointsShortcut(string name, string labelKey, InputKey key, Action action = null) : base(name, labelKey, key, action) { }
    }
    public class SpawnPointsToolThreadingExtension : BaseUUIThreadingExtension<SpawnPointsTool> { }
    public class SpawnPointsToolLoadingExtension : BaseUUIToolLoadingExtension<SpawnPointsTool> { }

    public class SelectBuildingMode : SpawnPointsToolMode
    {
        public override ToolModeType Type => ToolModeType.Select;
        public override bool ShowPanel => false;

        private ushort HoverBuildingId { get; set; } = 0;
        private bool IsHoverBuilding => HoverBuildingId != 0;

        public override void OnToolUpdate()
        {
            if (Tool.MouseRayValid)
            {
                var input = new RaycastInput(Tool.MouseRay, Camera.main.farClipPlane)
                {
                    m_ignoreTerrain = true,
                    m_ignoreBuildingFlags = Building.Flags.None,
                };
                input.m_buildingService.m_itemLayers = ItemClass.Layer.Default;

                if (SpawnPointsTool.RayCast(input, out var output))
                {
                    if (output.m_building.GetBuilding().m_flags.IsSet(Building.Flags.Untouchable))
                        HoverBuildingId = Building.FindParentBuilding(output.m_building);
                    else
                        HoverBuildingId = output.m_building;
                    return;
                }
            }

            HoverBuildingId = 0;
        }

        public override void OnPrimaryMouseClicked(Event e)
        {
            if (IsHoverBuilding)
            {
                Tool.SetData(SingletonManager<Manager>.Instance[HoverBuildingId, Options.Default]);
                Tool.SetDefaultMode();
            }
        }
        public override void OnMouseUp(Event e) => OnPrimaryMouseClicked(e);
        public override void OnSecondaryMouseClicked() => Tool.Disable();

        public override string GetToolInfo()
        {
            if (IsHoverBuilding)
                return string.Format(Localize.Tool_InfoHoverBuilding, HoverBuildingId);
            else
                return Localize.Tool_InfoSelectBuilding;
        }
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            if (IsHoverBuilding)
            {
                var building = HoverBuildingId.GetBuilding();
                BuildingTool.RenderOverlay(cameraInfo, ref building, Colors.Yellow, Colors.Yellow);

                var i = 0;
                while (building.m_subBuilding != 0 && i < BuildingManager.MAX_BUILDING_COUNT)
                {
                    building = building.m_subBuilding.GetBuilding();
                    BuildingTool.RenderOverlay(cameraInfo, ref building, Colors.Yellow, Colors.Yellow);
                    i += 1;
                }
            }
        }
    }
    public class EditSpawnPointsMode : SpawnPointsToolMode
    {
        public override ToolModeType Type => ToolModeType.Edit;

        public override void OnSecondaryMouseClicked()
        {
            Tool.SetData(null);
            Tool.SetMode(ToolModeType.Select);
        }
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            Tool.Panel.Render(cameraInfo);

            var building = Tool.Data.Id.GetBuilding();
            foreach (var point in Tool.Data.Points)
            {
                var color = point.VehicleTypes == VehicleType.None || point.Type == PointType.None ? Colors.Gray192 : point.Type.Value switch
                {
                    PointType.Spawn => Colors.Green,
                    PointType.Unspawn => Colors.Red,
                    //PointType.Middle => Colors.Purple,
                    PointType.Both /*or PointType.All*/ => Colors.Orange,
                    _ => Colors.Gray192,
                };

                point.GetAbsolute(ref building, out var position, out var target);
                position.RenderCircle(new OverlayData(cameraInfo) { Color = color }, 2f, 1.5f);

                var direction = target - position;
                new StraightTrajectory(position + direction, position + direction * 2f).Render(new OverlayData(cameraInfo) { Color = color });

                //var str = new StraightTrajectory(position + direction, position - direction);
                //Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(cameraInfo, color, str.Trajectory.GetBezier(), 0f, 0f, 0f, position.y - 1f, position.y + 1f, true, true);
            }
        }
    }
}
