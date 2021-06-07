using ColossalFramework.UI;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BuildingSpawnPoints.UI
{
    public class VehicleTypePropertyPanel : EditorItem, IReusable
    {
        public event Action<VehicleType> OnDelete;

        bool IReusable.InCache { get; set; }

        private Dictionary<VehicleType, VehicleItem> Items = new Dictionary<VehicleType, VehicleItem>();
        private float Padding => 5f;

        public void AddItems(VehicleType types)
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
            OnDelete = null;
        }

        private void AddItem(VehicleType type)
        {
            if (!Items.ContainsKey(type))
            {
                var item = ComponentPool.Get<VehicleItem>(this);
                item.Init(type);
                item.OnDelete += DeleteItem;
                Items.Add(type, item);
            }
        }

        private void DeleteItem(VehicleItem item)
        {
            OnDelete?.Invoke(item.Type);
            Items.Remove(item.Type);
            ComponentPool.Free(item);

            FitItems();
        }

        public void AddType(VehicleType type)
        {
            if (!Items.ContainsKey(type))
            {
                AddItem(type);
                FitItems();
            }
        }
        private void FitItems()
        {
            var items = Items.Values.OrderBy(i => i.width).ToList();
            var prev = default(VehicleItem);

            for(var i = items.Count - 1; i >= 0; i -= 1 )
            {
                if (prev == null)
                {
                    items[i].relativePosition = new Vector2(Padding, Padding);
                    prev = items[i];
                    items.RemoveAt(i);
                }
                else
                {
                    var j = i;
                    while (j >= 0 && prev.relativePosition.x + prev.width + items[j].width + Padding * 2 > width)
                        j -= 1;

                    if(j >= 0)
                    {
                        items[j].relativePosition = prev.relativePosition + new Vector3(prev.width + Padding, 0f);
                        prev = items[j];
                        items.RemoveAt(j);
                    }
                    else
                    {
                        items[i].relativePosition = new Vector2(Padding, prev.relativePosition.y + prev.height + Padding);
                        prev = items[i];
                        items.RemoveAt(i);
                    }
                }
            }

            if (prev != null)
                height = prev.relativePosition.y + prev.height + Padding;
            else
                height = 0f;
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            FitItems();
        }
        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();
            if (isVisible)
                FitItems();
        }
    }
    public class VehicleItem : UIAutoLayoutPanel, IReusable
    {
        public event Action<VehicleItem> OnDelete;

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
            Button.eventClick += (_, _) => OnDelete?.Invoke(this);

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
