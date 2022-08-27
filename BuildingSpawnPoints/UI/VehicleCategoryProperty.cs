using ColossalFramework.UI;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BuildingSpawnPoints.UI
{
    public class VehicleCategoryPropertyPanel : EditorItem, IReusable, IEnumerable<VehicleItem>
    {
        public event Action<VehicleCategory> OnDelete;
        public event Action<VehicleCategory> OnSelect;

        bool IReusable.InCache { get; set; }

        private Dictionary<VehicleCategory, VehicleItem> Items { get; } = new Dictionary<VehicleCategory, VehicleItem>();
        private float Padding => 5f;

        public bool Deletable { get; set; } = true;

        public void AddItems(VehicleCategory types)
        {
            foreach (var type in EnumExtension.GetEnumValues<VehicleCategory>(t => t.IsItem() && (t & types) != VehicleCategory.None))
                AddItem(type);

            FitItems();
        }
        public void SetItems(VehicleCategory types)
        {
            ClearItems();
            AddItems(types);
        }
        void IReusable.DeInit()
        {
            ClearItems();
            OnDelete = null;
            OnSelect = null;
            Deletable = true;
        }
        private void AddItem(VehicleCategory type)
        {
            if (!Items.ContainsKey(type))
            {
                var item = ComponentPool.Get<VehicleItem>(this);
                item.Init(type, Deletable);
                item.OnDelete += DeleteItem;
                item.OnEnter += EnterItem;
                item.OnLeave += LeaveItem;
                Items.Add(type, item);
            }
        }

        private void DeleteItem(VehicleItem item)
        {
            Items.Remove(item.Type);
            OnDelete?.Invoke(item.Type);
            ComponentPool.Free(item);

            FitItems();
        }
        private void ClearItems()
        {
            foreach (var component in components.ToArray())
                ComponentPool.Free(component);

            Items.Clear();
        }

        public void AddType(VehicleCategory type)
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

        private void EnterItem(VehicleItem item) => OnSelect?.Invoke(item.Type);
        private void LeaveItem(VehicleItem item) => OnSelect?.Invoke(VehicleCategory.None);

        public IEnumerator<VehicleItem> GetEnumerator() => Items.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public class VehicleItem : UIAutoLayoutPanel, IReusable
    {
        public event Action<VehicleItem> OnDelete;
        public event Action<VehicleItem> OnEnter;
        public event Action<VehicleItem> OnLeave;

        bool IReusable.InCache { get; set; }

        private CustomUILabel Label { get; }
        private CustomUIButton Button { get; }

        public VehicleCategory Type { get; private set; }

        private bool _isCorrect;
        public bool IsCorrect 
        {
            get => _isCorrect;
            set
            {
                if(value != _isCorrect)
                {
                    _isCorrect = value;
                    color = _isCorrect ? Color.white : Color.red;
                }
            }
        }

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
            Label.padding = new RectOffset(4, 4, 4, 0);

            Button = AddUIComponent<CustomUIButton>();
            Button.size = new Vector2(16f, 20f);
            Button.text = "×";
            Button.textScale = 1.2f;
            Button.textPadding = new RectOffset(0, 4, 0, 0);
            Button.textColor = new Color32(204, 204, 204, 255);
            Button.pressedColor = new Color32(224, 224, 224, 255);
            Button.eventClick += (_, _) => OnDelete?.Invoke(this);

            StartLayout();
        }
        public void Init(VehicleCategory type, bool deletable = true)
        {
            Type = type;
            Label.text = type.Description<VehicleCategory, Mod>();
            Button.isVisible = deletable;
        }

        void IReusable.DeInit()
        {
            Label.text = string.Empty;
            OnDelete = null;
            OnEnter = null;
            OnLeave = null;
            IsCorrect = true;
        }

        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            base.OnMouseEnter(p);
            OnEnter?.Invoke(this);
        }
        protected override void OnMouseLeave(UIMouseEventParameter p)
        {
            base.OnMouseLeave(p);
            OnLeave?.Invoke(this);
        }
    }
}
