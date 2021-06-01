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

        private static Dictionary<string, TextureHelper.SpriteParamsGetter> Files { get; } = new Dictionary<string, TextureHelper.SpriteParamsGetter>
        {
            {nameof(HeaderButtons), HeaderButtons},
        };

        static SpawnPointsTextures()
        {
            Atlas = TextureHelper.CreateAtlas(nameof(BuildingSpawnPoints), Files);
        }

        private static UITextureAtlas.SpriteInfo[] HeaderButtons(int texWidth, int texHeight, Rect rect) => TextureHelper.GetSpritesInfo(texWidth, texHeight, rect, 25, 25, new RectOffset(4, 4, 4, 4), 2, AddVehicle, AddVehicleGroup).ToArray();
    }
}
