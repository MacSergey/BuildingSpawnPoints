using BuildingSpawnPoints.Utilities;
using ColossalFramework.UI;
using ModsCommon;
using ModsCommon.UI;
using ModsCommon.Utilities;
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

        private PointPanel HoverPointPanel { get; set; }

        public override void Awake()
        {
            base.Awake();

            name = nameof(BuildingSpawnPointsPanel);
            Atlas = CommonTextures.Atlas;
            BackgroundSprite = CommonTextures.PanelBig;
            BgColors = PanelColor;


            Header = AddUIComponent<PanelHeader>();
            Header.name = nameof(Header);
            Header.Target = this;
            Header.BackgroundSprite = "ButtonWhite";
            Header.BgColors = new Color32(36, 40, 40, 255);
            Header.Init(HeaderHeight);


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


            var sizeChanger = AddUIComponent<SizeChanger>();
            Ignore(sizeChanger, true);


            minimumSize = GetSize(400f, 200f);

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
            ContentPanel.size = size - new Vector2(0, Header.height);

            MakePixelPerfect();
        }
        private void SetDefaulSize()
        {
            SingletonMod<Mod>.Logger.Debug($"Set default panel size");
            size = GetSize(400f, 400f);
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
            ResetPanel();

            ContentPanel.PauseLayout(() =>
            {
                RefreshHeader();
                AddWarning();
                RefreshWarning();
                AddAddButton();

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
            });
        }

        private void AddWarning()
        {
            Warning = ComponentPool.Get<WarningPanel>(ContentPanel);
            Warning.Init();
        }
        private void AddAddButton()
        {
            AddButton = ComponentPool.Get<CustomUIButton>(ContentPanel);
            AddButton.name = nameof(AddButton);
            AddButton.text = BuildingSpawnPoints.Localize.Panel_AddPoint;
            AddButton.SetDefaultStyle();
            AddButton.height = 30;
            AddButton.TextHorizontalAlignment = UIHorizontalAlignment.Center;
            AddButton.TextPadding.top = 5;
            AddButton.eventClick += (_, _) => AddPoint();
        }
        private void AddPointPanel(BuildingSpawnPoint point)
        {
            var pointPanel = ComponentPool.Get<PointPanel>(ContentPanel);
            pointPanel.Init(Data, point);
            pointPanel.PanelStyle = UIStyle.Default.PropertyPanel;
            pointPanel.OnEnter += PointMouseEnter;
            pointPanel.OnLeave += PointMouseLeave;
            pointPanel.OnChanged += RefreshWarning;

            AddButton.zOrder = -1;
        }

        public override void RefreshPanel() => SetPanel();
        public void RefreshHeader()
        {
            Header.Text = string.Format(BuildingSpawnPoints.Localize.Panel_Title, Data.Id);
            Header.Refresh();
        }
        private void RefreshWarning() => Warning.Init(Data.LostVehicles);

        private void AddPoint()
        {
            var newPoint = Data.AddPoint();
            AddPointPanel(newPoint);

            ContentPanel.ScrollToEnd();
        }

        public void DeletePoint(PointPanel pointPanel)
        {
            Data.DeletePoint(pointPanel.Point);

            if (HoverPointPanel == pointPanel)
                HoverPointPanel = null;

            ComponentPool.Free(pointPanel);
        }
        public void DuplicatePoint(PointPanel pointPanel)
        {
            var copyPoint = Data.DuplicatePoint(pointPanel.Point);
            AddPointPanel(copyPoint);

            ContentPanel.ScrollToEnd();
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
                pointPanel.Render(cameraInfo);
        }
    }

    public class PanelHeader : HeaderMoveablePanel<PanelHeaderContent>
    {
        private HeaderButtonInfo<HeaderButton> PasteButton { get; set; }

        protected override void FillContent()
        {
            Content.AddButton(new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.CopyHeaderButton, BuildingSpawnPoints.Localize.Panel_Copy, OnCopy));

            PasteButton = new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.PasteHeaderButton, BuildingSpawnPoints.Localize.Panel_Paste, OnPaste);
            Content.AddButton(PasteButton);

            Content.AddButton(new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.ApplyAllHeaderButton, BuildingSpawnPoints.Localize.Panel_ApplyToAll, OnApplyToAll));

            Content.AddButton(new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.ResetHeaderButton, BuildingSpawnPoints.Localize.Panel_ResetToDefault, OnResetToDefault));
        }

        private void OnCopy() => SingletonTool<SpawnPointsTool>.Instance.Copy();
        private void OnPaste() => SingletonTool<SpawnPointsTool>.Instance.Paste();
        private void OnApplyToAll() => SingletonTool<SpawnPointsTool>.Instance.ApplyToAll();
        private void OnResetToDefault() => SingletonTool<SpawnPointsTool>.Instance.ResetToDefault();

        public override void Refresh()
        {
            PasteButton.Enable = !SingletonTool<SpawnPointsTool>.Instance.IsBufferEmpty;
            base.Refresh();
        }
        public void Init(float height) => base.Init(height);
    }
}
