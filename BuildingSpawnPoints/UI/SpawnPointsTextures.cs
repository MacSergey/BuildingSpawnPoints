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

        public static string AddVehicleHeaderButton => nameof(AddVehicleHeaderButton);
        public static string AddVehicleGroupHeaderButton => nameof(AddVehicleGroupHeaderButton);
        public static string AddAllVehiclesHeaderButton => nameof(AddAllVehiclesHeaderButton);
        public static string CopyHeaderButton => nameof(CopyHeaderButton);
        public static string PasteHeaderButton => nameof(PasteHeaderButton);
        public static string AppendHeaderButton => nameof(AppendHeaderButton);
        public static string DuplicateHeaderButton => nameof(DuplicateHeaderButton);
        public static string ResetHeaderButton => nameof(ResetHeaderButton);
        public static string ApplyAllHeaderButton => nameof(ApplyAllHeaderButton);

        public static string UUIButtonNormal => nameof(UUIButtonNormal);
        public static string UUIButtonHovered => nameof(UUIButtonHovered);
        public static string UUIButtonPressed => nameof(UUIButtonPressed);

        public static string InfoNormal => nameof(InfoNormal);
        public static string InfoPressed => nameof(InfoPressed);

        static SpawnPointsTextures()
        {
            var spriteParams = new Dictionary<string, RectOffset>();

            //ActivationButton
            spriteParams[InfoNormal] = new RectOffset();
            spriteParams[InfoPressed] = new RectOffset();

            //UUIButton
            spriteParams[UUIButtonNormal] = new RectOffset();
            spriteParams[UUIButtonHovered] = new RectOffset();
            spriteParams[UUIButtonPressed] = new RectOffset();

            //HeaderButtons
            spriteParams[AddVehicleHeaderButton] = new RectOffset();
            spriteParams[AddVehicleGroupHeaderButton] = new RectOffset();
            spriteParams[AddAllVehiclesHeaderButton] = new RectOffset();
            spriteParams[CopyHeaderButton] = new RectOffset();
            spriteParams[PasteHeaderButton] = new RectOffset();
            spriteParams[AppendHeaderButton] = new RectOffset();
            spriteParams[DuplicateHeaderButton] = new RectOffset();
            spriteParams[ResetHeaderButton] = new RectOffset();
            spriteParams[ApplyAllHeaderButton] = new RectOffset();

            Atlas = TextureHelper.CreateAtlas(nameof(BuildingSpawnPoints), spriteParams);
        }
    }
}
