using BuildingSpawnPoints.Utilites;
using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using ModsCommon;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace BuildingSpawnPoints
{
    public class BuildingData : IToXml
    {
        public static string XmlName => "B";

        //private static Randomizer Randomizer = new Randomizer(DateTime.Now.Ticks);
        public ushort Id { get; }
        private List<BuildingSpawnPoint> SpawnPoints { get; } = new List<BuildingSpawnPoint>();
        public IEnumerable<BuildingSpawnPoint> Points => SpawnPoints;

        public VehicleType DefaultVehicles => Id.GetBuilding().GetDefaultVehicles();
        public VehicleType NeededVehicles => Id.GetBuilding().GetNeededVehicles();
        public VehicleType PossibleVehicles => Id.GetBuilding().GetPossibleVehicles();

        public VehicleType LostVehicles => SpawnPoints.Aggregate(NeededVehicles, (v, p) => v & ~p.VehicleTypes.Value);
        public VehicleType NotAddedVehicles => SpawnPoints.Aggregate(PossibleVehicles, (v, p) => v & ~p.VehicleTypes.Value);

        public bool IsCorrect => LostVehicles == VehicleType.None;

        public string XmlSection => XmlName;

        public BuildingData(ushort id, bool init = true)
        {
            Id = id;

            if (init)
                ResetToDefault();
        }

        public bool GetPosition(PointType type, ref Building data, VehicleInfo vehicle, ref Randomizer randomizer, out Vector3 position, out Vector3 target)
        {
            var vehicleType = vehicle.GetVehicleType();
            var points = SpawnPoints.Where(p => (p.Type & type) != PointType.None && (p.VehicleTypes & vehicleType) != VehicleType.None).ToArray();

            if (points.Length != 0)
            {
#if DEBUG
                var vehicleId = randomizer.seed;
#endif
                var index = randomizer.Int32((uint)points.Length);
                points[index].GetAbsolute(ref data, out position, out target);
#if DEBUG
                SingletonMod<Mod>.Logger.Debug($"{type} {vehicleType} on building #{Id}; {index + 1} of {points.Length}; {position}");
#endif
                return true;
            }
            else
            {
                position = Vector3.zero;
                target = Vector3.zero;
                return false;
            }
        }

        private void AddPoint(BuildingSpawnPoint point)
        {
            point.OnChanged = OnChanged;
            SpawnPoints.Add(point);
        }
        public BuildingSpawnPoint AddPoint()
        {
            var spawnPoint = new BuildingSpawnPoint(this, Vector3.zero, vehicleType: VehicleType.None);
            AddPoint(spawnPoint);
            return spawnPoint;
        }
        public void DeletePoint(BuildingSpawnPoint point)
        {
            point.OnChanged = null;
            SpawnPoints.Remove(point);
        }
        public BuildingSpawnPoint DuplicatePoint(BuildingSpawnPoint point)
        {
            var copyPoint = point.Copy();
            AddPoint(copyPoint);
            return copyPoint;
        }
        public void ResetToDefault()
        {
            SpawnPoints.Clear();
            AddPoint(new BuildingSpawnPoint(this, Vector3.back * (Id.GetBuilding().Length * 4f + 2f), 0f, DefaultVehicles));
            foreach (var point in this.GetDefaultPoints())
                AddPoint(point);
        }
        public void OnChanged()
        {
            //var vehicleId = Id.GetBuilding().m_ownVehicles;
            //var i = 0;

            //while (vehicleId != 0 && i < VehicleManager.MAX_VEHICLE_COUNT)
            //{
            //    var vehicle = vehicleId.GetVehicle();
            //    vehicle.Info.m_vehicleAI.BuildingRelocated(vehicleId, ref vehicle, Id);

            //    vehicleId = vehicle.m_nextOwnVehicle;
            //    i += 1;
            //}

            //vehicleId = Id.GetBuilding().m_guestVehicles;
            //i = 0;

            //while (vehicleId != 0 && i < VehicleManager.MAX_VEHICLE_COUNT)
            //{
            //    var vehicle = vehicleId.GetVehicle();
            //    vehicle.Info.m_vehicleAI.BuildingRelocated(vehicleId, ref vehicle, Id);

            //    vehicleId = vehicle.m_nextGuestVehicle;
            //    i += 1;
            //}
        }

        public XElement ToXml()
        {
            var config = new XElement(XmlSection);

            config.AddAttr(nameof(Id), Id);

            foreach (var point in Points)
                config.Add(point.ToXml());

            return config;
        }

        public void FromXml(XElement config)
        {
            SpawnPoints.Clear();

            foreach (var pointConfig in config.Elements(BuildingSpawnPoint.XmlName))
            {
                if (BuildingSpawnPoint.FromXml(pointConfig, this, out var point))
                    AddPoint(point);
            }
        }
        public static bool FromXml(XElement config, ObjectsMap map, out BuildingData data)
        {
            var id = config.GetAttrValue(nameof(Id), (ushort)0);

            if (map.TryGetBuilding(id, out var targetId))
                id = targetId;

            if (id != 0 && id <= BuildingManager.MAX_BUILDING_COUNT)
            {
                try
                {
                    data = new BuildingData(id, false);
                    data.FromXml(config);

                    return true;
                }
                catch { }
            }

            data = null;
            return false;
        }
    }
    public class BuildingSpawnPoint : IToXml
    {
        public static string XmlName => "P";

        public BuildingData Data { get; set; }
        public Action OnChanged { get; set; }

        public PropertyULongEnumValue<VehicleType> VehicleTypes { get; }
        public PropertyEnumValue<PointType> Type { get; }
        public PropertyStructValue<Vector4> Position { get; set; }

        public string XmlSection => XmlName;

        private delegate WaterSimulation.Cell[] WaterSimulationCellDelegate(WaterSimulation waterSimulation);
        private static WaterSimulationCellDelegate WaterCellsDelegate { get; set; }
        private static WaterSimulation.Cell[] WaterCells => WaterCellsDelegate.Invoke(TerrainManager.instance.WaterSimulation);
        static BuildingSpawnPoint()
        {
            var definition = new DynamicMethod("GetWaterBuffers", typeof(WaterSimulation.Cell[]), new Type[1] { typeof(WaterSimulation) }, true);
            var generator = definition.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(WaterSimulation), "m_waterBuffers"));
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(WaterSimulation), "m_waterFrameIndex"));
            generator.Emit(OpCodes.Ldc_I4_6);
            generator.Emit(OpCodes.Shr_Un);
            generator.Emit(OpCodes.Not);
            generator.Emit(OpCodes.Ldc_I4_1);
            generator.Emit(OpCodes.And);
            generator.Emit(OpCodes.Conv_U);
            generator.Emit(OpCodes.Ldelem_Ref);
            generator.Emit(OpCodes.Ret);
            WaterCellsDelegate = (WaterSimulationCellDelegate)definition.CreateDelegate(typeof(WaterSimulationCellDelegate));
        }

        private BuildingSpawnPoint(BuildingData data)
        {
            Data = data;

            VehicleTypes = new PropertyULongEnumValue<VehicleType>("V", Changed, Data.DefaultVehicles);
            Type = new PropertyEnumValue<PointType>("T", Changed, PointType.Both);
            Position = new PropertyVector4Value(Changed, Vector4.zero, labelY: "H", labelW: "A");
        }
        public BuildingSpawnPoint(BuildingData data, Vector3 position, float angle = 0f, VehicleType vehicleType = VehicleType.Default, PointType type = PointType.Both) : this(data)
        {
            Init(position.FixZ(), angle, vehicleType, type);
        }
        public BuildingSpawnPoint(BuildingData data, Vector3 position, Vector3 target, VehicleType vehicleType = VehicleType.Default, PointType type = PointType.Both, bool invert = false) : this(data)
        {
            position = position.FixZ();
            target = target.FixZ();
            Init(position, (invert ? position - target : target - position).AbsoluteAngle(), vehicleType, type);
        }

        private void Init(Vector4 position, float angle, VehicleType vehicleType, PointType type)
        {
            position.w = angle;
            Position.Value = position;
            VehicleTypes.Value = vehicleType;
            Type.Value = type;
        }
        private void Changed() => OnChanged?.Invoke();

        public void GetAbsolute(ref Building data, out Vector3 position, out Vector3 target)
        {
            position = data.m_position + (Vector3)Position.Value.TurnRad(data.m_angle, false);
            position.y = GetHeightWithWater(position) + Position.Value.y;
            target = position + Vector3.forward.TurnRad(data.m_angle + Position.Value.w * Mathf.Deg2Rad, false);
        }
        private float GetHeight(Vector3 position)
        {
            var x = position.x / 16f + 540f;
            var z = position.z / 16f + 540f;
            return TerrainManager.instance.SampleFinalHeight(x, z) * 0.015625f;
        }
        private float GetHeightWithWater(Vector3 position)
        {
            var x = position.x / 16f + 540f;
            var z = position.z / 16f + 540f;

            var xMin = Mathf.Clamp((int)x, 0, 1080);
            var xMax = Mathf.Clamp((int)x + 1, 0, 1080);
            var zMin = Mathf.Clamp((int)z, 0, 1080);
            var zMax = Mathf.Clamp((int)z + 1, 0, 1080);
            var xT = x - (int)x;
            var zT = z - (int)z;
            var cells = WaterCells;

            var zMinHeight = Mathf.Lerp(GetSurfaceHeight(xMin, zMin, cells), GetSurfaceHeight(xMax, zMin, cells), xT);
            var zMaxHeight = Mathf.Lerp(GetSurfaceHeight(xMin, zMax, cells), GetSurfaceHeight(xMax, zMax, cells), xT);
            var height = Mathf.Lerp(zMinHeight, zMaxHeight, zT);
            return height * 0.015625f;

            static float GetSurfaceHeight(int x, int z, WaterSimulation.Cell[] cells)
            {
                var waterHeight = cells[z * 1081 + x].m_height;
                if (waterHeight == 0f)
                    return TerrainManager.instance.FinalHeights[z * 1081 + x];
                if (waterHeight < 64f)
                    return TerrainManager.instance.RawHeights2[z * 1081 + x] + Mathf.Max(0f, waterHeight);
                else
                    return TerrainManager.instance.BlockHeights[z * 1081 + x] + waterHeight;
            }
        }

        public BuildingSpawnPoint Copy(BuildingData data = null)
        {
            var copy = new BuildingSpawnPoint(data ?? Data);

            copy.VehicleTypes.Value = VehicleTypes;
            copy.Type.Value = Type;
            copy.Position.Value = Position;

            return copy;
        }

        public XElement ToXml()
        {
            var config = new XElement(XmlSection);

            Type.ToXml(config);
            VehicleTypes.ToXml(config);
            Position.ToXml(config);

            return config;
        }
        public static bool FromXml(XElement config, BuildingData data, out BuildingSpawnPoint point)
        {
            point = new BuildingSpawnPoint(data);

            point.Type.FromXml(config);
            point.VehicleTypes.FromXml(config);
            point.Position.FromXml(config);

            return true;
        }
    }

    public enum PointType
    {
        [NotVisible]
        None = 0,

        [Description(nameof(Localize.PointType_Spawn))]
        Spawn = 1,

        [Description(nameof(Localize.PointType_Unspawn))]
        Unspawn = 2,

        [NotVisible]
        Both = Spawn | Unspawn,
    }
    public struct VehicleLaneData
    {
        public ItemClass.Service Service;
        public NetInfo.LaneType Lane;
        public VehicleInfo.VehicleType Type;
        public float Distance;

        public VehicleLaneData(ItemClass.Service service, NetInfo.LaneType lane, VehicleInfo.VehicleType type, float distance)
        {
            Service = service;
            Lane = lane;
            Type = type;
            Distance = distance;
        }
        private static Dictionary<VehicleService, VehicleLaneData> Dictinary { get; } = new Dictionary<VehicleService, VehicleLaneData>()
        {
            {VehicleService.Car, new VehicleLaneData(ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, 32f) },
            {VehicleService.Trolleybus, new VehicleLaneData(ItemClass.Service.Road, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Trolleybus, 32f) },
            {VehicleService.Tram, new VehicleLaneData(ItemClass.Service.Road, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Tram, 32f) },
            {VehicleService.Plane, new VehicleLaneData(ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Plane, 16f) },
            {VehicleService.Balloon, new VehicleLaneData(ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Balloon, 64f) },
            {VehicleService.Blimp, new VehicleLaneData(ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Blimp, 64f) },
            {VehicleService.Ship, new VehicleLaneData(ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Ship, 64f) },
            {VehicleService.Ferry, new VehicleLaneData(ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Ferry, 64f) },
            {VehicleService.Fishing, new VehicleLaneData(ItemClass.Service.Fishing, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Ship, 40f) },
            {VehicleService.Train, new VehicleLaneData(ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Train, 32f) },
            {VehicleService.Metro, new VehicleLaneData(ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Metro, 32f) },
            {VehicleService.Monorail, new VehicleLaneData(ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Monorail, 32f) },
            {VehicleService.CableCar, new VehicleLaneData(ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.CableCar, 32f) },
        };

        public static VehicleLaneData Get(VehicleService type) => Dictinary[type];
    }
}
