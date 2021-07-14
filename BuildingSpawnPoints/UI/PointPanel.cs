using BuildingSpawnPoints.Utilities;
using ColossalFramework.UI;
using ModsCommon;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BuildingSpawnPoints.UI
{
    public class PointPanel : PropertyGroupPanel
    {
        public event Action<PointPanel, UIMouseEventParameter> OnEnter;
        public event Action<PointPanel, UIMouseEventParameter> OnLeave;
        public event Action OnChanged;

        public BuildingData Data { get; private set; }
        public BuildingSpawnPoint Point { get; private set; }

        private PointHeaderPanel Header { get; set; }
        private WarningTextProperty Warning { get; set; }
        private VehicleTypePropertyPanel Vehicle { get; set; }
#if DEBUG
        private Vector3PropertyPanel Absolute { get; set; }
#endif
        private VehicleType NotAdded => Data.PossibleVehicles & ~Point.VehicleTypes.Value;
        private Dictionary<VehicleTypeGroup, PathUnit.Position> Groups { get; } = new Dictionary<VehicleTypeGroup, PathUnit.Position>();
        private VehicleType SelectedType { get; set; }

        public void Init(BuildingData data, BuildingSpawnPoint point)
        {
            Data = data;
            Point = point;

            StopLayout();

            AddHeader();
            AddWarning();
            AddVehicleType();
            AddPointType();
            AddPosition();
#if DEBUG
            AddAbsolute();
#endif
            Refresh();

            StartLayout();

            base.Init();
        }
        public override void DeInit()
        {
            base.DeInit();

            foreach (var component in components.ToArray())
                ComponentPool.Free(component);

            Data = null;
            Point = null;
            Header = null;
            Warning = null;
            Vehicle = null;
#if DEBUG
            Absolute = null;
#endif
            SelectedType = VehicleType.None;
            Groups.Clear();
        }

        private void AddHeader()
        {
            Header = ComponentPool.Get<PointHeaderPanel>(this, nameof(Header));
            InitHeader();
            Header.OnDelete += Delete;
            Header.OnAddType += AddVehicleType;
            Header.OnDuplicate += Duplicate;
        }
        private void AddWarning()
        {
            Warning = ComponentPool.Get<WarningTextProperty>(this);
            Warning.Init();
            Warning.Text = BuildingSpawnPoints.Localize.Panel_TooFarPoint;
        }
        private void AddVehicleType()
        {
            Vehicle = ComponentPool.Get<VehicleTypePropertyPanel>(this);
            Vehicle.AddItems(Point.VehicleTypes);
            Vehicle.OnDelete += DeleteVehicleType;
            Vehicle.OnSelect += SelectVehicleType;
        }

        private void Changed() => OnChanged?.Invoke();
        private void Delete()
        {
            SingletonItem<BuildingSpawnPointsPanel>.Instance.DeletePoint(this);
            Changed();
        }
        private void AddVehicleType(VehicleType type)
        {
            type &= Data.PossibleVehicles;
            Point.VehicleTypes.Value |= type;
            Vehicle.AddItems(type);
            InitHeader();

            Changed();
            Refresh();
        }
        private void DeleteVehicleType(VehicleType type)
        {
            Point.VehicleTypes.Value &= ~type;
            InitHeader();

            Changed();
            Refresh();
        }
        private void SelectVehicleType(VehicleType type) => SelectedType = type;
        private void Duplicate()
        {
            SingletonItem<BuildingSpawnPointsPanel>.Instance.DuplicatePoint(this);
            Changed();
        }
        private void InitHeader() => Header.Init(NotAdded);

        private void AddPointType()
        {
            var type = ComponentPool.Get<PointTypePropertyPanel>(this);
            type.Text = BuildingSpawnPoints.Localize.Property_PointType;
            type.Init();
            type.SelectedObject = Point.Type;
            type.OnSelectObjectChanged += (value) => Point.Type.Value = value;
        }
        private void AddPosition()
        {
            var position = ComponentPool.Get<PointPositionPropertyPanel>(this);
            position.Text = BuildingSpawnPoints.Localize.Property_Position;
            position.WheelTip = true;
            position.Init(0, 2, 1, 3);
            position.Value = Point.Position;
            position.OnValueChanged += OnPositionChanged;
        }
#if DEBUG
        private void AddAbsolute()
        {
            Absolute = ComponentPool.Get<Vector3PropertyPanel>(this);
            Absolute.Text = "Absolute";
            Absolute.Init(0, 1, 2);
            Absolute.FieldsWidth = 50f;
        }
#endif
        private void OnPositionChanged(Vector4 value)
        {
            Point.Position.Value = value;
            Refresh();
        }

        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            base.OnMouseEnter(p);
            OnEnter?.Invoke(this, p);
        }
        protected override void OnMouseLeave(UIMouseEventParameter p)
        {
            base.OnMouseLeave(p);
            OnLeave?.Invoke(this, p);
        }

        private void Refresh()
        {
            Groups.Clear();

            Point.GetAbsolute(ref Data.Id.GetBuilding(), out var position, out _);
#if DEBUG
            Absolute.Value = position;
#endif
            foreach (var group in EnumExtension.GetEnumValues<VehicleTypeGroup>())
            {
                if(((ulong)group & (ulong)Point.VehicleTypes.Value) != 0)
                {
                    var laneData = VehicleLaneData.Get(group);
                    if (PathManager.FindPathPosition(position, laneData.Service, laneData.Lane, laneData.Type, false, false, laneData.Distance, out var pathPos))
                    {
                        Groups[group] = pathPos;
                    }
                }
            }

            foreach(var item in Vehicle)
            {
                var group = item.Type.GetGroup();
                item.IsCorrect = group == VehicleTypeGroup.None || Groups.ContainsKey(group);
            }

            Warning.isVisible = Vehicle.Any(i => !i.IsCorrect);
        }

        public void Render(RenderManager.CameraInfo cameraInfo)
        {
            Point.GetAbsolute(ref Data.Id.GetBuilding(), out var position, out _);
            position.RenderCircle(new OverlayData(cameraInfo), 1.5f, 0f);

            if (SelectedType == VehicleType.None)
                return;

            var group = SelectedType.GetGroup();
            if (group == VehicleTypeGroup.None)
                return;

            if(!Groups.TryGetValue(group, out var pathPos))
            {
                var laneData = VehicleLaneData.Get(group);
                position.RenderCircle(new OverlayData(cameraInfo) { Width = laneData.Distance * 2f });
                return;
            }

            var segment = pathPos.m_segment.GetSegment();
            var lanes = segment.GetLaneIds().ToArray();
            if (pathPos.m_lane > lanes.Length - 1)
                return;

            var lane = lanes[pathPos.m_lane].GetLane();
            lane.m_bezier.RenderBezier(new OverlayData(cameraInfo) { Width = segment.Info.m_lanes[pathPos.m_lane].m_width, Cut = true });

            var lanePos = lane.m_bezier.Position(pathPos.m_offset / 255f);
            new StraightTrajectory(position, lanePos).Render(new OverlayData(cameraInfo));
        }
    }
    public class PointHeaderPanel : BaseDeletableHeaderPanel<HeaderContent>
    {
        public event Action<VehicleType> OnAddType;
        public event Action OnDuplicate;

        private HeaderButtonInfo<SelectVehicleHeaderButton> AddTypeButton { get; set; }
        private HeaderButtonInfo<HeaderButton> AddAllTypesButton { get; set; }

        public PointHeaderPanel()
        {
            AddTypeButton = new HeaderButtonInfo<SelectVehicleHeaderButton>(HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.AddVehicle, BuildingSpawnPoints.Localize.Panel_AddVehicle);
            AddTypeButton.Button.OnSelect += AddType;
            Content.AddButton(AddTypeButton);

            AddAllTypesButton = new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.AddVehicleGroup, BuildingSpawnPoints.Localize.Panel_AddAllVehicle, AddAllTypes);
            Content.AddButton(AddAllTypesButton);

            Content.AddButton(new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.Duplicate, BuildingSpawnPoints.Localize.Panel_DuplicatePoint, DuplicateClick));
        }

        private void AddType(VehicleType type) => OnAddType?.Invoke(type);

        public void Init(VehicleType notAdded)
        {
            base.Init();
            Fill(notAdded);
        }
        public override void DeInit()
        {
            base.DeInit();

            OnAddType = null;
            OnDuplicate = null;
        }
        private void Fill(VehicleType notAdded)
        {
            var types = GetGroup<VehicleType>(notAdded).ToArray();
            AddTypeButton.Button.Init(types);
            AddTypeButton.Enable = types.Length != 0;
            AddAllTypesButton.Enable = types.Length != 0;
        }

        private IEnumerable<VehicleType> GetGroup<Type>(VehicleType notAdded)
            where Type : Enum
        {
            foreach (var type in EnumExtension.GetEnumValues<Type>(v => v.IsItem()))
            {
                if (((ulong)(object)type & (ulong)notAdded) != 0ul)
                    yield return (VehicleType)(object)type;
            }
        }

        private void DuplicateClick() => OnDuplicate?.Invoke();
        private void AddAllTypes() => AddType(VehicleType.All);
    }
    public class SelectVehicleHeaderButton : BaseHeaderDropDown<VehicleType>
    {
        public SelectVehicleHeaderButton()
        {
            MinListWidth = 100f;
        }
        protected override string GetItemLabel(VehicleType item) => item.Description<VehicleType, Mod>();
    }
    public class PointTypePropertyPanel : EnumMultyPropertyPanel<PointType, PointTypePropertyPanel.PointTypeSegmented>
    {
        protected override bool IsEqual(PointType first, PointType second) => first == second;
        protected override string GetDescription(PointType value) => value.Description<PointType, Mod>();

        public class PointTypeSegmented : UIMultySegmented<PointType> { }
    }
    public class PointPositionPropertyPanel : BaseVectorPropertyPanel<Vector4>
    {
        protected override uint Dimension => 4;

        public PointPositionPropertyPanel()
        {
            for (var i = 0; i < Dimension; i += 1)
            {
                var field = Fields[i];
                field.width = 50f;

                if (i == 3)
                {
                    field.Format = BuildingSpawnPoints.Localize.Panel_AngleFormat;
                    field.NumberFormat = "0";
                    field.UseWheel = true;
                    field.WheelStep = 10f;
                    field.WheelTip = true;
                    field.CheckMin = true;
                    field.MinValue = -180;
                    field.CheckMax = true;
                    field.MaxValue = 180;
                    field.CyclicalValue = true;
                }
                else
                    field.Format = BuildingSpawnPoints.Localize.Panel_PositionFormat;
            }
        }

        protected override string GetName(int index) => index switch
        {
            0 => "X",
            1 => "H",
            2 => "Y",
            3 => "A",
            _ => "?",
        };
        protected override float Get(ref Vector4 vector, int index) => vector[index];
        protected override void Set(ref Vector4 vector, int index, float value) => vector[index] = value;
    }
}
