using ColossalFramework.UI;
using ModsCommon;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BuildingSpawnPoints
{
    public class SpawnPointsPanel : ToolPanel<Mod, SpawnPointsTool, SpawnPointsPanel>
    {
        private float Width => 400f;

        private PanelHeader Header { get; set; }
        private AdvancedScrollablePanel ContentPanel { get; set; }

        private AddPointButton AddButton { get; set; }

        public BuildingData Data { get; private set; }

        public override void Awake()
        {
            base.Awake();

            atlas = TextureHelper.InGameAtlas;
            backgroundSprite = "MenuPanel2";
            name = nameof(SpawnPointsPanel);

            CreateHeader();
            CreateContent();

            minimumSize = GetSize(200);
        }
        public override void Start()
        {
            base.Start();
            SetDefaulSize();
        }
        private void CreateHeader()
        {
            Header = AddUIComponent<PanelHeader>();
            Header.relativePosition = new Vector2(0, 0);
            Header.Target = parent;
            Header.Init(HeaderHeight);
        }
        private void CreateContent()
        {
            ContentPanel = AddUIComponent<AdvancedScrollablePanel>();
            ContentPanel.relativePosition = new Vector2(0, HeaderHeight);
            ContentPanel.Content.autoLayoutPadding = new RectOffset(10, 10, 10, 10);
            ContentPanel.atlas = TextureHelper.InGameAtlas;
            ContentPanel.backgroundSprite = "UnlockingItemBackground";
            ContentPanel.name = nameof(ContentPanel);
        }
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (Header != null)
                Header.width = width;
            if (ContentPanel != null)
                ContentPanel.size = size - new Vector2(0, Header.height);

            MakePixelPerfect();
        }
        private void SetDefaulSize()
        {
            SingletonMod<Mod>.Logger.Debug($"Set default panel size");
            size = GetSize(400);
        }
        private Vector2 GetSize(float additional) => new Vector2(Width, Header.height + additional);

        public void SetData(BuildingData data)
        {
            if ((Data = data) != null)
                SetPanel();
            else
                ResetPanel();
        }
        private void SetPanel()
        {
            ResetPanel();

            ContentPanel.Content.StopLayout();

            Header.Text = string.Format(BuildingSpawnPoints.Localize.Panel_Title, Data.Id);
            AddAddButton();

            foreach (var point in Data.Points)
                AddPointPanel(point);

            ContentPanel.Content.StartLayout();
        }
        private void ResetPanel()
        {
            ContentPanel.Content.StopLayout();

            foreach (var component in ContentPanel.Content.components.ToArray())
                ComponentPool.Free(component);

            ContentPanel.Content.StartLayout();
        }

        private void AddAddButton()
        {
            AddButton = ComponentPool.Get<AddPointButton>(ContentPanel.Content);
            AddButton.Text = BuildingSpawnPoints.Localize.Panel_AddPoint;
            AddButton.Init();
            AddButton.OnButtonClick += AddPoint;
        }
        private void AddPointPanel(BuildingSpawnPoint point)
        {
            var pointPanel = ComponentPool.Get<SpawnPointPanel>(ContentPanel.Content);
            pointPanel.Init(point);

            AddButton.zOrder = -1;
        }

        private void AddPoint()
        {
            var newPoint = Data.AddPoint();
            AddPointPanel(newPoint);

            ContentPanel.Content.ScrollToBottom();
        }

        public void DeletePoint(SpawnPointPanel pointPanel)
        {
            Data.DeletePoint(pointPanel.Point);
            ComponentPool.Free(pointPanel);
        }
    }

    public class PanelHeader : HeaderMoveablePanel<BaseHeaderContent>
    {
        protected override float DefaultHeight => 40f;
    }
    public class AddPointButton : ButtonPanel
    {
        public AddPointButton()
        {
            Button.textScale = 1f;
        }
        protected override void SetSize() => Button.size = size;
    }

    public class SpawnPointPanel : PropertyGroupPanel
    {
        public BuildingSpawnPoint Point { get; private set; }

        private PointHeaderPanel Header { get; set; }

        public void Init(BuildingSpawnPoint point)
        {
            Point = point;

            StopLayout();

            AddHeader();
            AddVehicleType();
            AddPointType();
            AddPosition();
            AddAngle();

            StartLayout();

            base.Init();
        }
        public override void DeInit()
        {
            base.DeInit();

            foreach (var component in components.ToArray())
                ComponentPool.Free(component);

            Point = null;
            Header = null;
        }

        private void AddHeader()
        {
            Header = ComponentPool.Get<PointHeaderPanel>(this, nameof(Header));
            Header.Init();
            Header.OnDelete += () => SingletonItem<SpawnPointsPanel>.Instance.DeletePoint(this);
        }
        private void AddVehicleType()
        {
            var vehicle = ComponentPool.Get<VehicleTypePropertyPanel>(this);
            vehicle.Init(Point.VehicleType);
        }
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
            var position = ComponentPool.Get<Vector3PropertyPanel>(this);
            position.Text = BuildingSpawnPoints.Localize.Property_Position;
            position.WheelTip = true;
            position.Init(0, 2);
            position.Value = Point.Position;
            position.OnValueChanged += (value) => Point.Position = value;
        }
        private void AddAngle()
        {
            var angle = ComponentPool.Get<FloatPropertyPanel>(this);
            angle.Text = BuildingSpawnPoints.Localize.Property_Angle;
            angle.UseWheel = true;
            angle.WheelStep = 1f;
            angle.WheelTip = true;
            angle.CheckMin = true;
            angle.MinValue = -180;
            angle.CheckMax = true;
            angle.MaxValue = 180;
            angle.CyclicalValue = true;
            angle.Init();
            angle.Value = Point.Angle;
            angle.OnValueChanged += (float value) => Point.Angle = value;
        }
    }
    public class PointHeaderPanel : BaseDeletableHeaderPanel<BaseHeaderContent>
    {

    }
    public class PointTypePropertyPanel : EnumMultyPropertyPanel<PointType, PointTypePropertyPanel.PointTypeSegmented>
    {
        protected override bool IsEqual(PointType first, PointType second) => first == second;
        protected override string GetDescription(PointType value) => value.Description<PointType, Mod>();

        public class PointTypeSegmented : UIMultySegmented<PointType> { }
    }
    public class VehicleTypePropertyPanel : EditorItem, IReusable
    {
        bool IReusable.InCache { get; set; }

        private Dictionary<VehicleType, VehicleItem> Items = new Dictionary<VehicleType, VehicleItem>();
        private float Padding => 5f;

        public void Init(VehicleType types)
        {
            foreach (var type in EnumExtension.GetEnumValues<VehicleType>(t => t.IsItem() && (t & types) != VehicleType.None))
                AddItem(type);

            FitItems();
        }
        void IReusable.DeInit()
        {
            foreach (var component in components.ToArray())
                ComponentPool.Free(component);

            Items.Clear();
        }

        private void AddItem(VehicleType type)
        {
            var item = ComponentPool.Get<VehicleItem>(this);
            item.Init(type);
            Items.Add(type, item);
        }
        public void AddType(VehicleType type)
        {
            if (!Items.ContainsKey(type))
                AddItem(type);
        }


        private void FitItems()
        {
            var prev = default(VehicleItem);

            foreach (var item in Items.Values)
            {
                if (prev == null)
                    item.relativePosition = new Vector2(Padding, Padding);
                else if (prev.relativePosition.x + prev.width + item.width + Padding * 2 < width)
                    item.relativePosition = prev.relativePosition + new Vector3(prev.width + Padding, 0f);
                else
                    item.relativePosition = new Vector2(Padding, prev.relativePosition.y + prev.height + Padding);

                prev = item;
            }

            if (prev != null)
                height = prev.relativePosition.y + prev.height + Padding;
            else
                height = 30f;
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            FitItems();
        }
    }
    public class VehicleItem : UIAutoLayoutPanel, IReusable
    {
        public event Action OnDelete;

        bool IReusable.InCache { get; set; }

        private CustomUILabel Label { get; }
        private CustomUIButton Button { get; }

        public VehicleType Type { get; private set; }

        public VehicleItem()
        {
            height = 20f;
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoFitChildrenHorizontally = true;

            atlas = CommonTextures.Atlas;
            backgroundSprite = CommonTextures.FieldNormal;

            StopLayout();

            Label = AddUIComponent<CustomUILabel>();
            Label.autoSize = true;
            Label.wordWrap = false;
            Label.textScale = 0.8f;
            Label.verticalAlignment = UIVerticalAlignment.Middle;
            Label.padding = new RectOffset(4, 0, 4, 0);

            Button = AddUIComponent<CustomUIButton>();
            Button.size = new Vector2(20f, 20f);
            Button.text = "×";
            Button.textScale = 1.2f;
            Button.textPadding = new RectOffset(0, 0, 0, 0);
            Button.textColor = new Color32(204, 204, 204, 255);
            Button.pressedColor = new Color32(224, 224, 224, 255);
            Button.eventClick += (_, _) => OnDelete?.Invoke();

            StartLayout();
        }
        public void Init(VehicleType type)
        {
            Type = type;
            Label.text = type.Description<VehicleType, Mod>();
        }

        void IReusable.DeInit()
        {
            Label.text = string.Empty;
            OnDelete = null;
        }
    }
}
