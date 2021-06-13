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
            return AddPrefix(typeof(Patcher), nameof(Patcher.BuildingAI_CalculateSpawnPosition_Prefix), typeof(BuildingAI), nameof(BuildingAI.CalculateSpawnPosition), parameters);
        }
        private bool Patch_BuildingAI_CalculateUnspawnPosition(Type[] parameters)
        {
            return AddPrefix(typeof(Patcher), nameof(Patcher.BuildingAI_CalculateUnpawnPosition_Prefix), typeof(BuildingAI), nameof(BuildingAI.CalculateUnspawnPosition), parameters);
        }

        private bool Patch_CalculateSpawnPosition(Type type, Type[] parameters)
        {
            return AddPrefix(typeof(Patcher), nameof(Patcher.CalculateSpawnPosition_Prefix), type, nameof(BuildingAI.CalculateSpawnPosition), parameters);
        }
        private bool Patch_CalculateUnspawnPosition(Type type, Type[] parameters)
        {
            return AddPrefix(typeof(Patcher), nameof(Patcher.CalculateUnspawnPosition_Prefix), type, nameof(BuildingAI.CalculateUnspawnPosition), parameters);
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
            success &= Patch_BusAI_StartPathFind(startPathFindParams);

            success &= Patch_FireTruckAI_SetSource();
            success &= Patch_DisasterResponseVehicleAI_SetSource();
        }

        private bool Patch_PoliceCarAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.Service_StartPathFind_Transpiler), typeof(PoliceCarAI), "StartPathFind", parameters);
        }
        private bool Patch_FireTruckAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.Service_StartPathFind_Transpiler), typeof(FireTruckAI), "StartPathFind", parameters);
        }
        private bool Patch_DisasterResponseVehicleAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.Service_StartPathFind_Transpiler), typeof(DisasterResponseVehicleAI), "StartPathFind", parameters);
        }
        private bool Patch_ParkMaintenanceVehicleAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.Service_StartPathFind_Transpiler), typeof(ParkMaintenanceVehicleAI), "StartPathFind", parameters);
        }
        private bool Patch_BusAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.PublicTransport_StartPathFind_Transpiler), typeof(BusAI), "StartPathFind", parameters);
        }

        private bool Patch_FireTruckAI_SetSource()
        {
            return AddPrefix(typeof(Patcher), nameof(Patcher.FireTruckAI_SetSource_Prefix), typeof(FireTruckAI), nameof(FireTruckAI.SetSource));
        }
        private bool Patch_DisasterResponseVehicleAI_SetSource()
        {
            return AddPrefix(typeof(Patcher), nameof(Patcher.DisasterResponseAI_SetSource_Prefix), typeof(DisasterResponseVehicleAI), nameof(DisasterResponseVehicleAI.SetSource));
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


        private delegate bool FindParkingSpaceDelegate(BuildingAI instance, ushort buildingID, ref Building data, ref Randomizer randomizer, VehicleInfo.VehicleType type, bool isElectric, out Vector3 position, out Vector3 target);
        private static FindParkingSpaceDelegate FindParkingSpace { get; } = AccessTools.MethodDelegate<FindParkingSpaceDelegate>(AccessTools.Method(typeof(BuildingAI), "FindParkingSpace", new Type[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(Randomizer).MakeByRefType(), typeof(VehicleInfo.VehicleType), typeof(bool), typeof(Vector3).MakeByRefType(), typeof(Vector3).MakeByRefType() }));

        private static void CalculatePosition(PointType type, BuildingAI instance, ushort buildingId, ref Building data, ref Randomizer randomizer, VehicleInfo info, out Vector3 position, out Vector3 target)
        {
            if (SingletonManager<Manager>.Instance[buildingId] is not BuildingData buildingData || !buildingData.GetPosition(type, ref data, info, ref randomizer, out position, out target))
            {
                if (info.m_vehicleType != VehicleInfo.VehicleType.Helicopter || !FindParkingSpace(instance, buildingId, ref data, ref randomizer, info.m_vehicleType, false, out position, out target))
                {
                    position = data.CalculateSidewalkPosition(0f, 2f);
                    target = position;
                }
            }
        }
        public static bool BuildingAI_CalculateSpawnPosition_Prefix(BuildingAI __instance, ushort buildingID, ref Building data, ref Randomizer randomizer, VehicleInfo info, out Vector3 position, out Vector3 target)
        {
            CalculatePosition(PointType.Spawn, __instance, buildingID, ref data, ref randomizer, info, out position, out target);
            return false;
        }
        public static bool BuildingAI_CalculateUnpawnPosition_Prefix(BuildingAI __instance, ushort buildingID, ref Building data, ref Randomizer randomizer, VehicleInfo info, out Vector3 position, out Vector3 target)
        {
            CalculatePosition(PointType.Unspawn, __instance, buildingID, ref data, ref randomizer, info, out position, out target);
            return false;
        }

        public static bool CalculateSpawnPosition_Prefix(ushort buildingID, ref Building data, ref Randomizer randomizer, VehicleInfo info, ref Vector3 position, ref Vector3 target)
        {
            return SingletonManager<Manager>.Instance[buildingID] is not BuildingData buildingData || !buildingData.GetPosition(PointType.Spawn, ref data, info, ref randomizer, out position, out target);
        }
        public static bool CalculateUnspawnPosition_Prefix(ushort buildingID, ref Building data, ref Randomizer randomizer, VehicleInfo info, ref Vector3 position, ref Vector3 target)
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
        public static IEnumerable<CodeInstruction> Service_StartPathFind_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => StartPathFind_Transpiler(instructions, original, new Dictionary<int, byte>() { { 1, 128 }, { 2, 1 } });
        public static IEnumerable<CodeInstruction> PublicTransport_StartPathFind_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => StartPathFind_Transpiler(instructions, original, new Dictionary<int, byte>() { { 1, 128 } });


        private static IEnumerable<CodeInstruction> StartPathFind_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original, Dictionary<int, byte> toPatch)
        {
            var enumerator = instructions.GetEnumerator();
            var getInstance = AccessTools.PropertyGetter(typeof(Singleton<BuildingManager>), nameof(Singleton<BuildingManager>.instance));
            var sidewalk = AccessTools.Method(typeof(Building), nameof(Building.CalculateSidewalkPosition), new Type[0]);
            var count = 0;

            while (enumerator.MoveNext())
            {
                var instruction = enumerator.Current;
                if (instruction.opcode == OpCodes.Call && instruction.operand == getInstance)
                {
                    count += 1;

                    if (toPatch.TryGetValue(count, out var j))
                    {
                        yield return new CodeInstruction(OpCodes.Ldc_I4, (int)PointType.Unspawn);
                        yield return original.GetLDArg("vehicleData");
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Vehicle), (j & 128) != 0 ? nameof(Vehicle.m_sourceBuilding) : nameof(Vehicle.m_targetBuilding)));

                        for (; instruction != null && (instruction.opcode != OpCodes.Call || instruction.operand != sidewalk); instruction = enumerator.Current)
                        {
                            yield return instruction;
                            enumerator.MoveNext();
                        }

                        yield return original.GetLDArg("vehicleID");
                        yield return original.GetLDArg("vehicleData");
                        yield return new CodeInstruction(OpCodes.Ldloca_S, j & 127);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patcher), nameof(Patcher.GetStartPathFindPosition)));

                        enumerator.MoveNext();
                        continue;
                    }
                }

                yield return instruction;
            }
        }

        public delegate void RemoveSourceDelegate<TypeAI>(TypeAI instance, ushort vehicleID, ref Vehicle data) where TypeAI : CarAI;

        private static RemoveSourceDelegate<FireTruckAI> FireTruckRemoveSource { get; } = AccessTools.MethodDelegate<RemoveSourceDelegate<FireTruckAI>>(AccessTools.Method(typeof(FireTruckAI), "RemoveSource"));
        private static RemoveSourceDelegate<DisasterResponseVehicleAI> DisasterResponseRemoveSource { get; } = AccessTools.MethodDelegate<RemoveSourceDelegate<DisasterResponseVehicleAI>>(AccessTools.Method(typeof(DisasterResponseVehicleAI), "RemoveSource"));

        public static bool FireTruckAI_SetSource_Prefix(FireTruckAI __instance, ushort vehicleID, ref Vehicle data, ushort sourceBuilding) => CarAISetSource(__instance, vehicleID, ref data, sourceBuilding, FireTruckRemoveSource);
        public static bool DisasterResponseAI_SetSource_Prefix(DisasterResponseVehicleAI __instance, ushort vehicleID, ref Vehicle data, ushort sourceBuilding) => CarAISetSource(__instance, vehicleID, ref data, sourceBuilding, DisasterResponseRemoveSource);

        public static bool CarAISetSource<TypeAI>(TypeAI instance, ushort vehicleID, ref Vehicle data, ushort sourceBuilding, RemoveSourceDelegate<TypeAI> removeSource)
            where TypeAI : CarAI
        {
            removeSource(instance, vehicleID, ref data);
            data.m_sourceBuilding = sourceBuilding;

            if (sourceBuilding != 0)
            {
                data.Unspawn(vehicleID);

                var building = sourceBuilding.GetBuilding();
                var randomizer = new Randomizer(vehicleID);
                building.Info.m_buildingAI.CalculateSpawnPosition(sourceBuilding, ref building, ref randomizer, instance.m_info, out var position, out var target);
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

                instance.FrameDataUpdated(vehicleID, ref data, ref data.m_frame0);
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
