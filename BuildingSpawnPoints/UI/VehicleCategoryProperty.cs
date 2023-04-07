﻿using ColossalFramework.UI;
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
    public class VehicleCategoryPropertyPanel : BaseEditorPanel, IReusable
    {
        public event Action<VehicleCategory> OnDelete;
        public event Action<VehicleCategory> OnSelect;

        bool IReusable.InCache { get; set; }

        private Dictionary<VehicleCategory, VehicleItem> Items { get; } = new Dictionary<VehicleCategory, VehicleItem>();
        public IEnumerable<VehicleItem> Values => Items.Values;

        public bool Deletable { get; set; } = true;

        public VehicleCategoryPropertyPanel() : base() 
        {
            AutoLayout = AutoLayout.Disabled;
            Padding = new RectOffset(10, 10, 5, 5);
            AutoLayoutSpace = 5;
        }

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

        private void FitItems()
        {
            var items = Items.Values.OrderBy(i => i.width).ToList();
            var prev = default(VehicleItem);

            for(var i = items.Count - 1; i >= 0; i -= 1 )
            {
                if (prev == null)
                {
                    items[i].relativePosition = new Vector2(Padding.left, Padding.top);
                    prev = items[i];
                    items.RemoveAt(i);
                }
                else
                {
                    var j = i;
                    while (j >= 0 && prev.relativePosition.x + prev.width + items[j].width + Padding.horizontal > width)
                        j -= 1;

                    if(j >= 0)
                    {
                        items[j].relativePosition = prev.relativePosition + new Vector3(prev.width + AutoLayoutSpace, 0f);
                        prev = items[j];
                        items.RemoveAt(j);
                    }
                    else
                    {
                        items[i].relativePosition = new Vector2(Padding.left, prev.relativePosition.y + prev.height + AutoLayoutSpace);
                        prev = items[i];
                        items.RemoveAt(i);
                    }
                }
            }

            if (prev != null)
                height = prev.relativePosition.y + prev.height + Padding.vertical;
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
            if (isVisibleSelf)
                FitItems();
        }

        private void EnterItem(VehicleItem item) => OnSelect?.Invoke(item.Type);
        private void LeaveItem(VehicleItem item) => OnSelect?.Invoke(VehicleCategory.None);

        public override void SetStyle(ControlStyle style) { }
    }
    public class VehicleItem : CustomUIPanel, IReusable
    {
        public event Action<VehicleItem> OnDelete;
        public event Action<VehicleItem> OnEnter;
        public event Action<VehicleItem> OnLeave;

        bool IReusable.InCache { get; set; }

        private CustomUILabel Label { get; set; }
        private CustomUIButton Button { get; set; }

        public VehicleCategory Type { get; private set; }

        private bool isCorrect;
        public bool IsCorrect 
        {
            get => isCorrect;
            set
            {
                if(value != isCorrect)
                {
                    isCorrect = value;
                    BgColors = isCorrect ? UIStyle.PropertyNormal : ComponentStyle.ErrorFocusedColor;
                }
            }
        }

        public VehicleItem()
        {
            height = 20f;

            PauseLayout(() =>
            {
                AutoLayout = AutoLayout.Horizontal;
                AutoChildrenHorizontally = AutoLayoutChildren.Fit;

                Atlas = CommonTextures.Atlas;
                BackgroundSprite = CommonTextures.PanelBig;
                BgColors = UIStyle.PropertyNormal;

                Label = AddUIComponent<CustomUILabel>();
                Label.autoSize = true;
                Label.WordWrap = false;
                Label.textScale = 0.8f;
                Label.VerticalAlignment = UIVerticalAlignment.Middle;
                Label.Padding = new RectOffset(4, 4, 4, 0);

                Button = AddUIComponent<CustomUIButton>();
                Button.size = new Vector2(16f, 20f);
                Button.text = "×";
                Button.textScale = 1.2f;
                Button.TextPadding = new RectOffset(0, 4, 0, 0);
                Button.TextColors = Color.white;
                Button.eventClick += (_, _) => OnDelete?.Invoke(this);
            });
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
