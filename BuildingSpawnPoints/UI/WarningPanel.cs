using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BuildingSpawnPoints.UI
{
    public class WarningPanel : PropertyGroupPanel
    {
        private CustomUILabel Label { get; set; }
        private VehicleCategoryPropertyPanel Vehicle { get; set; }
        private CustomUILabel LabelContinue { get; set; }

        public WarningPanel() : base()
        {
            atlas = CommonTextures.Atlas;
            backgroundSprite = CommonTextures.PanelLarge;
            bgColors = ComponentStyle.WarningColor;

            PauseLayout(() =>
            {
                Label = AddLabel();
                Label.Padding = new RectOffset(10, 10, 10, 0);

                Vehicle = ComponentPool.Get<VehicleCategoryPropertyPanel>(this);
                Vehicle.Deletable = false;

                LabelContinue = AddLabel();
                LabelContinue.Padding = new RectOffset(10, 10, 0, 5);
            });
        }

        public void Init(VehicleCategory type)
        {
            PauseLayout(() =>
            {
                Label.text = BuildingSpawnPoints.Localize.Panel_NoPointWarning;
                LabelContinue.text = BuildingSpawnPoints.Localize.Panel_NoPointWarningContinue;

                isVisible = type != VehicleCategory.None;
                Vehicle.SetItems(type);
            });

            base.Init();
        }
        public override void DeInit() => Vehicle.SetItems(VehicleCategory.None);

        private CustomUILabel AddLabel()
        {
            var label = AddUIComponent<CustomUILabel>();
            label.textScale = 0.8f;
            label.AutoSize = AutoSize.Height;
            label.WordWrap = true;
            return label;
        }
    }
}
