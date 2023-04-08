using ModsCommon;
using ModsCommon.Utilities;
using UnityEngine;
using static ToolBase;

namespace BuildingSpawnPoints
{
    public class DragSpawnPointsMode : SpawnPointsToolMode
    {
        public override ToolModeType Type => ToolModeType.Drag;
        public BuildingSpawnPoint DragPoint { get; set; } = null;
        public bool DragOverlay { get; protected set; }

        private Vector3 CachedPosition { get; set; }
        private int WasModifierPressed { get; set; }

        protected override void Reset(IToolMode prevMode)
        {
            if (prevMode is EditSpawnPointsMode editMode)
            {
                DragPoint = editMode.HoverPoint;
                DragOverlay = editMode.HoverOverlay;
            }
            else
            {
                DragPoint = null;
                DragOverlay = false;
            }
            WasModifierPressed = 0;
            Cache();
        }
        private void Cache()
        {
            ref var building = ref Tool.Data.Id.GetBuilding();
            DragPoint.GetAbsolute(ref building, out var position, out _, PositionType.Final);
            CachedPosition = position;
        }

        public override void OnToolUpdate()
        {
            var isPressed = 0;
            if (Utility.AltIsPressed)
                isPressed += 1;
            if (Utility.CtrlIsPressed)
                isPressed += 2;
            if (Utility.ShiftIsPressed)
                isPressed += 4;

            if (isPressed != WasModifierPressed && !DragOverlay)
            {
                Cache();
            }
            WasModifierPressed = isPressed;
        }
        public override void OnToolGUI(Event e)
        {
            if (!Input.GetMouseButton(0))
                Exit();
        }
        public override void OnMouseDrag(Event e)
        {
            Vector3 position;
            if (DragOverlay)
            {
                position = Tool.MouseWorldPosition;
                position.y = CachedPosition.y;
            }
            else if (Utility.OnlyShiftIsPressed)
            {
                position = CachedPosition;
                var plane = new Plane(Tool.CameraDirection, CachedPosition);
                if (plane.Raycast(Tool.MouseRay, out var rayT))
                    position.y = Mathf.Clamp((Tool.MouseRay.origin + Tool.MouseRay.direction * rayT).y, -1024f, 1024f);
            }
            else
            {
                position = Tool.Ray.GetRayPosition(CachedPosition.y, out _);
            }

            ref var building = ref Tool.Data.Id.GetBuilding();
            DragPoint.SetAbsolute(ref building, position);

            Tool.Panel.SelectPoint(DragPoint);
        }
        public override void OnPrimaryMouseClicked(Event e) => Exit();
        public override void OnMouseUp(Event e) => Exit();
        private void Exit() => Tool.SetDefaultMode();
        public override void RenderGeometry(RenderManager.CameraInfo cameraInfo)
        {
            DragPoint.RenderGeometry(cameraInfo, true);
        }
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            DragPoint.Render(cameraInfo, true);
        }
        public override bool GetExtraInfo(out string text, out Color color, out float size, out Vector3 position, out Vector3 direction)
        {
            ref var building = ref Tool.Data.Id.GetBuilding();
            DragPoint.GetAbsolute(ref building, out var absPos, out var target);
            direction = absPos - target;
            position = absPos + direction * 2f;

            size = 2f;
            color = CommonColors.Yellow;

            if (Utility.OnlyShiftIsPressed)
            {
                var h = absPos.y - BuildingSpawnPoint.GetHeightWithWater(absPos);
                var hSign = h < 0 ? "-" : h > 0 ? "+" : "";
                text = $"{hSign}{Mathf.Abs(h):0.0}";
            }
            else
            {
                var relPos = DragPoint.Position.Value;
                var xSign = relPos.x < 0 ? "-" : relPos.x > 0 ? "+" : "";
                var zSign = relPos.z < 0 ? "-" : relPos.z > 0 ? "+" : "";
                text = $"X: {xSign}{Mathf.Abs(relPos.x):0.0}\nY: {zSign}{Mathf.Abs(relPos.z):0.0}";
            }

            return true;
        }
    }
}
