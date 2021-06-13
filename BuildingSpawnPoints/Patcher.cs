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

        private static CodeInstruction Building_CalculateSidewalkPosition { get; } = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Building), nameof(Building.CalculateSidewalkPosition), new Type[0]));
        private static CodeInstruction Building_Position { get; } = new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Building), nameof(Building.m_position)));


        public static IEnumerable<CodeInstruction> Service_StartPathFind_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => StartPathFind_Transpiler(instructions, original, new Dictionary<int, StartPathFindInfo>()
        {
            { 1, new StartPathFindInfo(true, 0, Building_CalculateSidewalkPosition)},
            { 2, new StartPathFindInfo(false, 1, Building_CalculateSidewalkPosition)},
        });

        public static IEnumerable<CodeInstruction> Bus_StartPathFind_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => StartPathFind_Transpiler(instructions, original, new Dictionary<int, StartPathFindInfo>()
        {
            { 1, new StartPathFindInfo(true, 0, Building_CalculateSidewalkPosition)},
        });
        public static IEnumerable<CodeInstruction> Train_StartPathFind_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => StartPathFind_Transpiler(instructions, original, new Dictionary<int, StartPathFindInfo>()
        {
            { 1, new StartPathFindInfo(true, 2, Building_Position)},
            { 2, new StartPathFindInfo(false, 3, Building_Position)},
            { 3, new StartPathFindInfo(false, 4, Building_Position)},
        });
        public static IEnumerable<CodeInstruction> Plane_StartPathFind_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => StartPathFind_Transpiler(instructions, original, new Dictionary<int, StartPathFindInfo>()
        {
            { 1, new StartPathFindInfo(true, 0, Building_Position)},
            { 2, new StartPathFindInfo(false, 1, Building_Position)},
            { 3, new StartPathFindInfo(false, 2, Building_Position)},
        });
        public static IEnumerable<CodeInstruction> Blimp_StartPathFind_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => StartPathFind_Transpiler(instructions, original, new Dictionary<int, StartPathFindInfo>()
        {
            { 1, new StartPathFindInfo(true, 0, Building_CalculateSidewalkPosition)},
            { 2, new StartPathFindInfo(false, 1, Building_Position)},
        });
        public static IEnumerable<CodeInstruction> Copter_StartPathFind_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => StartPathFind_Transpiler(instructions, original, new Dictionary<int, StartPathFindInfo>()
        {
            { 1, new StartPathFindInfo(true, 0, Building_CalculateSidewalkPosition)},
            { 2, new StartPathFindInfo(false, 1, Building_Position)},
        });


        private static IEnumerable<CodeInstruction> StartPathFind_Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original, Dictionary<int, StartPathFindInfo> toPatch)
        {
            var enumerator = instructions.GetEnumerator();
            var getInstance = AccessTools.PropertyGetter(typeof(Singleton<BuildingManager>), nameof(Singleton<BuildingManager>.instance));
            var count = 0;

            while (enumerator.MoveNext())
            {
                var instruction = enumerator.Current;
                if (instruction.opcode == OpCodes.Call && instruction.operand == getInstance)
                {
                    count += 1;

                    if (toPatch.TryGetValue(count, out var info))
                    {
                        yield return new CodeInstruction(OpCodes.Ldc_I4, (int)PointType.Unspawn);
                        yield return original.GetLDArg("vehicleData");
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Vehicle), info.IsSource ? nameof(Vehicle.m_sourceBuilding) : nameof(Vehicle.m_targetBuilding)));

                        for (; instruction != null && (instruction.opcode != info.Stop.opcode || instruction.operand != info.Stop.operand); instruction = enumerator.Current)
                        {
                            yield return instruction;
                            enumerator.MoveNext();
                        }

                        yield return original.GetLDArg("vehicleID");
                        yield return original.GetLDArg("vehicleData");
                        yield return new CodeInstruction(OpCodes.Ldloca_S, info.Index);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patcher), nameof(Patcher.GetStartPathFindPosition)));

                        enumerator.MoveNext();
                        continue;
                    }
                }

                yield return instruction;
            }
        }

        public delegate void RemoveSourceDelegate<TypeAI>(TypeAI instance, ushort vehicleID, ref Vehicle data) where TypeAI : VehicleAI;

        private static RemoveSourceDelegate<FireTruckAI> FireTruckRemoveSource { get; } = AccessTools.MethodDelegate<RemoveSourceDelegate<FireTruckAI>>(AccessTools.Method(typeof(FireTruckAI), "RemoveSource"));
        private static RemoveSourceDelegate<DisasterResponseVehicleAI> DisasterResponseRemoveSource { get; } = AccessTools.MethodDelegate<RemoveSourceDelegate<DisasterResponseVehicleAI>>(AccessTools.Method(typeof(DisasterResponseVehicleAI), "RemoveSource"));
        private static RemoveSourceDelegate<BalloonAI> BalloonRemoveSource { get; } = AccessTools.MethodDelegate<RemoveSourceDelegate<BalloonAI>>(AccessTools.Method(typeof(BalloonAI), "RemoveSource"));

        public static bool FireTruckAI_SetSource_Prefix(FireTruckAI __instance, ushort vehicleID, ref Vehicle data, ushort sourceBuilding) => VehicleAISetSource(__instance, vehicleID, ref data, sourceBuilding, FireTruckRemoveSource);
        public static bool DisasterResponseAI_SetSource_Prefix(DisasterResponseVehicleAI __instance, ushort vehicleID, ref Vehicle data, ushort sourceBuilding) => VehicleAISetSource(__instance, vehicleID, ref data, sourceBuilding, DisasterResponseRemoveSource);
        public static bool BalloonAI_SetSource_Prefix(BalloonAI __instance, ushort vehicleID, ref Vehicle data, ushort sourceBuilding) => VehicleAISetSource(__instance, vehicleID, ref data, sourceBuilding, BalloonRemoveSource);

        private static bool VehicleAISetSource<TypeAI>(TypeAI instance, ushort vehicleID, ref Vehicle data, ushort sourceBuilding, RemoveSourceDelegate<TypeAI> removeSource)
            where TypeAI : VehicleAI
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

        private struct StartPathFindInfo
        {
            public bool IsSource;
            public int Index;
            public CodeInstruction Stop;

            public StartPathFindInfo(bool isSource, int index, CodeInstruction stop)
            {
                IsSource = isSource;
                Index = index;
                Stop = stop;
            }
        }
    }
}
