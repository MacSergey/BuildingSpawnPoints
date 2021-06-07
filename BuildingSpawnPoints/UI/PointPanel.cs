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

        public ushort BuildingId { get; private set; }
        public BuildingSpawnPoint Point { get; private set; }
        private VehicleType NotAdded => BuildingId.GetBuilding().GetPossibleVehicles() & ~Point.VehicleTypes;

        private PointHeaderPanel Header { get; set; }
        private VehicleTypePropertyPanel Vehicle { get; set; }

        public void Init(ushort buildingId, BuildingSpawnPoint point)
        {
            BuildingId = buildingId;
            Point = point;

            StopLayout();

            AddHeader();
            AddVehicleType();
            AddPointType();
            AddPosition();

            StartLayout();

            base.Init();
        }
        public override void DeInit()
        {
            base.DeInit();

            foreach (var component in components.ToArray())
                ComponentPool.Free(component);

            BuildingId = 0;
            Point = null;
            Header = null;
            Vehicle = null;
        }

        private void AddHeader()
        {
            Header = ComponentPool.Get<PointHeaderPanel>(this, nameof(Header));
            InitHeader();
            Header.OnDelete += () => SingletonItem<BuildingSpawnPointsPanel>.Instance.DeletePoint(this);
            Header.OnAddType += AddVehicleType;
        }
        private void AddVehicleType()
        {
            Vehicle = ComponentPool.Get<VehicleTypePropertyPanel>(this);
            Vehicle.AddItems(Point.VehicleTypes);
            Vehicle.OnDelete += DeleteVehicleType;
        }

        private void AddVehicleType(VehicleType type)
        {
            type &= NotAdded;
            Point.VehicleTypes |= type;
            Vehicle.AddItems(type);
            InitHeader();
        }
        private void DeleteVehicleType(VehicleType type)
        {
            Point.VehicleTypes &= ~type;
            InitHeader();
        }
        private void InitHeader() => Header.Init(NotAdded);

        private void AddPointType()
        {
            var type = ComponentPool.Get<PointTypePropertyPanel>(this);
            type.Text = BuildingSpawnPoints.Localize.Property_PointType;
            type.Init();
            type.SelectedObject = Point.Type;
            type.OnSelectObjectChanged += (value) => Point.Type = value;
        }
        private void AddPosition()
        {
            var position = ComponentPool.Get<PointPositionPropertyPanel>(this);
            position.Text = BuildingSpawnPoints.Localize.Property_Position;
            position.WheelTip = true;
            position.Init(0, 2, 3);
            position.Value = (Vector4)Point.Position + new Vector4(0f, 0f, 0f, Point.Angle);
            position.OnValueChanged += (value) =>
                {
                    Point.Position = value;
                    Point.Angle = value.w;
                };
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
    }
    public class PointHeaderPanel : BaseDeletableHeaderPanel<BaseHeaderContent>
    {
        public event Action<VehicleType> OnAddType;

        private SelectVehicleHeaderButton AddTypeButton { get; }
        //private SelectVehicleHeaderButton AddTypeGroupButton { get; }
        public PointHeaderPanel()
        {
            AddTypeButton = Content.AddButton<SelectVehicleHeaderButton>(SpawnPointsTextures.AddVehicle, BuildingSpawnPoints.Localize.Panel_AddVehicle);
            //AddTypeGroupButton = Content.AddButton<SelectVehicleHeaderButton>(SpawnPointsTextures.AddVehicleGroup, BuildingSpawnPoints.Localize.Panel_AddVehicleGroup);

            AddTypeButton.OnSelect += AddType;
            //AddTypeGroupButton.OnSelect += AddType;
        }

        private void AddType(VehicleType type)
        {
            OnAddType?.Invoke(type);
        }

        public void Init(VehicleType notAdded)
        {
            base.Init();
            Fill(notAdded);
        }
        private void Fill(VehicleType notAdded)
        {
            var types = GetGroup<VehicleType>(notAdded).ToArray();
            var typeGroups = new List<VehicleType>();
            if ((VehicleType.Default & notAdded) != VehicleType.None)
                typeGroups.Add(VehicleType.Default);
            typeGroups.AddRange(GetGroup<VehicleTypeGroupA>(notAdded));
            typeGroups.AddRange(GetGroup<VehicleTypeGroupB>(notAdded));
            typeGroups.AddRange(GetGroup<VehicleTypeGroupC>(notAdded));
            typeGroups.AddRange(GetGroup<VehicleTypeGroupD>(notAdded));
            if ((VehicleType.All & notAdded) != VehicleType.None)
                typeGroups.Add(VehicleType.All);

            AddTypeButton.Init(types);
            //AddTypeGroupButton.Init(groups);

            AddTypeButton.isEnabled = types.Length != 0;
            //AddTypeGroupButton.isEnabled = groups.Length != 0;
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
    }
    public class SelectVehicleHeaderButton : BaseHeaderDropDown<VehicleType>
    {
        protected override UITextureAtlas IconAtlas => SpawnPointsTextures.Atlas;

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
    public class PointPositionPropertyPanel : Vector4PropertyPanel
    {
        public PointPositionPropertyPanel()
        {
            Labels[3].text = "A";

            for (var i = 0; i < Dimension; i += 1)
            {
                var field = Fields[i];
                field.Format = BuildingSpawnPoints.Localize.Panel_PositionFormat;
                field.width = 50f;

                if(i == 3)
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
            }
        }
    }
}
