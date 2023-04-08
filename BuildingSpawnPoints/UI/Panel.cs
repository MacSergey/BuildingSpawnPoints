using BuildingSpawnPoints.Utilities;
using ColossalFramework.UI;
using ModsCommon;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ModsCommon.UI.ComponentStyle;

namespace BuildingSpawnPoints.UI
{
    public class BuildingSpawnPointsPanel : ToolPanel<Mod, SpawnPointsTool, BuildingSpawnPointsPanel>
    {
        private PanelHeader Header { get; set; }
        private CustomUIScrollablePanel ContentPanel { get; set; }
        private CustomUIButton AddButton { get; set; }
        private WarningPanel Warning { get; set; }

        public BuildingData Data { get; private set; }

        private Dictionary<BuildingSpawnPoint, PointPanel> PointPanels { get; } = new Dictionary<BuildingSpawnPoint, PointPanel>();
        private PointPanel HoverPointPanel { get; set; }
        public BuildingSpawnPoint HoverPoint => HoverPointPanel?.Point;

        public override void Awake()
        {
            base.Awake();

            name = nameof(BuildingSpawnPointsPanel);
            Atlas = CommonTextures.Atlas;
            BackgroundSprite = CommonTextures.PanelBig;
            BgColors = DarkPrimaryColor10;


            Header = AddUIComponent<PanelHeader>();
            Header.name = nameof(Header);
            Header.Target = this;
            Header.BackgroundSprite = "ButtonWhite";
            Header.BgColors = new Color32(36, 40, 40, 255);
            Header.Init(HeaderHeight);


            Warning = AddUIComponent<WarningPanel>();
            Warning.Init();
            SetItemMargin(Warning, new RectOffset(10, 10, 10, 10));
            Warning.eventVisibilityChanged += WarningVisibilityChanged;
            Warning.eventSizeChanged += WarningSizeChanged;


            ContentPanel = AddUIComponent<CustomUIScrollablePanel>();
            ContentPanel.name = nameof(ContentPanel);
            ContentPanel.ScrollOrientation = UIOrientation.Vertical;
            ContentPanel.Padding = new RectOffset(10, 10, 10, 10);
            ContentPanel.Atlas = TextureHelper.InGameAtlas;

            ContentPanel.AutoChildrenVertically = AutoLayoutChildren.None;
            ContentPanel.AutoChildrenHorizontally = AutoLayoutChildren.Fill;
            ContentPanel.AutoLayoutSpace = 10;
            ContentPanel.AutoLayout = AutoLayout.Vertical;

            ContentPanel.ScrollbarSize = 12f;
            ContentPanel.Scrollbar.DefaultStyle();


            AddButton = AddUIComponent<CustomUIButton>();
            AddButton.name = nameof(AddButton);
            AddButton.text = BuildingSpawnPoints.Localize.Panel_AddPoint;
            AddButton.tooltip = SpawnPointsTool.AddPointShortcut;
            AddButton.SetDefaultStyle();
            AddButton.height = 30;
            AddButton.TextHorizontalAlignment = UIHorizontalAlignment.Center;
            AddButton.TextPadding.top = 5;
            AddButton.eventClick += (_, _) => AddPoint();
            SetItemMargin(AddButton, new RectOffset(10, 10, 10, 10));


            var sizeChanger = AddUIComponent<SizeChanger>();
            Ignore(sizeChanger, true);


            minimumSize = GetSize(400f, 300f);

            AutoChildrenHorizontally = AutoLayoutChildren.Fill;
            AutoLayout = AutoLayout.Vertical;
        }

        public override void Start()
        {
            base.Start();
            SetDefaulSize();
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            Header.width = width;
            SetContentSize();
            MakePixelPerfect();
        }
        private void WarningVisibilityChanged(UIComponent component, bool value) => SetContentSize();
        private void WarningSizeChanged(UIComponent component, Vector2 value) => SetContentSize();
        private void SetContentSize() => ContentPanel.height = height - Header.height - (Warning.isVisibleSelf ? 10f + Warning.height + 10f : 0f) - 10f - AddButton.height - 10f;
        private void SetDefaulSize()
        {
            SingletonMod<Mod>.Logger.Debug($"Set default panel size");
            size = GetSize(400f, 600f);
        }
        private Vector2 GetSize(float width, float height) => new Vector2(width, Header.height + height);

        public void SetData(BuildingData data)
        {
            if ((Data = data) != null)
                SetPanel();
            else
                ResetPanel();
        }
        private void SetPanel()
        {
            RefreshWarning();
            ResetPanel();

            ContentPanel.PauseLayout(() =>
            {
                RefreshHeader();

                foreach (var point in Data.Points)
                    AddPointPanel(point);
            });
        }
        private void ResetPanel()
        {
            ContentPanel.PauseLayout(() =>
            {
                HoverPointPanel = null;

                foreach (var component in ContentPanel.components.ToArray())
                {
                    if (component != ContentPanel.Scrollbar)
                        ComponentPool.Free(component);
                }

                PointPanels.Clear();
            });
        }
        private void AddPointPanel(BuildingSpawnPoint point)
        {
            var pointPanel = ComponentPool.Get<PointPanel>(ContentPanel);
            pointPanel.Init(Data, point);
            pointPanel.PanelStyle = UIStyle.Default.PropertyPanel;
            pointPanel.OnEnter += PointMouseEnter;
            pointPanel.OnLeave += PointMouseLeave;
            pointPanel.OnChanged += RefreshWarning;

            PointPanels[point] = pointPanel;
        }

        public override void RefreshPanel() => SetPanel();
        public void RefreshHeader()
        {
            Header.Text = string.Format(BuildingSpawnPoints.Localize.Panel_Title, Data.Id);
            Header.Refresh();
        }
        private void RefreshWarning() => Warning.Init(Data.LostVehicles);

        public void AddPoint()
        {
            var newPoint = Data.AddPoint();
            AddPointPanel(newPoint);

            ContentPanel.ScrollToEnd();
        }

        public void DeletePoint(BuildingSpawnPoint point)
        {
            Data.DeletePoint(point);

            if (PointPanels.TryGetValue(point, out var pointPanel))
            {
                if (HoverPointPanel == pointPanel)
                    HoverPointPanel = null;

                ComponentPool.Free(pointPanel);
                PointPanels.Remove(point);
            }
        }
        public void DuplicatePoint(BuildingSpawnPoint point)
        {
            var copyPoint = Data.DuplicatePoint(point);
            AddPointPanel(copyPoint);

            ContentPanel.ScrollToEnd();
        }
        public void SelectPoint(BuildingSpawnPoint point)
        {
            if (PointPanels.TryGetValue(point, out var pointPanel))
            {
                pointPanel.Refresh();
                ContentPanel.ScrollIntoView(pointPanel);
            }
        }

        private void PointMouseEnter(PointPanel rulePanel, UIMouseEventParameter eventParam) => HoverPointPanel = rulePanel;
        private void PointMouseLeave(PointPanel rulePanel, UIMouseEventParameter eventParam)
        {
            var uiView = rulePanel.GetUIView();
            var mouse = uiView.ScreenPointToGUI((eventParam.position + eventParam.moveDelta) / uiView.inputScale);
            var pointRect = new Rect(ContentPanel.absolutePosition + rulePanel.relativePosition, rulePanel.size);
            var contentRect = new Rect(ContentPanel.absolutePosition, ContentPanel.size);

            if (eventParam.source == rulePanel || !pointRect.Contains(mouse) || !contentRect.Contains(mouse))
                HoverPointPanel = null;
        }
        public void Render(RenderManager.CameraInfo cameraInfo)
        {
            if (HoverPointPanel is PointPanel pointPanel)
                pointPanel.Point.Render(cameraInfo, true, pointPanel.HoveredVehicleType);
        }
        public void RenderGeometry(RenderManager.CameraInfo cameraInfo)
        {
            if (HoverPointPanel is PointPanel pointPanel)
                pointPanel.Point.RenderGeometry(cameraInfo, true);
        }
    }
}
