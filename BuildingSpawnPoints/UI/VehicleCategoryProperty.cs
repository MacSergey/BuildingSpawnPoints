using ColossalFramework.UI;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ModsCommon.UI.ComponentStyle;

namespace BuildingSpawnPoints.UI
{
    public class VehicleCategoryPropertyPanel : BaseEditorPanel, IReusable
    {
        public event Action<VehicleCategory> OnRemove;
        public event Action<VehicleCategory> OnHover;

        bool IReusable.InCache { get; set; }

        private Dictionary<VehicleCategory, VehicleItem> Items { get; } = new Dictionary<VehicleCategory, VehicleItem>();
        public IEnumerable<VehicleItem> Values => Items.Values;

        public bool Deletable { get; set; } = true;

        public VehicleCategoryPropertyPanel() : base()
        {
            AutoLayout = AutoLayout.Disabled;
            Padding = new RectOffset(10, 10, 7, 7);
            AutoLayoutSpace = 5;
        }

        public void AddItems(VehicleCategory types)
        {
            foreach (var type in types.GetEnumValues().IsItem())
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
            OnRemove = null;
            OnHover = null;
            Deletable = true;
        }
        private void AddItem(VehicleCategory type)
        {
            if (!Items.ContainsKey(type))
            {
                var item = ComponentPool.Get<VehicleItem>(this, type.ToString());
                item.Init(type, Deletable);
                item.OnRemove += RemoveItem;
                item.OnEnter += EnterItem;
                item.OnLeave += LeaveItem;
                Items.Add(type, item);
            }
        }

        private void RemoveItem(VehicleItem item)
        {
            Items.Remove(item.Type);
            OnRemove?.Invoke(item.Type);
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

            for (var i = items.Count - 1; i >= 0; i -= 1)
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

                    if (j >= 0)
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

        private void EnterItem(VehicleItem item) => OnHover?.Invoke(item.Type);
        private void LeaveItem(VehicleItem item) => OnHover?.Invoke(VehicleCategory.None);

        public override void SetStyle(ControlStyle style) { }
    }
    public class VehicleItem : CustomUIPanel, IReusable
    {
        public event Action<VehicleItem> OnRemove;
        public event Action<VehicleItem> OnEnter;
        public event Action<VehicleItem> OnLeave;

        bool IReusable.InCache { get; set; }

        private CustomUILabel Label { get; set; }
        private CustomUIButton Remove { get; set; }

        public VehicleCategory Type { get; private set; }

        private bool isCorrect;
        public bool IsCorrect
        {
            get => isCorrect;
            set
            {
                if (value != isCorrect)
                {
                    isCorrect = value;
                    SetColor();
                }
            }
        }

        private bool isAllCorrect;
        public bool IsAllCorrect
        {
            get => isAllCorrect;
            set
            {
                if (value != isAllCorrect)
                {
                    isAllCorrect = value;
                    SetColor();
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
                BackgroundSprite = CommonTextures.PanelLarge;
                BgColors = UIStyle.PropertyNormal;

                Label = AddUIComponent<CustomUILabel>();
                Label.name = nameof(Label);
                Label.autoSize = true;
                Label.WordWrap = false;
                Label.textScale = 0.8f;
                Label.VerticalAlignment = UIVerticalAlignment.Middle;
                Label.Padding = new RectOffset(7, 4, 4, 0);

                Remove = AddUIComponent<CustomUIButton>();
                Remove.name = nameof(Remove);
                Remove.size = new Vector2(16f, 20f);
                Remove.text = "×";
                Remove.textScale = 1.2f;
                Remove.TextPadding = new RectOffset(0, 4, 0, 0);
                Remove.eventClick += (_, _) => OnRemove?.Invoke(this);
            });
        }
        public void Init(VehicleCategory type, bool deletable = true)
        {
            Type = type;
            Label.text = type.Description<VehicleCategory, Mod>();
            Remove.isVisible = deletable;
            SetColor();
        }
        private void SetColor()
        {
            if (!IsCorrect)
            {
                BgColors = CommonColors.GetOverlayColor(CommonColors.Overlay.Red, 255);
                ForegroundSprite = string.Empty;
            }
            else if (!Settings.ColorTags)
            {
                BgColors = UIStyle.PropertyNormal;
                ForegroundSprite = string.Empty;
            }
            else if (!IsAllCorrect)
            {
                BgColors = DarkPrimaryColor20;
                ForegroundSprite = CommonTextures.BorderLarge;
                FgColors = GetColor();
            }
            else
            {
                BgColors = GetColor();
                ForegroundSprite = string.Empty;
            }

            if (Color.white.GetContrast(NormalBgColor) >= 4.5)
            {
                Label.textColor = Color.white;
                Remove.TextColors = new ColorSet(Color.white, DarkPrimaryColor90, DarkPrimaryColor80, Color.white, Color.white);
            }
            else
            {
                Label.textColor = DarkPrimaryColor15;
                Remove.TextColors = new ColorSet(DarkPrimaryColor15, DarkPrimaryColor25, DarkPrimaryColor30, DarkPrimaryColor15, DarkPrimaryColor15);
            }

            Color32 GetColor() => Type.GetFunction() switch
            {
                VehicleFunction.Planes => CommonColors.GetOverlayColor(CommonColors.Overlay.SkyBlue, 255),
                VehicleFunction.Copters => CommonColors.GetOverlayColor(CommonColors.Overlay.Purple, 255),
                VehicleFunction.Trains => CommonColors.GetOverlayColor(CommonColors.Overlay.Lime, 255),
                VehicleFunction.Ships => CommonColors.GetOverlayColor(CommonColors.Overlay.Blue, 255),
                VehicleFunction.Trucks or VehicleFunction.Cargo => CommonColors.GetOverlayColor(CommonColors.Overlay.Yellow, 255),
                VehicleFunction.Public => CommonColors.GetOverlayColor(CommonColors.Overlay.Green, 255),
                VehicleFunction.Emergency => CommonColors.GetOverlayColor(CommonColors.Overlay.Orange, 255),
                VehicleFunction.Service => CommonColors.GetOverlayColor(CommonColors.Overlay.Turquoise, 255),
                _ => UIStyle.PropertyNormal,
            };
        }

        void IReusable.DeInit()
        {
            Label.text = string.Empty;
            OnRemove = null;
            OnEnter = null;
            OnLeave = null;
            isCorrect = true;
            isAllCorrect = true;
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
