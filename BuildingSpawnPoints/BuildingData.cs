using ColossalFramework;
using ColossalFramework.Math;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BuildingSpawnPoints
{
    public class BuildingData
    {
        public ushort Id { get; }
        private List<BuildingSpawnPoint> SpawnPoints { get; } = new List<BuildingSpawnPoint>();
        public IEnumerable<BuildingSpawnPoint> Points => SpawnPoints;

        public BuildingData(ushort id)
        {
            Id = id;

            var building = id.GetBuilding();
            SpawnPoints.Add(new BuildingSpawnPoint(Vector3.back * (id.GetBuilding().Length * 4f + 2f), 0f, building.GetDefaultVehicles()));
            SpawnPoints.AddRange(building.Info.GetDefaultPoints());
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
            var spawnPoint = new BuildingSpawnPoint(Vector3.zero, vehicleType: VehicleType.None);
            SpawnPoints.Add(spawnPoint);
            return spawnPoint;
        }
        public void DeletePoint(BuildingSpawnPoint point) => SpawnPoints.Remove(point);
    }
    public class BuildingSpawnPoint
    {
        public VehicleType VehicleTypes { get; set; }
        public PointType Type { get; set; }
        public Vector3 Position { get; set; }
        public float Angle { get; set; }

        public BuildingSpawnPoint(Vector3 position, float angle = 0f, VehicleType vehicleType = VehicleType.Default, PointType type = PointType.Both)
        {
            Fix(ref position);
            Init(position, angle, vehicleType, type);
        }
        public BuildingSpawnPoint(Vector3 position, Vector3 target, VehicleType vehicleType = VehicleType.Default, PointType type = PointType.Both, bool invert = false)
        {
            Fix(ref position);
            Fix(ref target);
            Init(position, (invert ? position - target : target - position).AbsoluteAngle(), vehicleType, type);
        }
        public BuildingSpawnPoint(Vector3 position, Vector3 target, VehicleInfo.VehicleType vehicleType, PointType type = PointType.Both, bool invert = false) : this(position, target, vehicleType.GetVehicleType(), type, invert) { }

        private void Init(Vector3 position, float angle, VehicleType vehicleType, PointType type)
        {
            Position = position;
            Angle = angle;
            VehicleTypes = vehicleType;
            Type = type;
        }
        private void Fix(ref Vector3 vector) => vector.z = -vector.z;

        public void GetAbsolute(ref Building data, out Vector3 position, out Vector3 target)
        {
            position = data.m_position + Position.TurnRad(data.m_angle, false);
            target = position + Vector3.forward.TurnRad(data.m_angle + Angle * Mathf.Deg2Rad, false);
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
