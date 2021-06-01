﻿using ColossalFramework.UI;
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
            AddPointType();
            AddPosition();
            AddAngle();

            StartLayout();

            base.Init();
        }
        public override void DeInit()
        {
            base.DeInit();

            Point = null;
            Header = null;
        }

        private void AddHeader()
        {
            Header = ComponentPool.Get<PointHeaderPanel>(this, nameof(Header));
            Header.Init();
            Header.OnDelete += () => SingletonItem<SpawnPointsPanel>.Instance.DeletePoint(this);
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
            position.Init(0,2);
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
}
