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
        private VehicleCategoryPropertyPanel Vehicle { get; set; }
        private PointTypePropertyPanel Type { get; set; }
        private PointPositionPropertyPanel Position { get; set; }
        private BoolPropertyPanel FollowGround { get; set; }
#if DEBUG
        private Vector3PropertyPanel Absolute { get; set; }
#endif
        private VehicleCategory NotAdded => Data.PossibleVehicles & ~Point.Categories.Value;
        public VehicleCategory HoveredVehicleType { get; private set; }

        public void Init(BuildingData data, BuildingSpawnPoint point)
        {
            Data = data;
            Point = point;

            PauseLayout(() =>
            {
                AddHeader();
                AddWarning();
                AddVehicleType();
                AddPointType();
                AddPosition();
                AddFollowFround();
#if DEBUG
                if (Settings.ShowDebugProperties)
                    AddAbsolute();
#endif
                Refresh();
            });

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
            Type = null;
            Position = null;
            FollowGround = null;
#if DEBUG
            Absolute = null;
#endif
            HoveredVehicleType = VehicleCategory.None;
        }

        private void AddHeader()
        {
            Header = ComponentPool.Get<PointHeaderPanel>(this, nameof(Header));
            InitHeader();
            Header.OnDelete += Delete;
            Header.OnAddType += AddVehicleType;
            Header.OnDuplicate += Duplicate;
            Header.OnCopy += Copy;
            Header.OnPaste += Paste;
            Header.OnAppend += Append;
        }
        private void AddWarning()
        {
            Warning = ComponentPool.Get<WarningTextProperty>(this);
            Warning.Init();
            Warning.Text = BuildingSpawnPoints.Localize.Panel_TooFarPoint;
        }
        private void AddVehicleType()
        {
            Vehicle = ComponentPool.Get<VehicleCategoryPropertyPanel>(this);
            Vehicle.AddItems(Point.Categories);
            Vehicle.SetStyle(UIStyle.Default);
            Vehicle.OnRemove += RemoveVehicleType;
            Vehicle.OnHover += HoverVehicleType;
        }

        private void Changed() => OnChanged?.Invoke();
        private void Delete()
        {
            SingletonItem<BuildingSpawnPointsPanel>.Instance.DeletePoint(Point);
            Changed();
        }
        private void AddVehicleType(VehicleCategory type)
        {
            type &= Data.PossibleVehicles;
            Point.Categories.Value |= type;
            Vehicle.AddItems(type);
            InitHeader();

            Changed();
            Refresh();
        }
        private void RemoveVehicleType(VehicleCategory type)
        {
            Point.Categories.Value &= ~type;
            InitHeader();

            Changed();
            Refresh();
        }
        private void HoverVehicleType(VehicleCategory type) => HoveredVehicleType = type;
        private void Duplicate()
        {
            SingletonItem<BuildingSpawnPointsPanel>.Instance.DuplicatePoint(Point);
            Changed();
        }

        public static BuildingSpawnPoint Buffer { get; set; }
        private void Copy() => Buffer = Point.Copy();
        private void Paste() => Paste(false);
        private void Append() => Paste(true);
        private void Paste(bool append)
        {
            if (Buffer != null)
            {
                Point.Type.Value = Buffer.Type;
                Point.Categories.Value = (append ? Point.Categories : VehicleCategory.None) | Buffer.Categories & Data.PossibleVehicles;
                Point.Position.Value = Buffer.Position;
                Point.FollowGround.Value = Buffer.FollowGround;

                Vehicle.SetItems(Point.Categories);
                Type.SelectedObject = Point.Type;
                Position.Value = Point.Position;
                FollowGround.Value = Point.FollowGround;
                InitHeader();

                Changed();
                Refresh();
            }
        }

        private void InitHeader() => Header.Init(NotAdded);

        private void AddPointType()
        {
            Type = ComponentPool.Get<PointTypePropertyPanel>(this);
            Type.Label = BuildingSpawnPoints.Localize.Property_PointType;
            Type.Init();
            Type.SetStyle(UIStyle.Default);
            Type.SelectedObject = Point.Type;
            Type.OnSelectObjectChanged += (value) => Point.Type.Value = value;
        }
        private void AddPosition()
        {
            Position = ComponentPool.Get<PointPositionPropertyPanel>(this);
            Position.Label = BuildingSpawnPoints.Localize.Property_Position;
            Position.WheelTip = true;
            Position.UseWheel = true;
            Position.FieldsWidth = 50f;
            Position.WheelStep = new Vector4(1f, 1f, 1f, 10f);
            for (var i = 0; i < Position.Dimension; i += 1)
            {
                var field = Position[i];

                if (i == 3)
                {
                    field.Format = BuildingSpawnPoints.Localize.Panel_AngleFormat;
                    field.NumberFormat = "0";
                    field.CheckMin = true;
                    field.MinValue = -180;
                    field.CheckMax = true;
                    field.MaxValue = 180;
                    field.CyclicalValue = true;
                }
                else
                {
                    field.Format = BuildingSpawnPoints.Localize.Panel_PositionFormat;
                    field.NumberFormat = "0.#";
                }
            }
            Position.Init(0, 2, 1, 3);
            Position.SetStyle(UIStyle.Default);
            Position.Value = Point.Position;
            Position.OnValueChanged += (value) =>
            {
                Point.Position.Value = value;
                Refresh();
            };
        }
        private void AddFollowFround()
        {
            FollowGround = ComponentPool.Get<BoolPropertyPanel>(this);
            FollowGround.Label = BuildingSpawnPoints.Localize.Property_FollowGround;
            FollowGround.SetStyle(UIStyle.Default);
            FollowGround.Value = Point.FollowGround;
            FollowGround.OnValueChanged += (value) =>
            {
                Point.FollowGround.Value = value;
                Refresh();
            };
        }
#if DEBUG
        private void AddAbsolute()
        {
            Absolute = ComponentPool.Get<Vector3PropertyPanel>(this);
            Absolute.Label = "Absolute";
            Absolute.FieldsWidth = 50f;
            Absolute.Init(0, 1, 2);
            Absolute.SetStyle(UIStyle.Default);
        }
#endif

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

        public void Refresh()
        {
            Position.Value = Point.Position;
            Point.GetAbsolute(ref Data.Id.GetBuilding(), out var absPos, out _);
#if DEBUG
            if (Absolute != null)
                Absolute.Value = absPos;
#endif

            var isCorrect = Point.IsCorrect;
            foreach (var item in Vehicle.Values)
            {
                var service = item.Type.GetService();
                item.IsCorrect = Point.IsServiceCorrect(service);
                item.IsAllCorrect = isCorrect;
            }

            Warning.isVisible = !isCorrect;
        }
    }
    public class PointHeaderPanel : BaseDeletableHeaderPanel<PanelHeaderContent>
    {
        public event Action<VehicleCategory> OnAddType;
        public event Action OnDuplicate;
        public event Action OnCopy;
        public event Action OnPaste;
        public event Action OnAppend;

        private HeaderButtonInfo<CategoryHeaderButton> AddCategoryButton { get; set; }
        private HeaderButtonInfo<ServiceHeaderButton> AddServiceButton { get; set; }
        private HeaderButtonInfo<HeaderButton> AddAllTypesButton { get; set; }
        private HeaderButtonInfo<HeaderButton> PasteButton { get; set; }
        private HeaderButtonInfo<HeaderButton> AppendButton { get; set; }

        public PointHeaderPanel() : base()
        {
            Padding = new RectOffset(5, 10, 0, 0);
        }

        protected override void FillContent()
        {
            AddCategoryButton = new HeaderButtonInfo<CategoryHeaderButton>("Add category", HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.AddVehicleHeaderButton, BuildingSpawnPoints.Localize.Panel_AddVehicle);
            AddCategoryButton.Button.OnSelectObject += AddCategory;
            Content.AddButton(AddCategoryButton);

            AddServiceButton = new HeaderButtonInfo<ServiceHeaderButton>("Add service", HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.AddVehicleGroupHeaderButton, BuildingSpawnPoints.Localize.Panel_AddVehicleGroup);
            AddServiceButton.Button.OnSelectObject += AddService;
            Content.AddButton(AddServiceButton);

            AddAllTypesButton = new HeaderButtonInfo<HeaderButton>("Add all", HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.AddAllVehiclesHeaderButton, BuildingSpawnPoints.Localize.Panel_AddAllVehicle, AddAllTypes);
            Content.AddButton(AddAllTypesButton);

            Content.AddButton(new HeaderButtonInfo<HeaderButton>("Diplicate", HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.DuplicateHeaderButton, BuildingSpawnPoints.Localize.Panel_DuplicatePoint, DuplicateClick));

            Content.AddButton(new HeaderButtonInfo<HeaderButton>("Copy", HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.CopyHeaderButton, BuildingSpawnPoints.Localize.Panel_CopyPoint, CopyClick));

            PasteButton = new HeaderButtonInfo<HeaderButton>("Paste", HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.PasteHeaderButton, BuildingSpawnPoints.Localize.Panel_PastePointReplace, PasteClick);
            Content.AddButton(PasteButton);

            AppendButton = new HeaderButtonInfo<HeaderButton>("Append", HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.AppendHeaderButton, BuildingSpawnPoints.Localize.Panel_PastePointAppend, AppendClick);
            Content.AddButton(AppendButton);
        }

        private void AddCategory(VehicleCategory category) => OnAddType?.Invoke(category);
        private void AddService(VehicleService service) => OnAddType?.Invoke(service.GetAttr<CategoryAttribute, VehicleService>().Category);

        public void Init(VehicleCategory notAdded)
        {
            base.Init();
            Fill(notAdded);
        }
        public override void DeInit()
        {
            base.DeInit();

            OnAddType = null;
            OnDuplicate = null;
            OnCopy = null;
            OnPaste = null;
        }
        private void Fill(VehicleCategory notAdded)
        {
            var categories = notAdded.GetEnumValues().IsItem().ToArray();
            var services = categories.Select(c => c.GetAttr<ServiceAttribute, VehicleCategory>().Service).GetEnum().GetEnumValues().IsItem().ToArray();

            AddCategoryButton.Button.Init(categories);
            AddServiceButton.Button.Init(services);

            AddCategoryButton.Enable = categories.Length != 0;
            AddServiceButton.Enable = services.Length != 0;
            AddAllTypesButton.Enable = categories.Length != 0;
        }

        private void DuplicateClick() => OnDuplicate?.Invoke();
        private void AddAllTypes() => AddCategory(VehicleCategory.All);
        private void CopyClick() => OnCopy?.Invoke();
        private void PasteClick() => OnPaste?.Invoke();
        private void AppendClick() => OnAppend?.Invoke();
    }

    public abstract class SelectHeaderButton<EnumType, EntityType, PopupType> : ObjectDropDown<EnumType, EntityType, PopupType>, IHeaderButton
        where EnumType : Enum
        where EntityType : SelectEntity<EnumType>
        where PopupType : SelectPopup<EnumType, EntityType>
    {
        protected override Func<EnumType, bool> Selector => null;
        protected override Func<EnumType, EnumType, int> Sorter => null;

        protected override IEnumerable<EnumType> Objects => ObjectList;
        protected List<EnumType> ObjectList { get; } = new List<EnumType>();

        public SelectHeaderButton() : base()
        {
            BgAtlas = CommonTextures.Atlas;
            BgSprites = new SpriteSet(string.Empty, CommonTextures.HeaderHover, CommonTextures.HeaderHover, CommonTextures.HeaderHover, string.Empty);
        }
        public void SetSize(int buttonSize, int iconSize)
        {
            size = new Vector2(buttonSize, buttonSize);
            minimumSize = size;
            TextPadding = new RectOffset(iconSize + 5, 5, 5, 0);
        }
        public void SetIcon(UITextureAtlas atlas, string sprite)
        {
            FgAtlas = atlas ?? TextureHelper.InGameAtlas;
            FgSprites = sprite;
        }
        public void Init(EnumType[] categories)
        {
            ObjectList.Clear();
            ObjectList.AddRange(categories);
        }
        protected override void InitPopup()
        {
            Popup.MaximumSize = new Vector2(0, 700f);
            Popup.AutoWidth = true;
            base.InitPopup();
        }

        protected override void SetPopupStyle()
        {
            Popup.PopupDefaultStyle(20f);
            Popup.PopupStyle = UIStyle.Default.DropDown;
        }
    }
    public class SelectPopup<EnumType, EntityType> : ObjectPopup<EnumType, EntityType>
        where EnumType : Enum
        where EntityType : SelectEntity<EnumType>
    {
        protected override void SetEntityStyle(EntityType entity)
        {
            entity.TextHorizontalAlignment = UIHorizontalAlignment.Left;
            entity.TextPadding = new RectOffset(8, 8, 3, 0);
            entity.textScale = 0.8f;
            entity.EntityStyle = UIStyle.Default.DropDown;
        }
    }
    public class SelectEntity<EnumType> : PopupEntity<EnumType>
        where EnumType : Enum
    {
        public override void SetObject(int index, EnumType value, bool selected)
        {
            base.SetObject(index, value, selected);
            text = value.Description<EnumType, Mod>();
        }
    }

    public class CategoryHeaderButton : SelectHeaderButton<VehicleCategory, CategoryHeaderButton.CategoryEntity, CategoryHeaderButton.CategoryPopup>
    {
        public class CategoryPopup : SelectPopup<VehicleCategory, CategoryEntity> { }
        public class CategoryEntity : SelectEntity<VehicleCategory> { }
    }
    public class ServiceHeaderButton : SelectHeaderButton<VehicleService, ServiceHeaderButton.ServiceEntity, ServiceHeaderButton.ServicePopup>
    {
        public class ServicePopup : SelectPopup<VehicleService, ServiceEntity> { }
        public class ServiceEntity : SelectEntity<VehicleService> { }
    }


    public class PointTypePropertyPanel : EnumMultyPropertyPanel<PointType, PointTypePropertyPanel.PointTypeSegmented>
    {
        protected override bool IsEqual(PointType first, PointType second) => first == second;
        protected override string GetDescription(PointType value) => value.Description<PointType, Mod>();

        public override void SetStyle(ControlStyle style)
        {
            Selector.SegmentedStyle = style.Segmented;
        }

        public class PointTypeSegmented : UIMultySegmented<PointType> { }
    }
    public class PointPositionPropertyPanel : Vector4PropertyPanel
    {
        protected override string GetName(int index) => index switch
        {
            0 => "X",
            1 => "H",
            2 => "Y",
            3 => "A",
            _ => "?",
        };
    }
}
