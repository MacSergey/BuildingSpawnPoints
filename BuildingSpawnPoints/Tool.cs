using ColossalFramework;
using ColossalFramework.Math;
using ModsCommon;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ToolBase;

namespace BuildingSpawnPoints
{
    public class SpawnPointsTool : BaseTool<Mod, SpawnPointsTool, ToolModeType>
    {
        public static SpawnPointsShortcut ActivationShortcut { get; } = new SpawnPointsShortcut(nameof(ActivationShortcut), "Activate", SavedInputKey.Encode(KeyCode.P, true, false, false));

        public override Shortcut Activation => ActivationShortcut;
        protected override bool ShowToolTip => true;
        protected override IToolMode DefaultMode => ToolModes[ToolModeType.Select];
        public SpawnPointsPanel Panel => SingletonItem<SpawnPointsPanel>.Instance;

        public BuildingData Data { get; private set; }

        protected override IEnumerable<IToolMode<ToolModeType>> GetModes()
        {
            yield return CreateToolMode<SelectBuildingMode>();
            yield return CreateToolMode<EditSpawnPointsMode>();
        }

        protected override void InitProcess()
        {
            base.InitProcess();
            SpawnPointsPanel.CreatePanel();
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
    public class SpawnPointsToolThreadingExtension : BaseThreadingExtension<SpawnPointsTool> { }
    public class SpawnPointsToolLoadingExtension : BaseToolLoadingExtension<SpawnPointsTool> { }

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
                Tool.SetData(SingletonManager<Manager>.Instance[HoverBuildingId, true]);
                Tool.SetDefaultMode();
            }
        }
        public override void OnMouseUp(Event e) => OnPrimaryMouseClicked(e);
        public override void OnSecondaryMouseClicked() => Tool.Disable();

        public override string GetToolInfo()
        {
            if (IsHoverBuilding)
                return $"Building #{HoverBuildingId}";
            else
                return "Select building";
        }
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            if (IsHoverBuilding)
                BuildingTool.RenderOverlay(cameraInfo, ref HoverBuildingId.GetBuilding(), Colors.Blue, Colors.Red);
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
            var overlayData = new OverlayData(cameraInfo) { Color = Colors.Orange, Width = 2f };
            var building = Tool.Data.Id.GetBuilding();
            foreach (var point in Tool.Data.Points)
            {
                var position = point.GetAbsolutePosition(ref building);
                position.RenderCircle(overlayData);
            }
        }
    }
}
