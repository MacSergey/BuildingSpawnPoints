using ColossalFramework;
using ColossalFramework.Math;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
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

            AddPoint();
        }

        public void GetPosition(ref Building data, VehicleInfo vehicle, ref Randomizer randomizer, out Vector3 position, out Vector3 target)
        {
            var vehicleType = vehicle.GetVehicleType();
            var points = SpawnPoints.Where(p => (p.VehicleType & vehicleType) != VehicleType.None).ToArray();

            if (points.Length != 0)
            {
                var point = points[randomizer.Int32((uint)points.Length)];
                position = point.GetAbsolutePosition(ref data);
                target = position;
            }
            else
            {
                position = data.CalculateSidewalkPosition(0f, 2f);
                target = position;
            }
        }

        public BuildingSpawnPoint AddPoint()
        {
            var spawnPoint = new BuildingSpawnPoint()
            {
                Angle = 0f,
                Position = Vector3.forward * (Id.GetBuilding().Length * 4f + 2f),
                VehicleType = VehicleType.All,
            };
            SpawnPoints.Add(spawnPoint);

            return spawnPoint;
        }
        public void DeletePoint(BuildingSpawnPoint point) => SpawnPoints.Remove(point);
    }
    public class BuildingSpawnPoint
    {
        public VehicleType VehicleType { get; set; }
        public Vector3 Position { get; set; }
        public float Angle { get; set; }

        public Vector3 GetAbsolutePosition(ref Building data) => data.m_position + Position.TurnRad(data.m_angle, false);
    }

    public static class VehicleExtension
    {
        private static Dictionary<Type, VehicleType> Map { get; } = new Dictionary<Type, VehicleType>
        {
            {typeof(CargoPlaneAI),VehicleType.CargoPlane },
            {typeof(PassengerPlaneAI),VehicleType.PassengerPlane },
            {typeof(BalloonAI),VehicleType.Balloon },
            {typeof(BicycleAI),VehicleType.Bicycle },
            {typeof(PassengerBlimpAI),VehicleType.PassengerBlimp },
            {typeof(AmbulanceAI),VehicleType.Ambulance },
            {typeof(BusAI),VehicleType.Bus },
            {typeof(CargoTruckAI),VehicleType.CargoTruck },
            {typeof(DisasterResponseVehicleAI),VehicleType.Disaster },
            {typeof(FireTruckAI),VehicleType.FireTruck },
            {typeof(GarbageTruckAI),VehicleType.GarbageTruck },
            {typeof(HearseAI),VehicleType.Hearse },
            {typeof(MaintenanceTruckAI),VehicleType.MaintenanceTruck },
            {typeof(ParkMaintenanceVehicleAI),VehicleType.Park },
            {typeof(PoliceCarAI),VehicleType.Police },
            {typeof(PostVanAI),VehicleType.Post },
            {typeof(SnowTruckAI),VehicleType.SnowTruck },
            {typeof(TaxiAI),VehicleType.Taxi },
            {typeof(TrolleybusAI),VehicleType.Trolleybus },
            {typeof(WaterTruckAI),VehicleType.WaterTruck },
            {typeof(FishingBoatAI),VehicleType.FishingBoat },
            {typeof(PassengerFerryAI),VehicleType.PassengerFerry },
            {typeof(AmbulanceCopterAI),VehicleType.AmbulanceCopter },
            {typeof(DisasterResponseCopterAI),VehicleType.DisasterCopter },
            {typeof(FireCopterAI),VehicleType.FireCopter },
            {typeof(PoliceCopterAI),VehicleType.PoliceCopter },
            {typeof(PassengerHelicopterAI),VehicleType.PassengerCopter },
            {typeof(PrivatePlaneAI),VehicleType.PrivatePlane },
            {typeof(CargoShipAI),VehicleType.CargoShip },
            {typeof(PassengerShipAI),VehicleType.PassengerShip },
            {typeof(CargoTrainAI),VehicleType.CargoTrain },
            {typeof(PassengerTrainAI),VehicleType.PassengerTrain },
            {typeof(MetroTrainAI),VehicleType.MetroTrain },
            {typeof(TramAI),VehicleType.Tram },
        };
        public static VehicleType GetVehicleType(this VehicleInfo info) => Map.TryGetValue(info.m_vehicleAI.GetType(), out var type) ? type : VehicleType.All;

    }
    public static class BuildingExtension
    {
        public static ref Building GetBuilding(this ushort id) => ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[id];
    }
    [Flags]
    public enum VehicleType : ulong
    {
        None = 0ul,

        CargoTruck = 1ul,
        FireTruck = 1ul << 1,
        SnowTruck = 1ul << 2,
        WaterTruck = 1ul << 3,
        GarbageTruck = 1ul << 4,
        MaintenanceTruck = 1ul << 5,

        Ambulance = 1ul << 6,
        Disaster = 1ul << 7,
        Hearse = 1ul << 8,
        Park = 1ul << 9,
        Police = 1ul << 10,
        Post = 1ul << 11,

        Bus = 1ul << 12,
        Taxi = 1ul << 13,
        Trolleybus = 1ul << 14,
        Bicycle = 1ul << 15,

        CargoPlane = 1ul << 16,
        PassengerPlane = 1ul << 17,
        PrivatePlane = 1ul << 18,
        PassengerCopter = 1ul << 19,
        Balloon = 1ul << 20,
        PassengerBlimp = 1ul << 21,
        AmbulanceCopter = 1ul << 22,
        DisasterCopter = 1ul << 23,
        FireCopter = 1ul << 24,
        PoliceCopter = 1ul << 25,

        FishingBoat = 1ul << 26,
        PassengerFerry = 1ul << 27,
        CargoShip = 1ul << 28,
        PassengerShip = 1ul << 29,

        CargoTrain = 1ul << 30,
        PassengerTrain = 1ul << 31,
        MetroTrain = 1ul << 32,
        Tram = 1ul << 33,

        Passenger = Bus | Taxi | Trolleybus | Bicycle | PassengerPlane | PassengerCopter | PassengerBlimp | PassengerFerry | PassengerShip | PassengerTrain | MetroTrain | Tram,
        Service = FireTruck | SnowTruck | WaterTruck | GarbageTruck | MaintenanceTruck | Ambulance | Disaster | Hearse | Park | Police | Post,

        Plane = CargoPlane | PassengerPlane | PrivatePlane,
        Copter = PassengerCopter | AmbulanceCopter | DisasterCopter | FireCopter | PoliceCopter,
        Air = Plane | Copter | Balloon | PassengerBlimp,

        Water = FishingBoat | PassengerFerry | CargoShip | PassengerShip,

        Rail = CargoTrain | PassengerTrain | MetroTrain | Tram,

        Cargo = CargoTruck | CargoPlane | CargoShip | CargoTrain,
        Truck = CargoTruck | FireTruck | SnowTruck | WaterTruck | GarbageTruck | MaintenanceTruck,
        Car = Truck | Ambulance | Bus | Disaster | Hearse | Park | Police | Post | Taxi | Trolleybus,

        PassengerCar = Passenger & Car,
        PassengerAir = Passenger & Air,
        PassengerWater = Passenger & Water,
        PassengerRail = Passenger & Rail,

        All = ulong.MaxValue,
    }
}
