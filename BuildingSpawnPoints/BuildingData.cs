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

            AddPoint();
        }

        public void GetPosition(PointType type, ref Building data, VehicleInfo vehicle, ref Randomizer randomizer, out Vector3 position, out Vector3 target)
        {
            var vehicleType = vehicle.GetVehicleType();
            var points = SpawnPoints.Where(p => (p.Type & type) != PointType.None && (p.VehicleType & vehicleType) != VehicleType.None).ToArray();

            if (points.Length != 0)
            {
                var point = points[randomizer.Int32((uint)points.Length)];
                point.GetAbsolute(ref data, out position, out target);
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
                Type = PointType.Both,
            };
            SpawnPoints.Add(spawnPoint);

            return spawnPoint;
        }
        public void DeletePoint(BuildingSpawnPoint point) => SpawnPoints.Remove(point);
    }
    public class BuildingSpawnPoint
    {
        public VehicleType VehicleType { get; set; }
        public PointType Type { get; set; }
        public Vector3 Position { get; set; }
        public float Angle { get; set; }

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
        [NotItem]
        [Description(nameof(Localize.VehicleType_None))]
        None = 0ul,

        [Description(nameof(Localize.VehicleType_CargoTruck))]
        CargoTruck = 1ul,

        [Description(nameof(Localize.VehicleType_FireTruck))]
        FireTruck = 1ul << 1,

        [Description(nameof(Localize.VehicleType_SnowTruck))]
        SnowTruck = 1ul << 2,

        [Description(nameof(Localize.VehicleType_WaterTruck))]
        WaterTruck = 1ul << 3,

        [Description(nameof(Localize.VehicleType_GarbageTruck))]
        GarbageTruck = 1ul << 4,

        [Description(nameof(Localize.VehicleType_MaintenanceTruck))]
        MaintenanceTruck = 1ul << 5,



        [Description(nameof(Localize.VehicleType_Ambulance))]
        Ambulance = 1ul << 6,

        [Description(nameof(Localize.VehicleType_Disaster))]
        Disaster = 1ul << 7,

        [Description(nameof(Localize.VehicleType_Hearse))]
        Hearse = 1ul << 8,

        [Description(nameof(Localize.VehicleType_Park))]
        Park = 1ul << 9,

        [Description(nameof(Localize.VehicleType_Police))]
        Police = 1ul << 10,

        [Description(nameof(Localize.VehicleType_Post))]
        Post = 1ul << 11,



        [Description(nameof(Localize.VehicleType_Bus))]
        Bus = 1ul << 12,

        [Description(nameof(Localize.VehicleType_Taxi))]
        Taxi = 1ul << 13,

        [Description(nameof(Localize.VehicleType_Trolleybus))]
        Trolleybus = 1ul << 14,

        [Description(nameof(Localize.VehicleType_Bicycle))]
        Bicycle = 1ul << 15,



        [Description(nameof(Localize.VehicleType_CargoPlane))]
        CargoPlane = 1ul << 16,

        [Description(nameof(Localize.VehicleType_PassengerPlane))]
        PassengerPlane = 1ul << 17,

        [Description(nameof(Localize.VehicleType_PrivatePlane))]
        PrivatePlane = 1ul << 18,

        [Description(nameof(Localize.VehicleType_PassengerCopter))]
        PassengerCopter = 1ul << 19,

        [Description(nameof(Localize.VehicleType_Balloon))]
        Balloon = 1ul << 20,

        [Description(nameof(Localize.VehicleType_PassengerBlimp))]
        PassengerBlimp = 1ul << 21,

        [Description(nameof(Localize.VehicleType_AmbulanceCopter))]
        AmbulanceCopter = 1ul << 22,

        [Description(nameof(Localize.VehicleType_DisasterCopter))]
        DisasterCopter = 1ul << 23,

        [Description(nameof(Localize.VehicleType_FireCopter))]
        FireCopter = 1ul << 24,

        [Description(nameof(Localize.VehicleType_PoliceCopter))]
        PoliceCopter = 1ul << 25,



        [Description(nameof(Localize.VehicleType_FishingBoat))]
        FishingBoat = 1ul << 26,

        [Description(nameof(Localize.VehicleType_PassengerFerry))]
        PassengerFerry = 1ul << 27,

        [Description(nameof(Localize.VehicleType_CargoShip))]
        CargoShip = 1ul << 28,

        [Description(nameof(Localize.VehicleType_PassengerShip))]
        PassengerShip = 1ul << 29,



        [Description(nameof(Localize.VehicleType_CargoTrain))]
        CargoTrain = 1ul << 30,

        [Description(nameof(Localize.VehicleType_PassengerTrain))]
        PassengerTrain = 1ul << 31,

        [Description(nameof(Localize.VehicleType_MetroTrain))]
        MetroTrain = 1ul << 32,

        [Description(nameof(Localize.VehicleType_Tram))]
        Tram = 1ul << 33,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Passenger))]
        Passenger = Bus | Taxi | Trolleybus | Bicycle | PassengerPlane | PassengerCopter | PassengerBlimp | PassengerFerry | PassengerShip | PassengerTrain | MetroTrain | Tram,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Service))]
        Service = FireTruck | SnowTruck | WaterTruck | GarbageTruck | MaintenanceTruck | Ambulance | Disaster | Hearse | Park | Police | Post,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Planes))]
        Planes = CargoPlane | PassengerPlane | PrivatePlane,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Copters))]
        Copters = PassengerCopter | AmbulanceCopter | DisasterCopter | FireCopter | PoliceCopter,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Trains))]
        Trains = CargoTrain | PassengerTrain,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Air))]
        Air = Planes | Copters | Balloon | PassengerBlimp,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Water))]
        Water = FishingBoat | PassengerFerry | CargoShip | PassengerShip,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Rail))]
        Rail = CargoTrain | PassengerTrain | MetroTrain | Tram,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Cargo))]
        Cargo = CargoTruck | CargoPlane | CargoShip | CargoTrain,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Trucks))]
        Trucks = CargoTruck | FireTruck | SnowTruck | WaterTruck | GarbageTruck | MaintenanceTruck,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Road))]
        Road = Trucks | Ambulance | Bus | Disaster | Hearse | Park | Police | Post | Taxi | Trolleybus,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_PassengerRoad))]
        PassengerRoad = Passenger & Road,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_PassengerAir))]
        PassengerAir = Passenger & Air,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_PassengerWater))]
        PassengerWater = Passenger & Water,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_PassengerRail))]
        PassengerRail = Passenger & Rail,

        [NotItem]
        [Description(nameof(Localize.VehicleType_All))]
        All = ulong.MaxValue,
    }
}
