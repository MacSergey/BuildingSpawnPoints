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
    public class SpawnPointsPanel : ToolPanel<Mod, SpawnPointsTool, SpawnPointsPanel>
    {
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
}
