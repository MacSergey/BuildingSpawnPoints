using UnityEngine;
using ModsCommon.UI;
using static ModsCommon.UI.ComponentStyle;
using static ModsCommon.Utilities.CommonTextures;

namespace BuildingSpawnPoints.UI
{
    public static class UIStyle
    {
        public static Color32 PropertyPanel => DarkPrimaryColor20;
        public static Color32 PropertyNormal => DarkPrimaryColor45;
        public static Color32 PropertyHovered => DarkPrimaryColor55;
        public static Color32 PropertyPressed => DarkPrimaryColor60;
        public static Color32 PropertyFocused => NormalBlue;

        public static Color32 PopupBackground => DarkPrimaryColor15;
        public static Color32 PopupEntitySelected => NormalBlue;
        public static Color32 PopupEntityHovered => DarkPrimaryColor50;
        public static Color32 PopupEntityPressed => DarkPrimaryColor60;

        public static ControlStyle Default { get; } = new ControlStyle()
        {
            TextField = new TextFieldStyle()
            {
                BgAtlas = Atlas,
                FgAtlas = Atlas,

                BgSprites = new SpriteSet(FieldSingle, FieldSingle, FieldSingle, FieldSingle, BorderSmall),
                BgColors = new ColorSet(PropertyNormal, PropertyHovered, PropertyHovered, PropertyNormal, PropertyNormal),

                FgSprites = new SpriteSet(default, default, default, BorderSmall, default),
                FgColors = new ColorSet(default, default, default, PropertyFocused, default),

                TextColors = new ColorSet(Color.white, Color.white, Color.white, Color.white, Color.black),

                SelectionSprite = Empty,
                SelectionColor = PropertyFocused,
            },
            Segmented = new SegmentedStyle()
            {
                Single = GetSegmentedStyle(FieldSingle, BorderSmall),
                Left = GetSegmentedStyle(FieldLeft, FieldBorderLeft),
                Middle = GetSegmentedStyle(FieldMiddle, FieldBorderMiddle),
                Right = GetSegmentedStyle(FieldRight, FieldBorderRight),
            },
            Button = new ButtonStyle()
            {
                BgAtlas = Atlas,
                FgAtlas = Atlas,

                BgSprites = new SpriteSet(PanelBig, PanelBig, PanelBig, PanelBig, BorderBig),
                BgColors = new ColorSet(PropertyNormal, PropertyHovered, PropertyPressed, PropertyNormal, PropertyNormal),
                SelBgColors = new ColorSet(),

                FgSprites = default,
                FgColors = new ColorSet(Color.white, Color.white, Color.white, Color.white, Color.black),
                SelFgColors = default,

                TextColors = new ColorSet(Color.white, Color.white, Color.white, Color.white, Color.black),
                SelTextColors = default,
            },
            DropDown = new DropDownStyle()
            {
                BgAtlas = Atlas,
                FgAtlas = Atlas,

                AllBgSprites = new SpriteSet(FieldSingle, FieldSingle, FieldSingle, FieldSingle, BorderSmall),
                BgColors = new ColorSet(PropertyNormal, PropertyHovered, PropertyHovered, PropertyNormal, PropertyNormal),
                SelBgColors = PropertyFocused,

                FgSprites = new SpriteSet(VectorDown, VectorDown, VectorDown, VectorDown, default),
                FgColors = Color.white,

                AllTextColors = new ColorSet(Color.white, Color.white, Color.white, Color.white, Color.black),


                PopupAtlas = Atlas,
                PopupSprite = FieldSingle,
                PopupColor = PopupBackground,
                PopupItemsPadding = new RectOffset(4, 4, 4, 4),


                EntityAtlas = Atlas,

                EntitySprites = new SpriteSet(default, FieldSingle, FieldSingle, default, default),
                EntitySelSprites = FieldSingle,

                EntityColors = new ColorSet(default, PopupEntityHovered, PopupEntityPressed, default, default),
                EntitySelColors = PopupEntitySelected,
            },
            Toggle = new ToggleStyle()
            {
                BgAtlas = Atlas,
                MarkAtlas = Atlas,

                OnBgSprites = ToggleBackgroundSmall,
                OffBgSprites = new SpriteSet(ToggleBackgroundSmall, ToggleBackgroundSmall, ToggleBackgroundSmall, ToggleBackgroundSmall, ToggleBorderSmall),

                OnMarkSprites = ToggleCircle,
                OffMarkSprites = ToggleCircle,

                OnBgColors = new ColorSet(PropertyFocused, PropertyFocused, PropertyFocused, PropertyFocused, PropertyNormal),
                OffBgColors = new ColorSet(PropertyNormal, PropertyHovered, PropertyHovered, PropertyNormal, PropertyNormal),

                OnMarkColors = new ColorSet(Color.white, Color.white, Color.white, Color.white, PropertyPanel),
                OffMarkColors = new ColorSet(Color.white, Color.white, Color.white, Color.white, PropertyNormal),

                OnTextColors = new ColorSet(Color.white, Color.white, Color.white, Color.white, PropertyPanel),
                OffTextColors = new ColorSet(Color.white, Color.white, Color.white, Color.white, Color.black),
            },
            Label = new LabelStyle()
            {
                NormalTextColor = Color.white,
                DisabledTextColor = Color.black,
            },
            PropertyPanel = new PropertyPanelStyle()
            {
                BgAtlas = Atlas,
                BgSprites = PanelLarge,
                BgColors = PropertyPanel,
                MaskSprite = OpacitySliderMask,
            }
        };
        private static ButtonStyle GetSegmentedStyle(string background, string border)
        {
            return new ButtonStyle()
            {
                BgAtlas = Atlas,
                FgAtlas = Atlas,

                BgSprites = new SpriteSet(background, background, background, background, border),
                BgColors = new ColorSet(PropertyNormal, PropertyHovered, PropertyHovered, PropertyNormal, PropertyNormal),
                SelBgSprites = background,
                SelBgColors = new ColorSet(PropertyFocused, HoveredBlue, PressedBlue, PropertyFocused, PropertyNormal),

                FgColors = new ColorSet(Color.white, Color.white, Color.white, Color.white, PropertyNormal),
                SelFgColors = new ColorSet(Color.white, Color.white, Color.white, Color.white, PropertyPanel),

                TextColors = new ColorSet(Color.white, Color.white, Color.white, Color.white, Color.black),
                SelTextColors = new ColorSet(Color.white, Color.white, Color.white, Color.white, PropertyPanel),
            };
        }
    }
}
