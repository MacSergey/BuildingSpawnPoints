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
    public static class AIExtension
    {
        private static bool InvertTraffic => Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic == SimulationMetaData.MetaBool.True;
        private static Dictionary<Type, VehicleCategory> VehicleAllow { get; } = new Dictionary<Type, VehicleCategory>
        {
            //Service
            { typeof(AmbulanceAI),VehicleCategory.Ambulance },
            { typeof(PoliceCarAI),VehicleCategory.Police },
            { typeof(FireTruckAI),VehicleCategory.FireTruck },
            { typeof(HearseAI),VehicleCategory.Hearse },
            { typeof(CargoTruckAI),VehicleCategory.CargoTruck },

            //Transport
            { typeof(BicycleAI),VehicleCategory.Bicycle },
            { typeof(TrolleybusAI),VehicleCategory.Trolleybus },
            { typeof(BusAI),VehicleCategory.Bus },
            { typeof(TaxiAI),VehicleCategory.Taxi },
            { typeof(TramAI),VehicleCategory.Tram },
            { typeof(PostVanAI),VehicleCategory.PostTruck },

            //Maintenance
            { typeof(GarbageTruckAI),VehicleCategory.GarbageTruck },
            { typeof(MaintenanceTruckAI),VehicleCategory.RoadTruck },
            { typeof(ParkMaintenanceVehicleAI),VehicleCategory.ParkTruck },
            { typeof(SnowTruckAI),VehicleCategory.SnowTruck },
            { typeof(WaterTruckAI),VehicleCategory.VacuumTruck },
            { typeof(DisasterResponseVehicleAI),VehicleCategory.Disaster },
            { typeof(BankVanAI),VehicleCategory.BankTruck },

            //Trains
            { typeof(PassengerTrainAI),VehicleCategory.PassengerTrain },
            { typeof(CargoTrainAI),VehicleCategory.CargoTrain },
            { typeof(MetroTrainAI),VehicleCategory.MetroTrain },

            //Planes
            { typeof(CargoPlaneAI),VehicleCategory.CargoPlane },
            { typeof(PassengerPlaneAI),VehicleCategory.PassengerPlane },
            { typeof(PrivatePlaneAI),VehicleCategory.PrivatePlane },

            //Copters
            { typeof(AmbulanceCopterAI),VehicleCategory.AmbulanceCopter },
            { typeof(DisasterResponseCopterAI),VehicleCategory.DisasterCopter },
            { typeof(FireCopterAI),VehicleCategory.FireCopter },
            { typeof(PoliceCopterAI),VehicleCategory.PoliceCopter },
            { typeof(PassengerHelicopterAI),VehicleCategory.PassengerCopter },

            //Air
            { typeof(BalloonAI),VehicleCategory.PassengerBalloon },
            { typeof(PassengerBlimpAI),VehicleCategory.PassengerBlimp },

            //Ships
            { typeof(PassengerShipAI),VehicleCategory.PassengerShip },
            { typeof(CargoShipAI),VehicleCategory.CargoShip },
            { typeof(PassengerFerryAI),VehicleCategory.PassengerFerry },
            { typeof(FishingBoatAI),VehicleCategory.FishingBoat },
        };
        private static HashSet<Type> VehicleForbidden { get; } = new HashSet<Type>();

        private static Dictionary<Type, VehicleCategory> BuildingAllow { get; } = new Dictionary<Type, VehicleCategory>
        {
            //Common
            { typeof(CommonBuildingAI), VehicleCategory.Default },

            //PrivateBuilding
            { typeof(CommercialBuildingAI), VehicleCategory.Default |VehicleCategory.CargoTruck | VehicleCategory.BankTruck },
            { typeof(IndustrialBuildingAI), VehicleCategory.Default | VehicleCategory.CargoTruck },
            { typeof(IndustrialExtractorAI), VehicleCategory.Default |VehicleCategory.CargoTruck },
            { typeof(OfficeBuildingAI), VehicleCategory.Default | VehicleCategory.PostTruck },
            { typeof(ResidentialBuildingAI), VehicleCategory.Default | VehicleCategory.PostTruck },

            //Service
            { typeof(CemeteryAI), VehicleCategory.Default },
            { typeof(ChildcareAI), VehicleCategory.Default },
            { typeof(EldercareAI), VehicleCategory.Default },
            { typeof(FireStationAI), VehicleCategory.Default },
            { typeof(HospitalAI), VehicleCategory.Default },
            { typeof(MedicalCenterAI), VehicleCategory.Default },
            { typeof(LibraryAI), VehicleCategory.Default },
            { typeof(PoliceStationAI), VehicleCategory.Default },
            { typeof(SaunaAI), VehicleCategory.Default },
            { typeof(SchoolAI), VehicleCategory.Default },

            //Park
            { typeof(ParkAI), VehicleCategory.Default },
            { typeof(ParkBuildingAI), VehicleCategory.Default | VehicleCategory.ParkTruck },
            { typeof(ParkGateAI), VehicleCategory.Default | VehicleCategory.ParkTruck },

            //Industrial
            { typeof(FishFarmAI), VehicleCategory.Default },
            { typeof(FishingHarborAI), VehicleCategory.Default | VehicleCategory.CargoTruck },
            { typeof(WarehouseAI), VehicleCategory.Default | VehicleCategory.CargoTruck },

            { typeof(MainIndustryBuildingAI), VehicleCategory.Default },
            { typeof(AuxiliaryBuildingAI), VehicleCategory.Default },
            { typeof(ExtractingFacilityAI), VehicleCategory.Default | VehicleCategory.CargoTruck },
            { typeof(ProcessingFacilityAI), VehicleCategory.Default | VehicleCategory.CargoTruck },
            { typeof(UniqueFactoryAI), VehicleCategory.Default | VehicleCategory.CargoTruck },

            //Campus
            { typeof(MainCampusBuildingAI), VehicleCategory.Default },

            //Maintenance
            { typeof(MaintenanceDepotAI), VehicleCategory.Default | VehicleCategory.RoadTruck },
            { typeof(LandfillSiteAI), VehicleCategory.Default | VehicleCategory.Cargo },
            { typeof(SnowDumpAI), VehicleCategory.Default | VehicleCategory.SnowTruck },
            { typeof(WaterFacilityAI), VehicleCategory.Default | VehicleCategory.VacuumTruck },

            //Transport
            { typeof(CargoStationAI), VehicleCategory.Default },
            { typeof(CargoHarborAI), VehicleCategory.Default },
            { typeof(PrivateAirportAI), VehicleCategory.Default },
            { typeof(TaxiStandAI), VehicleCategory.Default & ~VehicleCategory.Taxi },
            { typeof(PostOfficeAI), VehicleCategory.Default },
            { typeof(BankOfficeAI), VehicleCategory.Default },

            //Depot
            { typeof(DepotAI), VehicleCategory.Default },
            { typeof(CableCarStationAI), VehicleCategory.Default },
            { typeof(TransportStationAI), VehicleCategory.Default },
            { typeof(HarborAI), VehicleCategory.Default },
          
            //Disaster
            { typeof(DisasterResponseBuildingAI), VehicleCategory.Default | VehicleCategory.Disaster },
            { typeof(DoomsdayVaultAI), VehicleCategory.Default | VehicleCategory.Disaster | VehicleCategory.CargoTruck },
            { typeof(ShelterAI), VehicleCategory.Default | VehicleCategory.CargoTruck },
            
            //Other
            { typeof(PowerPlantAI), VehicleCategory.Default | VehicleCategory.CargoTruck },
            { typeof(HeatingPlantAI), VehicleCategory.Default | VehicleCategory.CargoTruck },
            { typeof(MonumentAI), VehicleCategory.Default | VehicleCategory.PostTruck},
            { typeof(ServicePointAI), VehicleCategory.Default | VehicleCategory.CargoTruck | VehicleCategory.PostTruck | VehicleCategory.BankTruck}


            //{ typeof(), VehicleType.Default },
        };
        private static HashSet<Type> BuildingForbidden { get; } = new HashSet<Type>();


        public static VehicleCategory GetVehicleCategory(this VehicleInfo info) => GetVehicleType(info.m_vehicleAI.GetType(), VehicleAllow, VehicleForbidden);

        public static VehicleCategory GetDefaultVehicleTypes(this BuildingInfo info) => GetVehicleType(info.m_buildingAI.GetType(), BuildingAllow, BuildingForbidden);

        public static VehicleCategory GetNeededVehicleTypes(this BuildingInfo info)
        {
            var vehicleTypes = info.GetDefaultVehicleTypes();

            switch (info.m_buildingAI)
            {
                case DepotAI depot:
                    {
                        if (depot.m_transportInfo is TransportInfo info1)
                            vehicleTypes |= info1.GetVehicleType() & VehicleCategory.Passenger;
                        if (depot.m_secondaryTransportInfo is TransportInfo info2)
                            vehicleTypes |= info2.GetVehicleType() & VehicleCategory.Passenger;
                        break;
                    }

                case CargoStationAI cargoStation:
                    {
                        if (cargoStation.m_transportInfo is TransportInfo info1)
                            vehicleTypes |= info1.GetVehicleType() & VehicleCategory.Cargo;
                        if (cargoStation.m_transportInfo2 is TransportInfo info2)
                            vehicleTypes |= info2.GetVehicleType() & VehicleCategory.Cargo;
                        break;
                    }

                case TaxiStandAI:
                    vehicleTypes |= VehicleCategory.Taxi;
                    break;

                case PostOfficeAI:
                    vehicleTypes |= VehicleCategory.PostTruck;
                    break;

                case BankOfficeAI:
                    vehicleTypes |= VehicleCategory.BankTruck;
                    break;

                case HelicopterDepotAI:
                    vehicleTypes |= info.GetCopterType();
                    break;

                case FishingHarborAI:
                    vehicleTypes |= VehicleCategory.FishingBoat;
                    break;

                case TourBuildingAI tourBuilding:
                    {
                        if (tourBuilding.m_transportInfo is TransportInfo transportInfo)
                            vehicleTypes |= transportInfo.GetVehicleType() & VehicleCategory.Passenger;
                        break;
                    }
                case ShelterAI shelter:
                    vehicleTypes |= shelter.m_transportInfo.GetVehicleType();
                    break;
            }

            return vehicleTypes;
        }
        public static VehicleCategory GetPossibleVehicleTypes(this BuildingInfo info)
        {
            var vehicleTypes = info.GetNeededVehicleTypes();

            if (info.m_buildingAI is not HelicopterDepotAI)
                vehicleTypes |= VehicleCategory.AmbulanceCopter | VehicleCategory.PoliceCopter | VehicleCategory.DisasterCopter;

            return vehicleTypes;
        }

        private static VehicleCategory GetVehicleType(Type type, Dictionary<Type, VehicleCategory> allow, HashSet<Type> forbidden)
        {
            if (allow.TryGetValue(type, out var vehicleType))
                return vehicleType;
            else if (forbidden.Contains(type))
                return VehicleCategory.None;
            else
            {
                for (var parentType = type.BaseType; parentType != null; parentType = parentType.BaseType)
                {
                    if (allow.TryGetValue(parentType, out vehicleType))
                    {
                        allow.Add(type, vehicleType);
                        return vehicleType;
                    }
                }

                forbidden.Add(type);
                return VehicleCategory.None;
            }
        }

        public static IEnumerable<BuildingSpawnPoint> GetDefaultPoints(this BuildingData data) => data.Id.GetBuilding().Info.m_buildingAI switch
        {
            CargoStationAI cargoStation => cargoStation.GetPoints(data),
            DepotAI depot => depot.GetPoints(data),
            TaxiStandAI taxiStand => taxiStand.GetPoints(data),
            PostOfficeAI postOffice => postOffice.GetPoints(data),
            BankOfficeAI bankOffice => bankOffice.GetPoints(data),
            MaintenanceDepotAI maintenanceDepot => maintenanceDepot.GetPoints(data),
            TourBuildingAI tourBuilding => tourBuilding.GetPoints(data),
            FishingHarborAI fishingHarbor => fishingHarbor.GetPoints(data),
            ShelterAI shelter => shelter.GetPoints(data),
            ServicePointAI servicePoint => servicePoint.GetPoints(data),
            _ => GetCopterPoints(data),
        };

        private static IEnumerable<BuildingSpawnPoint> GetPoints(this CargoStationAI cargoStation, BuildingData data)
        {
            //yield return new BuildingSpawnPoint(data, cargoStation.m_truckSpawnPosition, 0f, VehicleType.CargoTruck | VehicleType.PostTruck, InvertTraffic ? PointType.Unspawn : PointType.Spawn);
            //yield return new BuildingSpawnPoint(data, cargoStation.m_truckUnspawnPosition, 0f, VehicleType.CargoTruck | VehicleType.PostTruck, InvertTraffic ? PointType.Spawn : PointType.Unspawn);

            if (cargoStation.m_transportInfo is TransportInfo info1)
            {
                var vehicleType = info1.GetVehicleType() & VehicleCategory.Cargo;
                if (vehicleType != VehicleCategory.None)
                    yield return new BuildingSpawnPoint(data, cargoStation.m_spawnPosition, cargoStation.m_spawnTarget, vehicleType, invert: cargoStation.m_canInvertTarget && InvertTraffic);
            }

            if (cargoStation.m_transportInfo2 is TransportInfo info2)
            {
                var vehicleType = info2.GetVehicleType() & VehicleCategory.Cargo;
                if (vehicleType != VehicleCategory.None)
                    yield return new BuildingSpawnPoint(data, cargoStation.m_spawnPosition2, cargoStation.m_spawnTarget2, vehicleType, invert: cargoStation.m_canInvertTarget2 && InvertTraffic);
            }
        }
        private static IEnumerable<BuildingSpawnPoint> GetPoints(this DepotAI depot, BuildingData data)
        {
            if (depot.m_transportInfo is TransportInfo info1)
            {
                var invert = depot.m_canInvertTarget && InvertTraffic;
                var vehicleType = info1.GetVehicleType() & VehicleCategory.Passenger;

                if (vehicleType != VehicleCategory.None)
                {
                    if (depot.m_spawnPoints != null && depot.m_spawnPoints.Length != 0)
                    {
                        foreach (var point in depot.m_spawnPoints)
                            yield return new BuildingSpawnPoint(data, point.m_position, point.m_target, vehicleType, invert: invert);
                    }
                    else
                        yield return new BuildingSpawnPoint(data, depot.m_spawnPosition, depot.m_spawnTarget, vehicleType, invert: invert);
                }
            }
            if (depot.m_secondaryTransportInfo is TransportInfo info2)
            {
                var invert = depot.m_canInvertTarget2 && InvertTraffic;
                var vehicleType = info2.GetVehicleType() & VehicleCategory.Passenger;

                if (vehicleType != VehicleCategory.None)
                {
                    if (depot.m_spawnPoints2 != null && depot.m_spawnPoints2.Length != 0)
                    {
                        foreach (var point in depot.m_spawnPoints2)
                            yield return new BuildingSpawnPoint(data, point.m_position, point.m_target, vehicleType, invert: invert);
                    }
                    else
                        yield return new BuildingSpawnPoint(data, depot.m_spawnPosition2, depot.m_spawnTarget2, vehicleType, invert: invert);
                }
            }

            foreach (var point in GetCopterPoints(data))
                yield return point;
        }
        private static IEnumerable<BuildingSpawnPoint> GetPoints(this FishingHarborAI fishingHarbor, BuildingData data)
        {
            yield return new BuildingSpawnPoint(data, fishingHarbor.m_boatSpawnPosition, fishingHarbor.m_boatSpawnTarget, VehicleCategory.FishingBoat, PointType.Spawn);
            yield return new BuildingSpawnPoint(data, fishingHarbor.m_boatUnspawnPosition, fishingHarbor.m_boatUnspawnTarget, VehicleCategory.FishingBoat, PointType.Unspawn);
        }
        private static IEnumerable<BuildingSpawnPoint> GetPoints(this ShelterAI shelter, BuildingData data)
        {
            var invert = shelter.m_canInvertTarget && InvertTraffic;
            yield return new BuildingSpawnPoint(data, shelter.m_spawnPosition, shelter.m_spawnTarget, shelter.m_transportInfo.GetVehicleType(), invert: invert);
        }
        private static IEnumerable<BuildingSpawnPoint> GetPoints(this ServicePointAI servicePoint, BuildingData data)
        {
            foreach(var spawnPoint in servicePoint.m_spawnPoints)
                yield return new BuildingSpawnPoint(data, spawnPoint.m_position, spawnPoint.m_target, (VehicleCategory)spawnPoint.vehicleCategory);
        }
        private static IEnumerable<BuildingSpawnPoint> GetPoints(this TaxiStandAI taxiStand, BuildingData data)
        {
            yield return new BuildingSpawnPoint(data, taxiStand.m_queueStartPos, taxiStand.m_queueEndPos, VehicleCategory.Taxi, invert: InvertTraffic);
        }
        private static IEnumerable<BuildingSpawnPoint> GetPoints(this PostOfficeAI postOffice, BuildingData data)
        {
            yield return new BuildingSpawnPoint(data, postOffice.m_truckSpawnPosition, 0f, VehicleCategory.PostTruck, InvertTraffic ? PointType.Unspawn : PointType.Spawn);
            yield return new BuildingSpawnPoint(data, postOffice.m_truckUnspawnPosition, 0f, VehicleCategory.PostTruck, InvertTraffic ? PointType.Spawn : PointType.Unspawn);
        }
        private static IEnumerable<BuildingSpawnPoint> GetPoints(this BankOfficeAI bankOffice, BuildingData data)
        {
            yield return new BuildingSpawnPoint(data, bankOffice.m_truckSpawnPosition, 0f, VehicleCategory.BankTruck, InvertTraffic ? PointType.Unspawn : PointType.Spawn);
            yield return new BuildingSpawnPoint(data, bankOffice.m_truckUnspawnPosition, 0f, VehicleCategory.BankTruck, InvertTraffic ? PointType.Spawn : PointType.Unspawn);
        }
        private static IEnumerable<BuildingSpawnPoint> GetPoints(this MaintenanceDepotAI maintenanceDepot, BuildingData data)
        {
            yield return new BuildingSpawnPoint(data, maintenanceDepot.m_spawnPosition, maintenanceDepot.m_spawnTarget, VehicleCategory.RoadTruck, PointType.Both);
        }
        private static IEnumerable<BuildingSpawnPoint> GetPoints(this TourBuildingAI tourBuilding, BuildingData data)
        {
            if (tourBuilding.m_transportInfo is TransportInfo info)
            {
                var vehicleType = info.GetVehicleType() & VehicleCategory.Passenger;
                if (vehicleType != VehicleCategory.None)
                    yield return new BuildingSpawnPoint(data, tourBuilding.m_vehicleSpawnPosition, 0f, vehicleType, PointType.Both);
            }
        }
        private static IEnumerable<BuildingSpawnPoint> GetCopterPoints(BuildingData data)
        {
            var info = data.Id.GetBuilding().Info;
            var vehicleType = info.GetCopterType();
            if (vehicleType == VehicleCategory.None)
                yield break;

            foreach (var prop in info.m_props)
            {
                if (prop.m_prop == null)
                    continue;

                foreach (var parking in prop.m_prop.m_parkingSpaces)
                {
                    if (parking.m_type.IsSet(VehicleInfo.VehicleType.Helicopter))
                    {
                        yield return new BuildingSpawnPoint(data, (prop.m_position + parking.m_position).FixZ(), prop.m_radAngle + parking.m_direction.AbsoluteAngle(), vehicleType, PointType.Both);
                    }
                }

            }
        }

        public static VehicleCategory GetVehicleType(this TransportInfo info) => info.m_class switch
        {
            { m_subService: ItemClass.SubService.PublicTransportBus } => VehicleCategory.Bus,
            { m_subService: ItemClass.SubService.PublicTransportTrolleybus } => VehicleCategory.Trolleybus,
            { m_subService: ItemClass.SubService.PublicTransportTram } => VehicleCategory.Tram,

            { m_subService: ItemClass.SubService.PublicTransportTours, m_level: ItemClass.Level.Level3 } => VehicleCategory.Bus,
            { m_subService: ItemClass.SubService.PublicTransportTours, m_level: ItemClass.Level.Level4 } => VehicleCategory.PassengerBalloon,

            { m_subService: ItemClass.SubService.PublicTransportTrain } => VehicleCategory.PassengerTrain | VehicleCategory.CargoTrain,
            { m_subService: ItemClass.SubService.PublicTransportMetro } => VehicleCategory.MetroTrain,
            { m_subService: ItemClass.SubService.PublicTransportMonorail } => VehicleCategory.Monorail,

            { m_subService: ItemClass.SubService.PublicTransportShip, m_level: ItemClass.Level.Level1 } => /*VehicleType.PassengerShip |*/ VehicleCategory.CargoShip,
            { m_subService: ItemClass.SubService.PublicTransportShip, m_level: ItemClass.Level.Level2 } => VehicleCategory.PassengerFerry,

            { m_subService: ItemClass.SubService.PublicTransportPlane, m_level: ItemClass.Level.Level1 } => VehicleCategory.CargoPlane,
            { m_subService: ItemClass.SubService.PublicTransportPlane, m_level: ItemClass.Level.Level2 } => VehicleCategory.PassengerBlimp,
            { m_subService: ItemClass.SubService.PublicTransportPlane, m_level: ItemClass.Level.Level3 } => VehicleCategory.PassengerCopter,

            { m_subService: ItemClass.SubService.PublicTransportCableCar } => VehicleCategory.CableCar,
            { m_service: ItemClass.Service.Disaster, m_level: ItemClass.Level.Level4 } => VehicleCategory.Bus,

            _ => VehicleCategory.None,
        };
        public static VehicleCategory GetCopterType(this BuildingInfo info) => info.m_class.m_service switch
        {
            ItemClass.Service.PoliceDepartment => VehicleCategory.PoliceCopter,
            ItemClass.Service.FireDepartment => VehicleCategory.FireCopter,
            ItemClass.Service.Disaster => VehicleCategory.DisasterCopter,
            ItemClass.Service.HealthCare => VehicleCategory.AmbulanceCopter,
            _ => VehicleCategory.None,
        };

        public static ref Building GetBuilding(this ushort id) => ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[id];
        public static ref Vehicle GetVehicle(this ushort id) => ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[id];

        public static VehicleCategory GetDefaultVehicles(this ref Building building) => building.Info.GetDefaultVehicleTypes();
        public static VehicleCategory GetNeededVehicles(this ref Building building) => building.Info.GetNeededVehicleTypes();
        public static VehicleCategory GetPossibleVehicles(this ref Building building) => building.Info.GetPossibleVehicleTypes();

        public static Vector3 FixZ(this Vector3 vector)
        {
            vector.z = -vector.z;
            return vector;
        }

        public static VehicleService GetGroup(this VehicleCategory type) => EnumExtension.GetEnumValues<VehicleService>().FirstOrDefault(v => ((ulong)v & (ulong)type) != 0);
    }
}
