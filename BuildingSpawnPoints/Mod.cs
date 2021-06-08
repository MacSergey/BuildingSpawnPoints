using BuildingSpawnPoints.Utilities;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using HarmonyLib;
using ICities;
using ModsCommon;
using ModsCommon.UI;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;
using System.Text;
using UnityEngine;

namespace BuildingSpawnPoints
{
    public class Mod : BasePatcherMod<Mod>
    {
        #region PROPERTIES
        public override string NameRaw => "Building Spawn Points";
        public override string Description => string.Empty;

        public override List<Version> Versions => new List<Version>()
        {
            new Version(1,0)
        };

        protected override ulong StableWorkshopId => 2511258910ul;
        protected override ulong BetaWorkshopId => 2504315382ul;

        protected override string IdRaw => nameof(BuildingSpawnPoints);

#if BETA
        public override bool IsBeta => true;
#else
        public override bool IsBeta => false;
#endif
        #endregion
        protected override ResourceManager LocalizeManager => Localize.ResourceManager;

        protected override void GetSettings(UIHelperBase helper)
        {
            var settings = new Settings();
            settings.OnSettingsUI(helper);
        }
        protected override void SetCulture(CultureInfo culture) => Localize.Culture = culture;

        #region PATCHES

        protected override bool PatchProcess()
        {
            var success = true;

            success &= AddTool();
            success &= ToolOnEscape();
            success &= Patch_BuildingWorldInfoPanel_Start();
            //success &= AssetDataExtensionFix();

            var parameters = new Type[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(Randomizer).MakeByRefType(), typeof(VehicleInfo), typeof(Vector3).MakeByRefType(), typeof(Vector3).MakeByRefType() };

            success &= Patch_BuildingAI_CalculateSpawnPosition(parameters);
            success &= Patch_BuildingAI_CalculateUnspawnPosition(parameters);

            //success &= Patch_CalculateSpawnPosition(typeof(CargoStationAI), parameters);
            //success &= Patch_CalculateUnspawnPosition(typeof(CargoStationAI), parameters);

            //success &= Patch_CalculateSpawnPosition(typeof(FishingHarborAI), parameters);
            //success &= Patch_CalculateUnspawnPosition(typeof(FishingHarborAI), parameters);

            //success &= Patch_CalculateSpawnPosition(typeof(ShelterAI), parameters);
            //success &= Patch_CalculateUnspawnPosition(typeof(ShelterAI), parameters);

            success &= Patch_CalculateSpawnPosition(typeof(PostOfficeAI), parameters);
            success &= Patch_CalculateUnspawnPosition(typeof(PostOfficeAI), parameters);

            success &= Patch_CalculateSpawnPosition(typeof(TaxiStandAI), parameters);
            success &= Patch_CalculateUnspawnPosition(typeof(TaxiStandAI), parameters);

            success &= Patch_CalculateSpawnPosition(typeof(MaintenanceDepotAI), parameters);
            success &= Patch_CalculateUnspawnPosition(typeof(MaintenanceDepotAI), parameters);

            success &= Patch_CalculateSpawnPosition(typeof(DepotAI), parameters);
            success &= Patch_CalculateUnspawnPosition(typeof(DepotAI), parameters);

            return success;
        }

        private bool AddTool()
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.ToolControllerAwakeTranspiler), typeof(ToolController), "Awake");
        }

        private bool ToolOnEscape()
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.GameKeyShortcutsEscapeTranspiler), typeof(GameKeyShortcuts), "Escape");
        }

        //private bool AssetDataExtensionFix()
        //{
        //    return AddPostfix(typeof(Patcher), nameof(Patcher.LoadAssetPanelOnLoadPostfix), typeof(LoadAssetPanel), nameof(LoadAssetPanel.OnLoad));
        //}
        private bool Patch_BuildingWorldInfoPanel_Start()
        {
            return AddPostfix(typeof(Patcher), nameof(Patcher.BuildingWorldInfoPanelStartPostfix), typeof(BuildingWorldInfoPanel), "Start");
        }

        private bool Patch_BuildingAI_CalculateSpawnPosition(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.CalculateSpawnPositionTranspiler), typeof(BuildingAI), nameof(BuildingAI.CalculateSpawnPosition), parameters);
        }
        private bool Patch_BuildingAI_CalculateUnspawnPosition(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.CalculateUnpawnPositionTranspiler), typeof(BuildingAI), nameof(BuildingAI.CalculateUnspawnPosition), parameters);
        }

        private bool Patch_CalculateSpawnPosition(Type type, Type[] parameters)
        {
            return AddPrefix(typeof(Patcher), nameof(Patcher.CalculateSpawnPositionPrefix), type, nameof(BuildingAI.CalculateSpawnPosition), parameters);
        }
        private bool Patch_CalculateUnspawnPosition(Type type, Type[] parameters)
        {
            return AddPrefix(typeof(Patcher), nameof(Patcher.CalculateUnspawnPositionPrefix), type, nameof(BuildingAI.CalculateUnspawnPosition), parameters);
        }

        #endregion
    }

    public static class Patcher
    {
        public static IEnumerable<CodeInstruction> ToolControllerAwakeTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions) => ModsCommon.Patcher.ToolControllerAwakeTranspiler<Mod, SpawnPointsTool>(generator, instructions);

        public static IEnumerable<CodeInstruction> GameKeyShortcutsEscapeTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions) => ModsCommon.Patcher.GameKeyShortcutsEscapeTranspiler<Mod, SpawnPointsTool>(generator, instructions);

        //public static void LoadAssetPanelOnLoadPostfix(LoadAssetPanel __instance, UIListBox ___m_SaveList) => ModsCommon.Patcher.LoadAssetPanelOnLoadPostfix<AssetDataExtension>(__instance, ___m_SaveList);

        public static IEnumerable<CodeInstruction> CalculateSpawnPositionTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => CalculatePositionTranspiler(PointType.Spawn, instructions, original);
        public static IEnumerable<CodeInstruction> CalculateUnpawnPositionTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => CalculatePositionTranspiler(PointType.Unspawn, instructions, original);
        private static IEnumerable<CodeInstruction> CalculatePositionTranspiler(PointType type, IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            var enumerator = instructions.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var instruction = enumerator.Current;
                yield return instruction;

                if (instruction.opcode == OpCodes.Ret)
                {
                    enumerator.MoveNext();
                    break;
                }
            }

            yield return new CodeInstruction(OpCodes.Nop) { labels = enumerator.Current.labels };
            yield return new CodeInstruction(OpCodes.Ldc_I4, (int)type);
            yield return original.GetLDArg("buildingID");
            yield return original.GetLDArg("data");
            yield return original.GetLDArg("randomizer");
            yield return original.GetLDArg("info");
            yield return original.GetLDArg("position");
            yield return original.GetLDArg("target");
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patcher), nameof(Patcher.GetPosition)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static void GetPosition(PointType type, ushort buildingID, ref Building data, ref Randomizer randomizer, VehicleInfo info, out Vector3 position, out Vector3 target)
        {
            if (SingletonManager<Manager>.Instance[buildingID] is not BuildingData buildingData || !buildingData.GetPosition(type, ref data, info, ref randomizer, out position, out target))
            {
                position = data.CalculateSidewalkPosition(0f, 2f);
                target = position;
            }
        }

        public static bool CalculateSpawnPositionPrefix(ushort buildingID, ref Building data, ref Randomizer randomizer, VehicleInfo info, ref Vector3 position, ref Vector3 target)
        {
            return SingletonManager<Manager>.Instance[buildingID] is not BuildingData buildingData || !buildingData.GetPosition(PointType.Spawn, ref data, info, ref randomizer, out position, out target);
        }
        public static bool CalculateUnspawnPositionPrefix(ushort buildingID, ref Building data, ref Randomizer randomizer, VehicleInfo info, ref Vector3 position, ref Vector3 target)
        {
            return SingletonManager<Manager>.Instance[buildingID] is not BuildingData buildingData || !buildingData.GetPosition(PointType.Unspawn, ref data, info, ref randomizer, out position, out target);
        }
        public static void BuildingWorldInfoPanelStartPostfix(CityServiceWorldInfoPanel __instance)
        {
            var buildingName = __instance.Find<UITextField>("BuildingName");
            var namePosition = buildingName.relativePosition;
            buildingName.width -= 36f;
            buildingName.relativePosition = namePosition;

            var locationMarker = __instance.Find<UIMultiStateButton>("LocationMarker");
            locationMarker.relativePosition -= new Vector3(36f, 0f);

            var routesButton = __instance.Find<UIMultiStateButton>("ShowHideRoutesButton");
            routesButton.relativePosition -= new Vector3(36f, 0f);

            var button = routesButton.parent.AddUIComponent<MultyAtlasUIButton>();
            button.relativePosition = routesButton.relativePosition + new Vector3(36f, 0f);
            button.size = new Vector2(32f, 32f);
            button.zOrder = routesButton.zOrder + 1;
            button.eventTooltipEnter += (_,_) => button.tooltip = $"{SingletonMod<Mod>.Instance.NameRaw} ({SingletonTool<SpawnPointsTool>.Activation})";

            button.BgAtlas = TextureHelper.InGameAtlas;
            button.normalBgSprite = "InfoIconBaseNormal";
            button.pressedBgSprite = "InfoIconBasePressed";
            button.hoveredBgSprite = "InfoIconBaseHovered";
            button.disabledBgSprite = "InfoIconBaseDisabled";

            button.FgAtlas = SpawnPointsTextures.Atlas;
            button.normalFgSprite = SpawnPointsTextures.InfoNormal;
            button.pressedFgSprite = SpawnPointsTextures.InfoPressed;

            button.eventClicked += (_, _) =>
            {
                var currentInstance = WorldInfoPanel.GetCurrentInstanceID();
                if (currentInstance.Building != 0)
                {
                    var tool = SingletonTool<SpawnPointsTool>.Instance;
                    tool.Enable();
                    tool.SetData(SingletonManager<Manager>.Instance[currentInstance.Building, Options.Default]);
                    tool.SetDefaultMode();
                }
            };
        }

        private static void Button_eventTooltipEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
            throw new NotImplementedException();
        }
    }
}
