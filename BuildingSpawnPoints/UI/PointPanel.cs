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
        private VehicleTypePropertyPanel Vehicle { get; set; }

        private VehicleType NotAdded => Data.PossibleVehicles & ~Point.VehicleTypes.Value;

        public void Init(BuildingData data, BuildingSpawnPoint point)
        {
            Data = data;
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

            Data = null;
            Point = null;
            Header = null;
            Vehicle = null;
        }

        private void AddHeader()
        {
            Header = ComponentPool.Get<PointHeaderPanel>(this, nameof(Header));
            InitHeader();
            Header.OnDelete += Delete;
            Header.OnAddType += AddVehicleType;
            Header.OnDuplicate += Duplicate;
        }
        private void AddVehicleType()
        {
            Vehicle = ComponentPool.Get<VehicleTypePropertyPanel>(this);
            Vehicle.AddItems(Point.VehicleTypes);
            Vehicle.OnDelete += DeleteVehicleType;
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
        }
        private void DeleteVehicleType(VehicleType type)
        {
            Point.VehicleTypes.Value &= ~type;
            InitHeader();

            Changed();
        }
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
            position.OnValueChanged += (value) => Point.Position.Value = value;
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
        public event Action OnDuplicate;

        private HeaderButtonInfo<SelectVehicleHeaderButton> AddTypeButton { get; set; }
        private HeaderButtonInfo<BasePanelHeaderButton> AddAllTypesButton { get; set; }

        public PointHeaderPanel()
        {
            AddTypeButton = new HeaderButtonInfo<SelectVehicleHeaderButton>(HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.AddVehicle, BuildingSpawnPoints.Localize.Panel_AddVehicle);
            AddTypeButton.Button.OnSelect += AddType;
            Content.AddButton(AddTypeButton);

            AddAllTypesButton = new HeaderButtonInfo<BasePanelHeaderButton>(HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.AddVehicleGroup, BuildingSpawnPoints.Localize.Panel_AddAllVehicle, AddAllTypes);
            Content.AddButton(AddAllTypesButton);

            Content.AddButton(new HeaderButtonInfo<BasePanelHeaderButton>(HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.Duplicate, BuildingSpawnPoints.Localize.Panel_DuplicatePoint, DuplicateClick));
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
