using BuildingSpawnPoints.Utilites;
using ColossalFramework;
using ColossalFramework.Math;
using ModsCommon;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
                SingletonMod<Mod>.Logger.Debug($"{type} {vehicleType} on building #{Id}; {index+1} of {points.Length}; {position}");
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

        public BuildingData Data { get; }
        public Action OnChanged { get; set; }

        public PropertyULongEnumValue<VehicleType> VehicleTypes { get; }
        public PropertyEnumValue<PointType> Type { get; }
        public PropertyStructValue<Vector4> Position { get; set; }

        public string XmlSection => XmlName;

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
            target = position + Vector3.forward.TurnRad(data.m_angle + Position.Value.w * Mathf.Deg2Rad, false);
        }
        public BuildingSpawnPoint Copy()
        {
            var copy = new BuildingSpawnPoint(Data);

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

        //[Description(nameof(Localize.PointType_Middle))]
        //Middle = 4,

        [NotVisible]
        Both = Spawn | Unspawn,

        //[NotVisible]
        //All = Both | Middle,
    }

}
