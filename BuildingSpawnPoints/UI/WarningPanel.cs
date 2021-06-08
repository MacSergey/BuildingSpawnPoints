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
        protected override Color32 Color => Colors.Warning;

        private CustomUILabel Label { get; }
        private VehicleTypePropertyPanel Vehicle { get; }
        private CustomUILabel LabelContinue { get; }

        public WarningPanel()
        {
            StopLayout();

            Label = AddLabel();
            Label.padding = new RectOffset(5, 5, 5, 0);

            Vehicle = ComponentPool.Get<VehicleTypePropertyPanel>(this);
            Vehicle.Deletable = false;

            LabelContinue = AddLabel();
            LabelContinue.padding = new RectOffset(5, 5, 0, 5);

            StartLayout();
        }

        public void Init(VehicleType type)
        {
            StopLayout();

            Label.text = BuildingSpawnPoints.Localize.Panel_NoPointWarning;
            LabelContinue.text = BuildingSpawnPoints.Localize.Panel_NoPointWarningContinue;

            isVisible = type != VehicleType.None;
            Vehicle.SetItems(type);

            StartLayout();

            base.Init();
        }
        public override void DeInit() => Vehicle.SetItems(VehicleType.None);

        private CustomUILabel AddLabel()
        {
            var label = AddUIComponent<CustomUILabel>();
            label.textScale = 0.8f;
            label.autoHeight = true;
            label.wordWrap = true;
            return label;
        }
    }
}
