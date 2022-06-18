using ColossalFramework.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BuildingSpawnPoints.Utilities
{
    public static class SpawnPointsTextures
    {
        public static UITextureAtlas Atlas;
        public static Texture2D Texture => Atlas.texture;

        public static string AddVehicle => nameof(AddVehicle);
        public static string AddVehicleGroup => nameof(AddVehicleGroup);
        public static string AddAllVehicles => nameof(AddAllVehicles);
        public static string Copy => nameof(Copy);
        public static string Paste => nameof(Paste);
        public static string Append => nameof(Append);
        public static string Duplicate => nameof(Duplicate);
        public static string Reset => nameof(Reset);
        public static string ApplyAll => nameof(ApplyAll);

        public static string UUINormal => nameof(UUINormal);
        public static string UUIHovered => nameof(UUIHovered);
        public static string UUIPressed => nameof(UUIPressed);
        //public static string UUIDisabled => nameof(UUIDisabled);

        public static string InfoNormal => nameof(InfoNormal);
        public static string InfoPressed => nameof(InfoPressed);

        private static Dictionary<string, TextureHelper.SpriteParamsGetter> Files { get; } = new Dictionary<string, TextureHelper.SpriteParamsGetter>
        {
            {nameof(HeaderButtons), HeaderButtons},
            {nameof(InfoIcon), InfoIcon},
            {nameof(UUIButton), UUIButton},
        };

        static SpawnPointsTextures()
        {
            Atlas = TextureHelper.CreateAtlas(nameof(BuildingSpawnPoints), Files);
        }

        private static UITextureAtlas.SpriteInfo[] HeaderButtons(int texWidth, int texHeight, Rect rect) => TextureHelper.GetSpritesInfo(texWidth, texHeight, rect, 25, 25, new RectOffset(4, 4, 4, 4), 2, AddVehicle, AddVehicleGroup, AddAllVehicles, Copy, Paste, Append, Duplicate, Reset, ApplyAll).ToArray();

        private static UITextureAtlas.SpriteInfo[] InfoIcon(int texWidth, int texHeight, Rect rect) => TextureHelper.GetSpritesInfo(texWidth, texHeight, rect, 36, 36, InfoNormal, InfoPressed).ToArray();

        private static UITextureAtlas.SpriteInfo[] UUIButton(int texWidth, int texHeight, Rect rect) => TextureHelper.GetSpritesInfo(texWidth, texHeight, rect, 40, 40, UUINormal, UUIHovered, UUIPressed/*, UUIDisabled*/).ToArray();
    }
}
