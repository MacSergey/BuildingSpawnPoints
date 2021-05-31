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
        public BuildingData Data { get; private set; }

        public override void Awake()
        {
            base.Awake();

            atlas = TextureHelper.InGameAtlas;
            backgroundSprite = "MenuPanel2";
            name = nameof(SpawnPointsPanel);

            CreateHeader();

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
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (Header != null)
                Header.width = width;

            MakePixelPerfect();
        }
        private void SetDefaulSize()
        {
            SingletonMod<Mod>.Logger.Debug($"Set default panel size");
            size = GetSize(200);
        }
        private Vector2 GetSize(float additional) => new Vector2(Width, Header.height + additional);

        public void SetData(BuildingData data)
        {
            if ((Data = data) != null)
                SetPanel();
            //else
            //    ResetPanel();
        }
        private void SetPanel()
        {
            Header.Text = $"Building #{Data.Id}";
        }

    }

    public class PanelHeader : HeaderMoveablePanel<BaseHeaderContent>
    {
        protected override float DefaultHeight => 40f;
    }
}
