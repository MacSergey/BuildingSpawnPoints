using ModsCommon;
using ModsCommon.Utilities;
using UnityEngine;
using static ToolBase;

namespace BuildingSpawnPoints
{
    public class EditSpawnPointsMode : SpawnPointsToolMode
    {
        public override ToolModeType Type => ToolModeType.Edit;

        public BuildingSpawnPoint HoverPoint { get; protected set; }
        public bool HoverOverlay { get; protected set; }

        public override string GetToolInfo()
        {
            if (HoverPoint != null)
            {
                var correct = string.Empty;
                var incorrect = string.Empty;
                var correctLength = 0;
                var incorrectLength = 0;
                foreach (var category in HoverPoint.SelectedCategories)
                {
                    if (HoverPoint.IsServiceCorrect(category.GetService()))
                        AddCategory(category, ref correct, ref correctLength);
                    else
                        AddCategory(category, ref incorrect, ref incorrectLength);
                }

                if (!string.IsNullOrEmpty(correct) && !string.IsNullOrEmpty(incorrect))
                    return $"{Localize.Tool_InfoCorrect.AddActionColor()}\n{correct.AddActionColor()}\n\n{Localize.Tool_InfoIncorrect.AddWarningColor()}\n{incorrect.AddWarningColor()}";
                else if (!string.IsNullOrEmpty(correct))
                    return $"{Localize.Tool_InfoCorrect.AddActionColor()}\n{correct.AddActionColor()}";
                else if (!string.IsNullOrEmpty(incorrect))
                    return $"{Localize.Tool_InfoIncorrect.AddWarningColor()}\n{incorrect.AddWarningColor()}";
                else
                    return Localize.Tool_InfoEmptyPoint;

                static void AddCategory(VehicleCategory category, ref string list, ref int length)
                {
                    var name = category.Description<VehicleCategory, Mod>();

                    if (length == 0)
                    {
                        list = name;
                        length = name.Length;
                    }
                    else if (length + name.Length <= 50)
                    {
                        list = $"{list}, {name}";
                        length += name.Length + 2;
                    }
                    else
                    {
                        list = $"{list}\n{name}";
                        length = name.Length;
                    }
                }
            }
            else
                return Localize.Tool_InfoDragPoint + '\n' + string.Format(Localize.Tool_InfoPointHeight, LocalizeExtension.Shift.AddInfoColor());
        }
        public override void OnToolUpdate()
        {
            if (SingletonTool<SpawnPointsTool>.Instance.MouseRayValid)
            {
                ref var building = ref Tool.Data.Id.GetBuilding();
                foreach (var point in Tool.Data.Points)
                {
                    point.GetAbsolute(ref building, out var position, out _, PositionType.Final);
                    var bounds = new Bounds(position, new Vector3(2f, 2f, 2f));
                    if (bounds.IntersectRay(SingletonTool<SpawnPointsTool>.Instance.MouseRay))
                    {
                        HoverPoint = point;
                        HoverOverlay = false;
                        return;
                    }
                }

                foreach (var point in Tool.Data.Points)
                {
                    point.GetAbsolute(ref building, out var position, out _, PositionType.OnGround);
                    var bounds = new Bounds(position, new Vector3(2f, 2f, 2f));
                    if (bounds.IntersectRay(SingletonTool<SpawnPointsTool>.Instance.MouseRay))
                    {
                        HoverPoint = point;
                        HoverOverlay = true;
                        return;
                    }
                }
            }

            HoverPoint = null;
        }
        public override void OnMouseDrag(Event e)
        {
            if (HoverPoint != null)
                Tool.SetMode(ToolModeType.Drag);
        }

        public override void OnSecondaryMouseClicked()
        {
            Tool.SetData(null);
            Tool.SetMode(ToolModeType.Select);
        }
        public override void RenderGeometry(RenderManager.CameraInfo cameraInfo)
        {
            foreach (var point in Tool.Data.Points)
            {
                if (point == Tool.Panel.HoverPoint)
                    Tool.Panel.RenderGeometry(cameraInfo);
                else
                    point.RenderGeometry(cameraInfo, point == HoverPoint);
            }
        }
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            foreach (var point in Tool.Data.Points)
            {
                if (point == Tool.Panel.HoverPoint)
                    Tool.Panel.Render(cameraInfo);
                else
                    point.Render(cameraInfo, point == HoverPoint);
            }
        }
    }
}
