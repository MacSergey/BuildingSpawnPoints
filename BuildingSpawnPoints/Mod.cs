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
using static ColossalFramework.Plugins.PluginManager;

namespace BuildingSpawnPoints
{
    public class Mod : BasePatcherMod<Mod>
    {
        #region PROPERTIES
        public override string NameRaw => "Building Spawn Points";
        public override string Description => !IsBeta ? Localize.Mod_Description : CommonLocalize.Mod_DescriptionBeta;

        public override List<Version> Versions => new List<Version>()
        {
            new Version(1,0,2),
            new Version(1,0,1),
            new Version(1,0),
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

        private static string METMName => "More Effective Transfer Manager";
        private static ulong METMId => 1680840913ul;
        private static PluginSearcher METMSearcher { get; } = PluginUtilities.GetSearcher(METMName, METMId);
        public static PluginInfo METM => PluginUtilities.GetPlugin(METMSearcher);

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

            PatchBuildings(ref success);
            PatchVehicles(ref success);

            if (METM is null)
                Logger.Debug("METM not exist, skip patches");
            else
                Patch_METM(ref success);

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

        #region BUILDINGS

        private void PatchBuildings(ref bool success)
        {
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
        }
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

        #region VEHICLES

        private void PatchVehicles(ref bool success)
        {
            var startPathFindParams = new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() };
            success &= Patch_PoliceCarAI_StartPathFind(startPathFindParams);
            success &= Patch_FireTruckAI_StartPathFind(startPathFindParams);
            success &= Patch_DisasterResponseVehicleAI_StartPathFind(startPathFindParams);
            success &= Patch_ParkMaintenanceVehicleAI_StartPathFind(startPathFindParams);

            success &= Patch_FireTruckAI_SetSource();
        }

        private bool Patch_PoliceCarAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.StartPathFind_Transpiler), typeof(PoliceCarAI), "StartPathFind", parameters);
        }
        private bool Patch_FireTruckAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.StartPathFind_Transpiler), typeof(FireTruckAI), "StartPathFind", parameters);
        }
        private bool Patch_DisasterResponseVehicleAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.StartPathFind_Transpiler), typeof(DisasterResponseVehicleAI), "StartPathFind", parameters);
        }
        private bool Patch_ParkMaintenanceVehicleAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.StartPathFind_Transpiler), typeof(ParkMaintenanceVehicleAI), "StartPathFind", parameters);
        }
        private bool Patch_FireTruckAI_SetSource()
        {
            return AddPrefix(typeof(Patcher), nameof(Patcher.FireTruckAI_SetSource_Prefix), typeof(FireTruckAI), nameof(FireTruckAI.SetSource));
        }

        #endregion

        #region METM

        private void Patch_METM(ref bool success)
        {
            success &= AddTranspiler(typeof(Patcher), nameof(Patcher.METMTargetMethodTranspiler), Type.GetType("MoreEffectiveTransfer.Patch.WarehouseAICalculateSpawnPositionPatch"), "TargetMethod");
            success &= AddTranspiler(typeof(Patcher), nameof(Patcher.METMTargetMethodTranspiler), Type.GetType("MoreEffectiveTransfer.Patch.WarehouseAICalculateUnspawnPositionPatch"), "TargetMethod");
            success &= AddTranspiler(typeof(Patcher), nameof(Patcher.METMPostfixTranspiler), Type.GetType("MoreEffectiveTransfer.Patch.WarehouseAICalculateSpawnPositionPatch"), "Postfix");
            success &= AddTranspiler(typeof(Patcher), nameof(Patcher.METMPostfixTranspiler), Type.GetType("MoreEffectiveTransfer.Patch.WarehouseAICalculateUnspawnPositionPatch"), "Postfix");
        }

        #endregion

        #endregion
    }

    public static class Patcher
    {
        public static IEnumerable<CodeInstruction> ToolControllerAwakeTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions) => ModsCommon.Patcher.ToolControllerAwakeTranspiler<Mod, SpawnPointsTool>(generator, instructions);

        public static IEnumerable<CodeInstruction> GameKeyShortcutsEscapeTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions) => ModsCommon.Patcher.GameKeyShortcutsEscapeTranspiler<Mod, SpawnPointsTool>(generator, instructions);

        private static void GetPosition(PointType type, ushort buildingId, ref Building data, ref Randomizer randomizer, VehicleInfo info, out Vector3 position, out Vector3 target)
        {
            if (SingletonManager<Manager>.Instance[buildingId] is not BuildingData buildingData || !buildingData.GetPosition(type, ref data, info, ref randomizer, out position, out target))
            {
                position = data.CalculateSidewalkPosition(0f, 2f);
                target = position;
            }
        }
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
            button.eventTooltipEnter += (_, _) => button.tooltip = $"{SingletonMod<Mod>.Instance.NameRaw} ({SingletonTool<SpawnPointsTool>.Activation})";

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

        private static void GetStartPathFindPosition(PointType type, ushort buildingId, ref Building data, ushort vehicleId, ref Vehicle vehicle, out Vector3 position)
        {
            if (SingletonManager<Manager>.Instance[buildingId] is BuildingData buildingData)
            {
                var randomizer = new Randomizer(vehicleId);
                if (buildingData.GetPosition(type, ref data, vehicle.Info, ref randomizer, out position, out _))
                    return;
            }

            position = data.CalculateSidewalkPosition();
        }
        public static IEnumerable<CodeInstruction> StartPathFind_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            var enumerator = instructions.GetEnumerator();
            var getInstance = AccessTools.PropertyGetter(typeof(Singleton<BuildingManager>), nameof(Singleton<BuildingManager>.instance));
            var sidewalk = AccessTools.Method(typeof(Building), nameof(Building.CalculateSidewalkPosition), new Type[0]);
            var i = 0;

            while (enumerator.MoveNext())
            {
                var instruction = enumerator.Current;
                if (instruction.opcode == OpCodes.Call && instruction.operand == getInstance)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4, (int)PointType.Unspawn);
                    yield return original.GetLDArg("vehicleData");
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Vehicle), i == 0 ? nameof(Vehicle.m_sourceBuilding) : nameof(Vehicle.m_targetBuilding)));

                    for (; instruction != null && (instruction.opcode != OpCodes.Call || instruction.operand != sidewalk); instruction = enumerator.Current)
                    {
                        yield return instruction;
                        enumerator.MoveNext();
                    }

                    yield return original.GetLDArg("vehicleID");
                    yield return original.GetLDArg("vehicleData");
                    yield return new CodeInstruction(OpCodes.Ldloca_S, i);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patcher), nameof(Patcher.GetStartPathFindPosition)));

                    enumerator.MoveNext();

                    i += 1;
                }
                else
                    yield return instruction;
            }
        }
        private delegate void RemoveSourceDelegate(FireTruckAI instance, ushort vehicleID, ref Vehicle data);
        private static RemoveSourceDelegate FireTruckRemoveSource { get; } = AccessTools.MethodDelegate<RemoveSourceDelegate>(AccessTools.Method(typeof(FireTruckAI), "RemoveSource"));
        public static bool FireTruckAI_SetSource_Prefix(FireTruckAI __instance, ushort vehicleID, ref Vehicle data, ushort sourceBuilding)
        {
            FireTruckRemoveSource(__instance, vehicleID, ref data);
            data.m_sourceBuilding = sourceBuilding;

            if (sourceBuilding != 0)
            {
                data.Unspawn(vehicleID);

                var building = sourceBuilding.GetBuilding();
                var randomizer = new Randomizer(vehicleID);
                building.Info.m_buildingAI.CalculateSpawnPosition(sourceBuilding, ref building, ref randomizer, __instance.m_info, out var position, out var target);
                var rotation = Quaternion.identity;
                var forward = target - position;
                if (forward.sqrMagnitude > 0.01f)
                    rotation = Quaternion.LookRotation(forward);

                data.m_frame0 = new Vehicle.Frame(position, rotation);
                data.m_frame1 = data.m_frame0;
                data.m_frame2 = data.m_frame0;
                data.m_frame3 = data.m_frame0;
                data.m_targetPos0 = position;
                data.m_targetPos0.w = 2f;
                data.m_targetPos1 = target;
                data.m_targetPos1.w = 2f;
                data.m_targetPos2 = data.m_targetPos1;
                data.m_targetPos3 = data.m_targetPos1;

                __instance.FrameDataUpdated(vehicleID, ref data, ref data.m_frame0);
                building.AddOwnVehicle(vehicleID, ref data);
            }

            return false;
        }


        public static IEnumerable<CodeInstruction> METMTargetMethodTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var wrongType = typeof(WarehouseAI);
            var needType = typeof(BuildingAI);
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldtoken && instruction.operand == wrongType)
                    instruction.operand = needType;

                yield return instruction;
            }
        }
        public static IEnumerable<CodeInstruction> METMPostfixTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }
}
