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
            SpawnPoints.Add(new BuildingSpawnPoint(Vector3.back * (id.GetBuilding().Length * 4f + 2f)));
            SpawnPoints.AddRange(building.Info.GetDefaultPoints());
        }

        public void GetPosition(PointType type, ref Building data, VehicleInfo vehicle, ref Randomizer randomizer, out Vector3 position, out Vector3 target)
        {
            var vehicleType = vehicle.GetVehicleType();
            var points = SpawnPoints.Where(p => (p.Type & type) != PointType.None && (p.VehicleTypes & vehicleType) != VehicleType.None).ToArray();

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
            var spawnPoint = new BuildingSpawnPoint(Vector3.zero);
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
    public static class AIExtension
    {
        private static bool InvertTraffic => Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic == SimulationMetaData.MetaBool.True;
        private static Dictionary<Type, VehicleType> VehicleAllowMap { get; } = new Dictionary<Type, VehicleType>
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
        private static HashSet<Type> VehicleForbiddenMap { get; } = new HashSet<Type>();

        public static VehicleType GetVehicleType(this VehicleInfo info)
        {
            var type = info.m_vehicleAI.GetType();

            if (VehicleAllowMap.TryGetValue(type, out var vehicleType))
                return vehicleType;
            else if (VehicleForbiddenMap.Contains(type))
                return VehicleType.None;
            else if (typeof(VehicleAI).IsAssignableFrom(type))
            {
                var parentType = type.BaseType;
                while (parentType != null)
                {
                    if (VehicleAllowMap.TryGetValue(parentType, out vehicleType))
                    {
                        VehicleAllowMap.Add(type, vehicleType);
                        break;
                    }
                }
                return vehicleType;
            }
            else
            {
                VehicleForbiddenMap.Add(type);
                return VehicleType.None;
            }
        }

        public static VehicleType GetVehicleTypes(this BuildingInfo info)
        {
            var type = VehicleType.Default;

            switch (info.m_buildingAI)
            {
                case CargoStationAI cargoStation:
                    {
                        if (cargoStation.m_transportInfo is TransportInfo info1)
                            type |= info1.m_vehicleType.GetVehicleType() & VehicleType.Cargo;
                        if (cargoStation.m_transportInfo2 is TransportInfo info2)
                            type |= info2.m_vehicleType.GetVehicleType() & VehicleType.Cargo;
                        break;
                    }
                case DepotAI depot:
                    {
                        if (depot.m_transportInfo is TransportInfo info1)
                            type |= info1.m_vehicleType.GetVehicleType() & VehicleType.Passenger;
                        if (depot.m_secondaryTransportInfo is TransportInfo info2)
                            type |= info2.m_vehicleType.GetVehicleType() & VehicleType.Passenger;
                        break;
                    }
                case FishingHarborAI:
                    type |= VehicleType.FishingBoat;
                    break;
                case ShelterAI shelter:
                    type |= shelter.m_transportInfo.m_vehicleType.GetVehicleType();
                    break;
                case TaxiStandAI taxiStand:
                    type |= taxiStand.m_transportInfo.m_vehicleType.GetVehicleType();
                    break;
                case PrivateAirportAI:
                    type |= VehicleType.PrivatePlane;
                    break;
            }

            return type;
        }
        public static IEnumerable<BuildingSpawnPoint> GetDefaultPoints(this BuildingInfo info) => info.m_buildingAI switch
        {
            CargoStationAI cargoStation => cargoStation.GetPoints(),
            DepotAI depot => depot.GetPoints(),
            FishingHarborAI fishingHarbor => fishingHarbor.GetPoints(),
            ShelterAI shelter => shelter.GetPoints(),
            TaxiStandAI taxiStand => taxiStand.GetPoints(),
            PrivateAirportAI privateAirport => privateAirport.GetPoints(),
            _ => new BuildingSpawnPoint[0],
        };


        public static IEnumerable<BuildingSpawnPoint> GetPoints(this CargoStationAI cargoStation)
        {
            yield return new BuildingSpawnPoint(cargoStation.m_truckSpawnPosition, 0f, VehicleType.CargoTruck | VehicleType.Post, InvertTraffic ? PointType.Unspawn : PointType.Spawn);
            yield return new BuildingSpawnPoint(cargoStation.m_truckUnspawnPosition, 0f, VehicleType.CargoTruck | VehicleType.Post, InvertTraffic ? PointType.Spawn : PointType.Unspawn);

            if (cargoStation.m_transportInfo is TransportInfo info1)
            {
                var vehicleType = info1.m_vehicleType.GetVehicleType() & VehicleType.Cargo;
                yield return new BuildingSpawnPoint(cargoStation.m_spawnPosition, cargoStation.m_spawnTarget, vehicleType, invert: cargoStation.m_canInvertTarget && InvertTraffic);
            }

            if (cargoStation.m_transportInfo2 is TransportInfo info2)
            {
                var vehicleType = info2.m_vehicleType.GetVehicleType() & VehicleType.Cargo;
                yield return new BuildingSpawnPoint(cargoStation.m_spawnPosition2, cargoStation.m_spawnTarget2, vehicleType, invert: cargoStation.m_canInvertTarget2 && InvertTraffic);
            }
        }

        public static IEnumerable<BuildingSpawnPoint> GetPoints(this DepotAI depot)
        {
            if (depot.m_transportInfo is TransportInfo info1)
            {
                var invert = depot.m_canInvertTarget && InvertTraffic;
                var vehicleType = info1.m_vehicleType.GetVehicleType() & VehicleType.Passenger;

                if (depot.m_spawnPoints != null && depot.m_spawnPoints.Length != 0)
                {
                    foreach (var point in depot.m_spawnPoints)
                        yield return new BuildingSpawnPoint(point.m_position, point.m_target, vehicleType, invert: invert);
                }
                else
                    yield return new BuildingSpawnPoint(depot.m_spawnPosition, depot.m_spawnTarget, vehicleType, invert: invert);
            }
            if (depot.m_secondaryTransportInfo is TransportInfo info2)
            {
                var invert = depot.m_canInvertTarget2 && InvertTraffic;
                var vehicleType = info2.m_vehicleType.GetVehicleType() & VehicleType.Passenger;

                if (depot.m_spawnPoints2 != null && depot.m_spawnPoints2.Length != 0)
                {
                    foreach (var point in depot.m_spawnPoints2)
                        yield return new BuildingSpawnPoint(point.m_position, point.m_target, vehicleType, invert: invert);
                }
                else
                    yield return new BuildingSpawnPoint(depot.m_spawnPosition2, depot.m_spawnTarget2, vehicleType, invert: invert);
            }
        }
        public static IEnumerable<BuildingSpawnPoint> GetPoints(this FishingHarborAI fishingHarbor)
        {
            yield return new BuildingSpawnPoint(fishingHarbor.m_boatSpawnPosition, fishingHarbor.m_boatSpawnTarget, VehicleType.FishingBoat);
        }
        public static IEnumerable<BuildingSpawnPoint> GetPoints(this ShelterAI shelter)
        {
            var invert = shelter.m_canInvertTarget && InvertTraffic;
            yield return new BuildingSpawnPoint(shelter.m_spawnPosition, shelter.m_spawnTarget, shelter.m_transportInfo.m_vehicleType, invert: invert);
        }
        public static IEnumerable<BuildingSpawnPoint> GetPoints(this TaxiStandAI taxiStand)
        {
            yield return new BuildingSpawnPoint(taxiStand.m_queueStartPos, taxiStand.m_queueEndPos, taxiStand.m_transportInfo.m_vehicleType, invert: InvertTraffic);
        }
        public static IEnumerable<BuildingSpawnPoint> GetPoints(this PrivateAirportAI privateAirport)
        {
            foreach (var runway in privateAirport.m_runways)
                yield return new BuildingSpawnPoint(runway.m_position, runway.m_target, VehicleType.PrivatePlane);
        }

        public static VehicleType GetVehicleType(this VehicleInfo.VehicleType vehicleType) => vehicleType switch
        {
            VehicleInfo.VehicleType.Car => VehicleType.Default,
            VehicleInfo.VehicleType.Metro => VehicleType.MetroTrain,
            VehicleInfo.VehicleType.Train => VehicleType.Trains,
            VehicleInfo.VehicleType.Ship => VehicleType.CargoShip | VehicleType.PassengerShip,
            VehicleInfo.VehicleType.Plane => VehicleType.CargoPlane | VehicleType.PassengerPlane,
            VehicleInfo.VehicleType.Bicycle => VehicleType.Bicycle,
            VehicleInfo.VehicleType.Tram => VehicleType.Tram,
            VehicleInfo.VehicleType.Helicopter => VehicleType.Copters,
            VehicleInfo.VehicleType.Ferry => VehicleType.PassengerFerry,
            VehicleInfo.VehicleType.Monorail => VehicleType.Monorail,
            VehicleInfo.VehicleType.CableCar => VehicleType.CableCar,
            VehicleInfo.VehicleType.Blimp => VehicleType.PassengerBlimp,
            VehicleInfo.VehicleType.Balloon => VehicleType.Balloon,
            VehicleInfo.VehicleType.Rocket => VehicleType.Rocket,
            VehicleInfo.VehicleType.Trolleybus => VehicleType.Trolleybus,
            _ => VehicleType.Default,
        };
    }
    public static class BuildingExtension
    {
        public static ref Building GetBuilding(this ushort id) => ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[id];
    }
    [Flags]
    public enum VehicleType : ulong
    {
        [NotItem]
        None = 0ul,


        [Description(nameof(Localize.VehicleType_Ambulance))]
        Ambulance = 1ul,

        [Description(nameof(Localize.VehicleType_Hearse))]
        Hearse = 1ul << 1,

        [Description(nameof(Localize.VehicleType_Police))]
        Police = 1ul << 2,

        [Description(nameof(Localize.VehicleType_Post))]
        Post = 1ul << 3,

        [Description(nameof(Localize.VehicleType_Disaster))]
        Disaster = 1ul << 4,

        [Description(nameof(Localize.VehicleType_Park))]
        Park = 1ul << 5,


        [Description(nameof(Localize.VehicleType_Bus))]
        Bus = 1ul << 6,

        [Description(nameof(Localize.VehicleType_Trolleybus))]
        Trolleybus = 1ul << 7,

        [Description(nameof(Localize.VehicleType_Taxi))]
        Taxi = 1ul << 8,

        [Description(nameof(Localize.VehicleType_Bicycle))]
        Bicycle = 1ul << 9,



        [Description(nameof(Localize.VehicleType_CargoTruck))]
        CargoTruck = 1ul << 10,

        [Description(nameof(Localize.VehicleType_FireTruck))]
        FireTruck = 1ul << 11,

        [Description(nameof(Localize.VehicleType_SnowTruck))]
        SnowTruck = 1ul << 12,

        [Description(nameof(Localize.VehicleType_WaterTruck))]
        WaterTruck = 1ul << 13,

        [Description(nameof(Localize.VehicleType_GarbageTruck))]
        GarbageTruck = 1ul << 14,

        [Description(nameof(Localize.VehicleType_MaintenanceTruck))]
        MaintenanceTruck = 1ul << 15,


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

        [Description(nameof(Localize.VehicleType_Monorail))]
        Monorail = 1ul << 33,

        [Description(nameof(Localize.VehicleType_Tram))]
        Tram = 1ul << 34,



        [Description(nameof(Localize.VehicleType_Rocket))]
        Rocket = 1ul << 35,

        [Description(nameof(Localize.VehicleType_CableCar))]
        CableCar = 1ul << 36,



        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Passenger))]
        Passenger = Bus | Taxi | Trolleybus | Bicycle | PassengerPlane | PassengerCopter | PassengerBlimp | PassengerFerry | PassengerShip | PassengerTrain | MetroTrain | Monorail | Tram,

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
        [Description(nameof(Localize.VehicleTypeGroup_Ships))]
        Ships = CargoShip | PassengerShip,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Air))]
        Air = Planes | Copters | Balloon | PassengerBlimp,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Water))]
        Water = FishingBoat | PassengerFerry | CargoShip | PassengerShip,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Rail))]
        Rail = CargoTrain | PassengerTrain | MetroTrain | Monorail | Tram,

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
        [Description(nameof(Localize.VehicleTypeGroup_Default))]
        Default = Ambulance | Hearse | Police | Post | Taxi | GarbageTruck | CargoTruck | Disaster | MaintenanceTruck | SnowTruck,

        [NotItem]
        [Description(nameof(Localize.VehicleType_All))]
        All = ulong.MaxValue,
    }

    public enum VehicleTypeGroupA : ulong
    {
        Trucks = VehicleType.Trucks,
        Trains = VehicleType.Trains,
        Planes = VehicleType.Planes,
        Copters = VehicleType.Copters,
        Ships = VehicleType.Ships,
    }
    public enum VehicleTypeGroupB : ulong
    {
        Road = VehicleType.Road,
        Rail = VehicleType.Rail,
        Air = VehicleType.Air,
        Water = VehicleType.Water,
    }
    public enum VehicleTypeGroupC : ulong
    {
        Passenger = VehicleType.Passenger,
        Service = VehicleType.Service,
        Cargo = VehicleType.Cargo,
    }
    public enum VehicleTypeGroupD : ulong
    {
        PassengerRoad = VehicleType.PassengerRoad,
        PassengerRail = VehicleType.PassengerRail,
        PassengerAir = VehicleType.PassengerAir,
        PassengerWater = VehicleType.PassengerWater,      
    }
}
