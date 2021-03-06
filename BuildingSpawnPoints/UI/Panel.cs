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
    public class BuildingSpawnPointsPanel : ToolPanel<Mod, SpawnPointsTool, BuildingSpawnPointsPanel>
    {
        private PanelHeader Header { get; set; }
        private AdvancedScrollablePanel ContentPanel { get; set; }
        private AddPointButton AddButton { get; set; }
        private WarningPanel Warning { get; set; }

        public BuildingData Data { get; private set; }

        private PointPanel HoverPointPanel { get; set; }

        public override void Awake()
        {
            base.Awake();

            atlas = TextureHelper.InGameAtlas;
            backgroundSprite = "MenuPanel2";
            name = nameof(BuildingSpawnPointsPanel);

            CreateHeader();
            CreateContent();
            CreateSizeChanger();

            minimumSize = GetSize(300f, 200f);
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
        private void CreateSizeChanger() => AddUIComponent<SizeChanger>();

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

            ContentPanel.Content.StopLayout();

            RefreshHeader();
            AddWarning();
            RefreshWarning();
            AddAddButton();

            foreach (var point in Data.Points)
                AddPointPanel(point);

            ContentPanel.Content.StartLayout();
        }
        private void ResetPanel()
        {
            ContentPanel.Content.StopLayout();

            HoverPointPanel = null;

            foreach (var component in ContentPanel.Content.components.ToArray())
                ComponentPool.Free(component);

            ContentPanel.Content.StartLayout();
        }

        private void AddWarning()
        {
            Warning = ComponentPool.Get<WarningPanel>(ContentPanel.Content);
            Warning.Init();
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
            var pointPanel = ComponentPool.Get<PointPanel>(ContentPanel.Content);
            pointPanel.Init(Data, point);
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

            ContentPanel.Content.ScrollToBottom();
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

            ContentPanel.Content.ScrollToBottom();
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

        private class AddPointButton : ButtonPanel
        {
            public AddPointButton()
            {
                Button.textScale = 1f;
            }
            protected override void SetSize() => Button.size = size;
        }
    }

    public class PanelHeader : HeaderMoveablePanel<PanelHeaderContent>
    {
        private HeaderButtonInfo<HeaderButton> PasteButton { get; set; }
        protected override float DefaultHeight => 40f;

        public PanelHeader()
        {
            Content.AddButton(new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.Copy, BuildingSpawnPoints.Localize.Panel_Copy, OnCopy));

            PasteButton = new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.Paste, BuildingSpawnPoints.Localize.Panel_Paste, OnPaste);
            Content.AddButton(PasteButton);

            Content.AddButton(new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.ApplyAll, BuildingSpawnPoints.Localize.Panel_ApplyToAll, OnApplyToAll));

            Content.AddButton(new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.Reset, BuildingSpawnPoints.Localize.Panel_ResetToDefault, OnResetToDefault));
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
