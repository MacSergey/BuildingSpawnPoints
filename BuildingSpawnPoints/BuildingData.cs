using BuildingSpawnPoints.Utilites;
using ColossalFramework;
using ColossalFramework.Math;
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

        public ushort Id { get; }
        private List<BuildingSpawnPoint> SpawnPoints { get; } = new List<BuildingSpawnPoint>();
        public IEnumerable<BuildingSpawnPoint> Points => SpawnPoints;
        public VehicleType DefaultVehicles => Id.GetBuilding().GetDefaultVehicles();
        public VehicleType PossibleVehicles => Id.GetBuilding().GetPossibleVehicles();

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
                var index = randomizer.Int32((uint)points.Length);
                points[index].GetAbsolute(ref data, out position, out target);
                return true;
            }
            else
            {
                position = Vector3.zero;
                target = Vector3.zero;
                return false;
            }
        }

        public BuildingSpawnPoint AddPoint()
        {
            var spawnPoint = new BuildingSpawnPoint(this, Vector3.zero, vehicleType: VehicleType.None);
            SpawnPoints.Add(spawnPoint);
            return spawnPoint;
        }
        public void DeletePoint(BuildingSpawnPoint point) => SpawnPoints.Remove(point);
        public BuildingSpawnPoint DuplicatePoint(BuildingSpawnPoint point)
        {
            var copyPoint = point.Copy();
            SpawnPoints.Add(copyPoint);
            return copyPoint;
        }
        public void ResetToDefault()
        {
            SpawnPoints.Clear();
            SpawnPoints.Add(new BuildingSpawnPoint(this, Vector3.back * (Id.GetBuilding().Length * 4f + 2f), 0f, DefaultVehicles));
            SpawnPoints.AddRange(this.GetDefaultPoints());
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
                    SpawnPoints.Add(point);
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

        public VehicleType VehicleTypes { get; set; }
        public PointType Type { get; set; }
        public Vector4 Position { get; set; }

        public string XmlSection => XmlName;

        private BuildingSpawnPoint(BuildingData data)
        {
            Data = data;
        }
        public BuildingSpawnPoint(BuildingData data, Vector3 position, float angle = 0f, VehicleType vehicleType = VehicleType.Default, PointType type = PointType.Both) : this(data)
        {
            Fix(ref position);
            Init(position, angle, vehicleType, type);
        }
        public BuildingSpawnPoint(BuildingData data, Vector3 position, Vector3 target, VehicleType vehicleType = VehicleType.Default, PointType type = PointType.Both, bool invert = false) : this(data)
        {
            Fix(ref position);
            Fix(ref target);
            Init(position, (invert ? position - target : target - position).AbsoluteAngle(), vehicleType, type);
        }
        public BuildingSpawnPoint(BuildingData data, Vector3 position, Vector3 target, VehicleInfo.VehicleType vehicleType, PointType type = PointType.Both, bool invert = false) : this(data, position, target, vehicleType.GetVehicleType(), type, invert) { }

        private void Init(Vector4 position, float angle, VehicleType vehicleType, PointType type)
        {
            position.w = angle;
            Position = position;
            VehicleTypes = vehicleType;
            Type = type;
        }
        private void Fix(ref Vector3 vector) => vector.z = -vector.z;

        public void GetAbsolute(ref Building data, out Vector3 position, out Vector3 target)
        {
            position = data.m_position + (Vector3)Position.TurnRad(data.m_angle, false);
            target = position + Vector3.forward.TurnRad(data.m_angle + Position.w * Mathf.Deg2Rad, false);
        }
        public BuildingSpawnPoint Copy() => new BuildingSpawnPoint(Data)
        {
            VehicleTypes = VehicleTypes,
            Type = Type,
            Position = Position,
        };

        public XElement ToXml()
        {
            var config = new XElement(XmlSection);

            config.AddAttr("T", (int)Type);
            config.AddAttr("V", (int)VehicleTypes);
            config.AddAttr("X", Position.x);
            config.AddAttr("Z", Position.z);
            config.AddAttr("A", Position.w);

            return config;
        }
        public static bool FromXml(XElement config, BuildingData data, out BuildingSpawnPoint point)
        {
            point = new BuildingSpawnPoint(data);

            point.Type = (PointType)config.GetAttrValue("T", (int)PointType.Both);
            point.VehicleTypes = (VehicleType)config.GetAttrValue("V", (int)data.DefaultVehicles);

            var x = config.GetAttrValue("X", 0f);
            var z = config.GetAttrValue("Z", 0f);
            var w = config.GetAttrValue("A", 0f);
            point.Position = new Vector4(x, 0f, z, w);

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

}
