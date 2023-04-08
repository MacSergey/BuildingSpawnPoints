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

        public ushort Id { get; }
        private List<BuildingSpawnPoint> SpawnPoints { get; } = new List<BuildingSpawnPoint>();
        public IEnumerable<BuildingSpawnPoint> Points => SpawnPoints;

        public VehicleCategory DefaultVehicles => Id.GetBuilding().GetDefaultVehicles();
        public VehicleCategory NeededVehicles => Id.GetBuilding().GetNeededVehicles();
        public VehicleCategory PossibleVehicles => Id.GetBuilding().GetPossibleVehicles();

        public VehicleCategory LostVehicles => SpawnPoints.Aggregate(NeededVehicles, (v, p) => v & ~p.Categories.Value);
        public VehicleCategory NotAddedVehicles => SpawnPoints.Aggregate(PossibleVehicles, (v, p) => v & ~p.Categories.Value);

        public bool IsCorrect => LostVehicles == VehicleCategory.None;

        public string XmlSection => XmlName;

        public BuildingData(ushort id, bool init = true)
        {
            Id = id;

            if (init)
                ResetToDefault();
        }

        public bool GetPosition(PointType type, ref Building data, VehicleInfo vehicle, ref Randomizer randomizer, out Vector3 position, out Vector3 target)
        {
            var category = vehicle.GetVehicleCategory();
            var points = SpawnPoints.Where(p => (p.Type & type) != PointType.None && (p.Categories & category) != VehicleCategory.None).ToArray();

            if (points.Length != 0)
            {
#if DEBUG
                var vehicleId = randomizer.seed;
#endif
                var index = randomizer.Int32((uint)points.Length);
                points[index].GetAbsolute(ref data, out position, out target);
#if DEBUG
                SingletonMod<Mod>.Logger.Debug($"{type} {category} on building #{Id}; {index + 1} of {points.Length}; {position}");
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
            var spawnPoint = new BuildingSpawnPoint(this, Vector3.zero, vehicleType: VehicleCategory.None);
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

        public void FromXml(Version version, XElement config)
        {
            SpawnPoints.Clear();

            foreach (var pointConfig in config.Elements(BuildingSpawnPoint.XmlName))
            {
                if (BuildingSpawnPoint.FromXml(version, pointConfig, this, out var point))
                    AddPoint(point);
            }
        }
        public static bool FromXml(Version version, XElement config, ObjectsMap map, out BuildingData data)
        {
            var id = config.GetAttrValue(nameof(Id), (ushort)0);

            if (map.TryGetBuilding(id, out var targetId))
                id = targetId;

            if (id != 0 && id <= BuildingManager.MAX_BUILDING_COUNT)
            {
                try
                {
                    data = new BuildingData(id, false);
                    data.FromXml(version, config);

                    return true;
                }
                catch { }
            }

            data = null;
            return false;
        }
    }
}
