using ModsCommon;
using ModsCommon.Utilities;
using UnityEngine;
using static ToolBase;

namespace BuildingSpawnPoints
{
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
                BuildingTool.RenderOverlay(cameraInfo, ref building, CommonColors.Yellow, CommonColors.Yellow);

                var i = 0;
                while (building.m_subBuilding != 0 && i < BuildingManager.MAX_BUILDING_COUNT)
                {
                    building = building.m_subBuilding.GetBuilding();
                    BuildingTool.RenderOverlay(cameraInfo, ref building, CommonColors.Yellow, CommonColors.Yellow);
                    i += 1;
                }
            }
        }
    }
}
