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
        //protected override Color32 Color => Colors.Warning;

        private CustomUILabel Label { get; }
        private VehicleCategoryPropertyPanel Vehicle { get; }
        private CustomUILabel LabelContinue { get; }

        public WarningPanel()
        {
            StopLayout();

            Label = AddLabel();
            Label.Padding = new RectOffset(5, 5, 5, 0);

            Vehicle = ComponentPool.Get<VehicleCategoryPropertyPanel>(this);
            Vehicle.Deletable = false;

            LabelContinue = AddLabel();
            LabelContinue.Padding = new RectOffset(5, 5, 0, 5);

            StartLayout();
        }

        public void Init(VehicleCategory type)
        {
            StopLayout();

            Label.text = BuildingSpawnPoints.Localize.Panel_NoPointWarning;
            LabelContinue.text = BuildingSpawnPoints.Localize.Panel_NoPointWarningContinue;

            isVisible = type != VehicleCategory.None;
            Vehicle.SetItems(type);

            StartLayout();

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
