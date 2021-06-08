﻿using ColossalFramework;
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
        private static Dictionary<Type, VehicleType> VehicleAllow { get; } = new Dictionary<Type, VehicleType>
        {
            //Service
            { typeof(AmbulanceAI),VehicleType.Ambulance },
            { typeof(PoliceCarAI),VehicleType.Police },
            { typeof(FireTruckAI),VehicleType.FireTruck },
            { typeof(HearseAI),VehicleType.Hearse },
            { typeof(CargoTruckAI),VehicleType.CargoTruck },

            //Transport
            { typeof(BicycleAI),VehicleType.Bicycle },
            { typeof(TrolleybusAI),VehicleType.Trolleybus },
            { typeof(BusAI),VehicleType.Bus },
            { typeof(TaxiAI),VehicleType.Taxi },
            { typeof(TramAI),VehicleType.Tram },
            { typeof(PostVanAI),VehicleType.PostTruck },

            //Maintenance
            { typeof(GarbageTruckAI),VehicleType.GarbageTruck },
            { typeof(MaintenanceTruckAI),VehicleType.RoadTruck },
            { typeof(ParkMaintenanceVehicleAI),VehicleType.ParkTruck },
            { typeof(SnowTruckAI),VehicleType.SnowTruck },
            { typeof(WaterTruckAI),VehicleType.VacuumTruck },
            { typeof(DisasterResponseVehicleAI),VehicleType.Disaster },

            //Trains
            { typeof(PassengerTrainAI),VehicleType.PassengerTrain },
            { typeof(CargoTrainAI),VehicleType.CargoTrain },
            { typeof(MetroTrainAI),VehicleType.MetroTrain },

            //Planes
            { typeof(CargoPlaneAI),VehicleType.CargoPlane },
            { typeof(PassengerPlaneAI),VehicleType.PassengerPlane },
            { typeof(PrivatePlaneAI),VehicleType.PrivatePlane },

            //Copters
            { typeof(AmbulanceCopterAI),VehicleType.AmbulanceCopter },
            { typeof(DisasterResponseCopterAI),VehicleType.DisasterCopter },
            { typeof(FireCopterAI),VehicleType.FireCopter },
            { typeof(PoliceCopterAI),VehicleType.PoliceCopter },
            { typeof(PassengerHelicopterAI),VehicleType.PassengerCopter },

            //Air
            { typeof(BalloonAI),VehicleType.Balloon },
            { typeof(PassengerBlimpAI),VehicleType.PassengerBlimp },

            //Ships
            { typeof(PassengerShipAI),VehicleType.PassengerShip },
            { typeof(CargoShipAI),VehicleType.CargoShip },
            { typeof(PassengerFerryAI),VehicleType.PassengerFerry },
            { typeof(FishingBoatAI),VehicleType.FishingBoat },
        };
        private static HashSet<Type> VehicleForbidden { get; } = new HashSet<Type>();

        private static Dictionary<Type, VehicleType> BuildingAllow { get; } = new Dictionary<Type, VehicleType>
        {
            //Common
            { typeof(CommonBuildingAI), VehicleType.Default },

            //PrivateBuilding
            { typeof(CommercialBuildingAI), VehicleType.Default | VehicleType.Ambulance | VehicleType.Hearse |VehicleType.CargoTruck },
            { typeof(IndustrialBuildingAI), VehicleType.Default | VehicleType.Ambulance | VehicleType.Hearse | VehicleType.CargoTruck },
            { typeof(IndustrialExtractorAI), VehicleType.Default | VehicleType.Ambulance | VehicleType.Hearse |VehicleType.CargoTruck },
            { typeof(OfficeBuildingAI), VehicleType.Default | VehicleType.Ambulance |VehicleType.Hearse | VehicleType.PostTruck },
            { typeof(ResidentialBuildingAI), VehicleType.Default | VehicleType.Ambulance | VehicleType.Hearse | VehicleType.PostTruck },

            //Service
            { typeof(CemeteryAI), VehicleType.Default | VehicleType.Ambulance | VehicleType.Hearse },
            { typeof(ChildcareAI), VehicleType.Default },
            { typeof(EldercareAI), VehicleType.Default },
            { typeof(FireStationAI), VehicleType.Default },
            { typeof(HospitalAI), VehicleType.Default | VehicleType.Ambulance },
            { typeof(MedicalCenterAI), VehicleType.Default | VehicleType.Ambulance },
            { typeof(LibraryAI), VehicleType.Default },
            { typeof(PoliceStationAI), VehicleType.Default },
            { typeof(SaunaAI), VehicleType.Default },
            { typeof(SchoolAI), VehicleType.Default },

            //Park
            { typeof(ParkAI), VehicleType.Default },
            { typeof(ParkBuildingAI), VehicleType.Default | VehicleType.ParkTruck },
            { typeof(ParkGateAI), VehicleType.Default | VehicleType.ParkTruck },

            //Industrial
            { typeof(FishFarmAI), VehicleType.Default },
            { typeof(FishingHarborAI), VehicleType.Default | VehicleType.CargoTruck | VehicleType.FishingBoat },
            { typeof(WarehouseAI), VehicleType.Default | VehicleType.CargoTruck },

            { typeof(MainIndustryBuildingAI), VehicleType.Default },
            { typeof(AuxiliaryBuildingAI), VehicleType.Default },
            { typeof(ExtractingFacilityAI), VehicleType.Default | VehicleType.CargoTruck },
            { typeof(ProcessingFacilityAI), VehicleType.Default | VehicleType.CargoTruck },
            { typeof(UniqueFactoryAI), VehicleType.Default | VehicleType.CargoTruck },

            //Campus
            { typeof(MainCampusBuildingAI), VehicleType.Default },

            //Maintenance
            { typeof(MaintenanceDepotAI), VehicleType.Default | VehicleType.RoadTruck },
            { typeof(LandfillSiteAI), VehicleType.Default },
            { typeof(SnowDumpAI), VehicleType.Default | VehicleType.SnowTruck },
            { typeof(WaterFacilityAI), VehicleType.Default | VehicleType.VacuumTruck },

            //Transport
            { typeof(CargoStationAI), VehicleType.Default },
            { typeof(CargoHarborAI), VehicleType.Default },
            { typeof(PrivateAirportAI), VehicleType.Default },
            { typeof(TaxiStandAI), VehicleType.Default & ~VehicleType.Taxi },
            { typeof(PostOfficeAI), VehicleType.Default },

            //Depot
            { typeof(DepotAI), VehicleType.Default },
            { typeof(CableCarStationAI), VehicleType.Default },
            { typeof(TransportStationAI), VehicleType.Default },
            { typeof(HarborAI), VehicleType.Default },
          
            //Disaster
            { typeof(DisasterResponseBuildingAI), VehicleType.Default | VehicleType.Disaster },
            { typeof(DoomsdayVaultAI), VehicleType.Default | VehicleType.Disaster | VehicleType.CargoTruck },
            { typeof(ShelterAI), VehicleType.Default | VehicleType.CargoTruck },
            
            //Other
            { typeof(PowerPlantAI), VehicleType.Default | VehicleType.CargoTruck },
            { typeof(HeatingPlantAI), VehicleType.Default | VehicleType.CargoTruck },
            { typeof(MonumentAI), VehicleType.Default | VehicleType.PostTruck},


            //{ typeof(), VehicleType.Default },
        };
        private static Dictionary<Type, VehicleType> BuildingAdditional { get; } = new Dictionary<Type, VehicleType>
        {
            { typeof(TaxiStandAI), VehicleType.Taxi },
            { typeof(PostOfficeAI), VehicleType.PostTruck },
        };
        private static HashSet<Type> BuildingForbidden { get; } = new HashSet<Type>();


        public static VehicleType GetVehicleType(this VehicleInfo info) => GetVehicleType(info.m_vehicleAI.GetType(), VehicleAllow, VehicleForbidden);

        public static VehicleType GetDefaultVehicleTypes(this BuildingInfo info) => GetVehicleType(info.m_buildingAI.GetType(), BuildingAllow, BuildingForbidden);
        public static VehicleType GetAllVehicleTypes(this BuildingInfo info)
        {
            var type = info.m_buildingAI.GetType();
            var vehicleTypes = GetVehicleType(type, BuildingAllow, BuildingForbidden);

            if (BuildingAdditional.TryGetValue(type, out var additional))
                vehicleTypes |= additional;

            //switch (info.m_buildingAI)
            //{
            //    //case CargoStationAI cargoStation:
            //    //    {
            //    //        type |= VehicleType.CargoTruck | VehicleType.PostTruck;
            //    //        if (cargoStation.m_transportInfo is TransportInfo info1)
            //    //            type |= info1.m_vehicleType.GetVehicleType() & VehicleType.Cargo;
            //    //        if (cargoStation.m_transportInfo2 is TransportInfo info2)
            //    //            type |= info2.m_vehicleType.GetVehicleType() & VehicleType.Cargo;
            //    //        break;
            //    //    }
            //    //case DepotAI depot:
            //    //    {
            //    //        if (depot.m_transportInfo is TransportInfo info1)
            //    //            type |= info1.m_vehicleType.GetVehicleType() & VehicleType.Passenger;
            //    //        if (depot.m_secondaryTransportInfo is TransportInfo info2)
            //    //            type |= info2.m_vehicleType.GetVehicleType() & VehicleType.Passenger;
            //    //        break;
            //    //    }
            //    //case ShelterAI shelter:
            //    //    type |= shelter.m_transportInfo.m_vehicleType.GetVehicleType();
            //    //    break;
            //    //case TaxiStandAI:
            //    //    type |= VehicleType.Taxi;
            //    //    break;
            //}

            return vehicleTypes;
        }

        private static VehicleType GetVehicleType(Type type, Dictionary<Type, VehicleType> allow, HashSet<Type> forbidden)
        {
            if (allow.TryGetValue(type, out var vehicleType))
                return vehicleType;
            else if (forbidden.Contains(type))
                return VehicleType.None;
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
                return VehicleType.None;
            }
        }

        public static IEnumerable<BuildingSpawnPoint> GetDefaultPoints(this BuildingData data) => data.Id.GetBuilding().Info.m_buildingAI switch
        {
            //CargoStationAI cargoStation => cargoStation.GetPoints(data),
            //DepotAI depot => depot.GetPoints(data),
            //FishingHarborAI fishingHarbor => fishingHarbor.GetPoints(data),
            //ShelterAI shelter => shelter.GetPoints(data),
            TaxiStandAI taxiStand => taxiStand.GetPoints(data),
            PostOfficeAI postOffice => postOffice.GetPoints(data),
            MaintenanceDepotAI maintenanceDepot => maintenanceDepot.GetPoints(data),
            _ => new BuildingSpawnPoint[0],
        };


        public static IEnumerable<BuildingSpawnPoint> GetPoints(this CargoStationAI cargoStation, BuildingData data)
        {
            yield return new BuildingSpawnPoint(data, cargoStation.m_truckSpawnPosition, 0f, VehicleType.CargoTruck | VehicleType.PostTruck, InvertTraffic ? PointType.Unspawn : PointType.Spawn);
            yield return new BuildingSpawnPoint(data, cargoStation.m_truckUnspawnPosition, 0f, VehicleType.CargoTruck | VehicleType.PostTruck, InvertTraffic ? PointType.Spawn : PointType.Unspawn);

            if (cargoStation.m_transportInfo is TransportInfo info1)
            {
                var vehicleType = info1.m_vehicleType.GetVehicleType() & VehicleType.Cargo;
                yield return new BuildingSpawnPoint(data, cargoStation.m_spawnPosition, cargoStation.m_spawnTarget, vehicleType, invert: cargoStation.m_canInvertTarget && InvertTraffic);
            }

            if (cargoStation.m_transportInfo2 is TransportInfo info2)
            {
                var vehicleType = info2.m_vehicleType.GetVehicleType() & VehicleType.Cargo;
                yield return new BuildingSpawnPoint(data, cargoStation.m_spawnPosition2, cargoStation.m_spawnTarget2, vehicleType, invert: cargoStation.m_canInvertTarget2 && InvertTraffic);
            }
        }
        public static IEnumerable<BuildingSpawnPoint> GetPoints(this DepotAI depot, BuildingData data)
        {
            if (depot.m_transportInfo is TransportInfo info1)
            {
                var invert = depot.m_canInvertTarget && InvertTraffic;
                var vehicleType = info1.m_vehicleType.GetVehicleType() & VehicleType.Passenger;

                if (depot.m_spawnPoints != null && depot.m_spawnPoints.Length != 0)
                {
                    foreach (var point in depot.m_spawnPoints)
                        yield return new BuildingSpawnPoint(data, point.m_position, point.m_target, vehicleType, invert: invert);
                }
                else
                    yield return new BuildingSpawnPoint(data, depot.m_spawnPosition, depot.m_spawnTarget, vehicleType, invert: invert);
            }
            if (depot.m_secondaryTransportInfo is TransportInfo info2)
            {
                var invert = depot.m_canInvertTarget2 && InvertTraffic;
                var vehicleType = info2.m_vehicleType.GetVehicleType() & VehicleType.Passenger;

                if (depot.m_spawnPoints2 != null && depot.m_spawnPoints2.Length != 0)
                {
                    foreach (var point in depot.m_spawnPoints2)
                        yield return new BuildingSpawnPoint(data, point.m_position, point.m_target, vehicleType, invert: invert);
                }
                else
                    yield return new BuildingSpawnPoint(data, depot.m_spawnPosition2, depot.m_spawnTarget2, vehicleType, invert: invert);
            }
        }
        public static IEnumerable<BuildingSpawnPoint> GetPoints(this FishingHarborAI fishingHarbor, BuildingData data)
        {
            yield return new BuildingSpawnPoint(data, fishingHarbor.m_boatSpawnPosition, fishingHarbor.m_boatSpawnTarget, VehicleType.FishingBoat);
        }
        public static IEnumerable<BuildingSpawnPoint> GetPoints(this ShelterAI shelter, BuildingData data)
        {
            var invert = shelter.m_canInvertTarget && InvertTraffic;
            yield return new BuildingSpawnPoint(data, shelter.m_spawnPosition, shelter.m_spawnTarget, shelter.m_transportInfo.m_vehicleType, invert: invert);
        }
        public static IEnumerable<BuildingSpawnPoint> GetPoints(this TaxiStandAI taxiStand, BuildingData data)
        {
            yield return new BuildingSpawnPoint(data, taxiStand.m_queueStartPos, taxiStand.m_queueEndPos, VehicleType.Taxi, invert: InvertTraffic);
        }
        public static IEnumerable<BuildingSpawnPoint> GetPoints(this PostOfficeAI postOffice, BuildingData data)
        {
            yield return new BuildingSpawnPoint(data, postOffice.m_truckSpawnPosition, 0f, VehicleType.PostTruck, InvertTraffic ? PointType.Unspawn : PointType.Spawn);
            yield return new BuildingSpawnPoint(data, postOffice.m_truckUnspawnPosition, 0f, VehicleType.PostTruck, InvertTraffic ? PointType.Spawn : PointType.Unspawn);
        }
        public static IEnumerable<BuildingSpawnPoint> GetPoints(this MaintenanceDepotAI maintenanceDepot, BuildingData data)
        {
            yield return new BuildingSpawnPoint(data, maintenanceDepot.m_spawnPosition, maintenanceDepot.m_spawnTarget, VehicleType.PostTruck, PointType.Both);
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

        public static ref Building GetBuilding(this ushort id) => ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[id];

        public static VehicleType GetDefaultVehicles(this ref Building building) => building.Info.GetDefaultVehicleTypes();
        public static VehicleType GetPossibleVehicles(this ref Building building) => building.Info.GetAllVehicleTypes();
    }
}