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
    [Flags]
    public enum VehicleCategory : ulong
    {
        [NotItem]
        None = 0ul,


        [Description(nameof(Localize.VehicleType_Ambulance))]
        Ambulance = VehicleInfo.VehicleCategory.Ambulance,

        [Description(nameof(Localize.VehicleType_Hearse))]
        Hearse = VehicleInfo.VehicleCategory.Hearse,

        [Description(nameof(Localize.VehicleType_Police))]
        Police = VehicleInfo.VehicleCategory.Police,

        [Description(nameof(Localize.VehicleType_DisasterResponse))]
        Disaster = VehicleInfo.VehicleCategory.Disaster,


        [Description(nameof(Localize.VehicleType_Bus))]
        Bus = VehicleInfo.VehicleCategory.Bus,

        [Description(nameof(Localize.VehicleType_Trolleybus))]
        Trolleybus = VehicleInfo.VehicleCategory.Trolleybus,

        [Description(nameof(Localize.VehicleType_Taxi))]
        Taxi = VehicleInfo.VehicleCategory.Taxi,

        [Description(nameof(Localize.VehicleType_Bicycle))]
        Bicycle = VehicleInfo.VehicleCategory.Bicycle,



        [Description(nameof(Localize.VehicleType_CargoTruck))]
        CargoTruck = VehicleInfo.VehicleCategory.CargoTruck,

        [Description(nameof(Localize.VehicleType_FireTruck))]
        FireTruck = VehicleInfo.VehicleCategory.FireTruck,

        [Description(nameof(Localize.VehicleType_PostTruck))]
        PostTruck = VehicleInfo.VehicleCategory.PostTruck,

        [Description(nameof(Localize.VehicleType_GarbageTruck))]
        GarbageTruck = VehicleInfo.VehicleCategory.GarbageTruck,

        [Description(nameof(Localize.VehicleType_RoadMaintenanceTruck))]
        RoadTruck = VehicleInfo.VehicleCategory.MaintenanceTruck,

        [Description(nameof(Localize.VehicleType_ParkMaintenanceTruck))]
        ParkTruck = VehicleInfo.VehicleCategory.ParkTruck,

        [Description(nameof(Localize.VehicleType_SnowTruck))]
        SnowTruck = VehicleInfo.VehicleCategory.SnowTruck,

        [Description(nameof(Localize.VehicleType_VacuumTruck))]
        VacuumTruck = VehicleInfo.VehicleCategory.VacuumTruck,



        [Description(nameof(Localize.VehicleType_PassengerPlane))]
        PassengerPlane = VehicleInfo.VehicleCategory.PassengerPlane,

        [Description(nameof(Localize.VehicleType_CargoPlane))]
        CargoPlane = VehicleInfo.VehicleCategory.CargoPlane,

        [Description(nameof(Localize.VehicleType_PrivatePlane))]
        PrivatePlane = VehicleInfo.VehicleCategory.PrivatePlane,

        [Description(nameof(Localize.VehicleType_PassengerCopter))]
        PassengerCopter = VehicleInfo.VehicleCategory.PassengerCopter,

        [Description(nameof(Localize.VehicleType_AmbulanceCopter))]
        AmbulanceCopter = VehicleInfo.VehicleCategory.AmbulanceCopter,

        [Description(nameof(Localize.VehicleType_FireCopter))]
        FireCopter = VehicleInfo.VehicleCategory.FireCopter,

        [Description(nameof(Localize.VehicleType_PoliceCopter))]
        PoliceCopter = VehicleInfo.VehicleCategory.PoliceCopter,

        [Description(nameof(Localize.VehicleType_DisasterCopter))]
        DisasterCopter = VehicleInfo.VehicleCategory.DisasterCopter,

        [Description(nameof(Localize.VehicleType_Balloon))]
        PassengerBalloon = VehicleInfo.VehicleCategory.PassengerBalloon,

        [Description(nameof(Localize.VehicleType_PassengerBlimp))]
        PassengerBlimp = VehicleInfo.VehicleCategory.PassengerBlimp,



        [Description(nameof(Localize.VehicleType_PassengerShip))]
        PassengerShip = VehicleInfo.VehicleCategory.PassengerShip,

        [Description(nameof(Localize.VehicleType_CargoShip))]
        CargoShip = VehicleInfo.VehicleCategory.CargoShip,

        [Description(nameof(Localize.VehicleType_PassengerFerry))]
        PassengerFerry = VehicleInfo.VehicleCategory.PassengerFerry,

        [Description(nameof(Localize.VehicleType_FishingBoat))]
        FishingBoat = VehicleInfo.VehicleCategory.FishingBoat,



        [Description(nameof(Localize.VehicleType_PassengerTrain))]
        PassengerTrain = VehicleInfo.VehicleCategory.PassengerTrain,

        [Description(nameof(Localize.VehicleType_CargoTrain))]
        CargoTrain = VehicleInfo.VehicleCategory.CargoTrain,

        [Description(nameof(Localize.VehicleType_MetroTrain))]
        MetroTrain = VehicleInfo.VehicleCategory.MetroTrain,

        [Description(nameof(Localize.VehicleType_Monorail))]
        Monorail = VehicleInfo.VehicleCategory.Monorail,

        [Description(nameof(Localize.VehicleType_Tram))]
        Tram = VehicleInfo.VehicleCategory.Tram,



        [Description(nameof(Localize.VehicleType_Rocket))]
        Rocket = VehicleInfo.VehicleCategory.Rocket,

        [Description(nameof(Localize.VehicleType_CableCar))]
        CableCar = VehicleInfo.VehicleCategory.CableCar,



        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Passenger))]
        Passenger = Bus | Taxi | Trolleybus | Bicycle | PassengerPlane | PassengerCopter | PassengerBlimp | PassengerFerry | PassengerShip | PassengerTrain | MetroTrain | Monorail | Tram | PassengerBalloon,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Service))]
        Service = FireTruck | SnowTruck | VacuumTruck | GarbageTruck | RoadTruck | Ambulance | Disaster | Hearse | ParkTruck | Police | PostTruck,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Planes))]
        Planes = CargoPlane | PassengerPlane | PrivatePlane,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Copters))]
        Copters = PassengerCopter | AmbulanceCopter | DisasterCopter | FireCopter | PoliceCopter,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Trains))]
        Trains = CargoTrain | PassengerTrain | MetroTrain | Monorail,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Ships))]
        Ships = CargoShip | PassengerShip | PassengerFerry | FishingBoat,

        //[NotItem]
        //[Description(nameof(Localize.VehicleTypeGroup_Air))]
        //Air = Planes | Copters | PassengerBalloon | PassengerBlimp,

        //[NotItem]
        //[Description(nameof(Localize.VehicleTypeGroup_Water))]
        //Water = FishingBoat | PassengerFerry | CargoShip | PassengerShip,

        //[NotItem]
        //[Description(nameof(Localize.VehicleTypeGroup_Rail))]
        //Rail = CargoTrain | PassengerTrain | MetroTrain | Monorail | Tram,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Cargo))]
        Cargo = CargoTruck | CargoPlane | CargoShip | CargoTrain,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Trucks))]
        Trucks = CargoTruck | FireTruck | SnowTruck | VacuumTruck | GarbageTruck | RoadTruck,

        [NotItem]
        Cars = Trucks | Ambulance | Bus | Disaster | Hearse | ParkTruck | Police | PostTruck | Taxi,

        //[NotItem]
        //[Description(nameof(Localize.VehicleTypeGroup_Road))]
        //Road = Trucks | Cars | Trolleybus,

        //[NotItem]
        //[Description(nameof(Localize.VehicleTypeGroup_PassengerRoad))]
        //PassengerRoad = Passenger & Road,

        //[NotItem]
        //[Description(nameof(Localize.VehicleTypeGroup_PassengerAir))]
        //PassengerAir = Passenger & Air,

        //[NotItem]
        //[Description(nameof(Localize.VehicleTypeGroup_PassengerWater))]
        //PassengerWater = Passenger & Water,

        //[NotItem]
        //[Description(nameof(Localize.VehicleTypeGroup_PassengerRail))]
        //PassengerRail = Passenger & Rail,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Default))]
        Default = Police | FireTruck | GarbageTruck | Ambulance | Hearse | Taxi | Disaster,

        [NotItem]
        [Description(nameof(Localize.VehicleType_All))]
        All = ulong.MaxValue,
    }
    public enum VehicleService : ulong
    {
        None = VehicleCategory.None,
        Car = VehicleCategory.Cars | VehicleCategory.Trucks,
        Trolleybus = VehicleCategory.Trolleybus,
        Tram = VehicleCategory.Tram,
        Plane = VehicleCategory.Planes & ~VehicleCategory.PrivatePlane,
        Balloon = VehicleCategory.PassengerBalloon,
        Blimp = VehicleCategory.PassengerBlimp,
        Ship = VehicleCategory.Ships & ~VehicleCategory.PassengerFerry & ~VehicleCategory.FishingBoat,
        Ferry = VehicleCategory.PassengerFerry,
        Fishing = VehicleCategory.FishingBoat,
        Train = VehicleCategory.Trains & ~VehicleCategory.MetroTrain & ~VehicleCategory.Monorail,
        Metro = VehicleCategory.MetroTrain,
        Monorail = VehicleCategory.Monorail,
        CableCar = VehicleCategory.CableCar,
    }
    public enum VehicleGroupType : ulong
    {
        Planes = VehicleCategory.Planes,
        Copters = VehicleCategory.Copters,
        Trains = VehicleCategory.Trains,
        Ships = VehicleCategory.Ships,
        Trucks = VehicleCategory.Trucks,
        Passenger = VehicleCategory.Passenger,
        Service = VehicleCategory.Service,
    }

    public static class VehicleCategoryUtility
    {
        public static VehicleCategory GetNew(this VehicleCategory oldCategory)
        {
            var newCategory = VehicleCategory.None;
            var oldValue = (ulong)oldCategory;

            if ((oldValue & 1ul) != 0) newCategory |= VehicleCategory.Ambulance;
            if ((oldValue & (1ul << 1)) != 0) newCategory |= VehicleCategory.Hearse;
            if ((oldValue & (1ul << 2)) != 0) newCategory |= VehicleCategory.Bus;
            if ((oldValue & (1ul << 3)) != 0) newCategory |= VehicleCategory.Police;
            if ((oldValue & (1ul << 4)) != 0) newCategory |= VehicleCategory.Disaster;
            if ((oldValue & (1ul << 5)) != 0) newCategory |= VehicleCategory.Trolleybus;
            if ((oldValue & (1ul << 6)) != 0) newCategory |= VehicleCategory.Taxi;
            if ((oldValue & (1ul << 7)) != 0) newCategory |= VehicleCategory.Bicycle;
            if ((oldValue & (1ul << 8)) != 0) newCategory |= VehicleCategory.CargoTruck;
            if ((oldValue & (1ul << 9)) != 0) newCategory |= VehicleCategory.FireTruck;
            if ((oldValue & (1ul << 10)) != 0) newCategory |= VehicleCategory.PostTruck;
            if ((oldValue & (1ul << 11)) != 0) newCategory |= VehicleCategory.GarbageTruck;
            if ((oldValue & (1ul << 12)) != 0) newCategory |= VehicleCategory.RoadTruck;
            if ((oldValue & (1ul << 13)) != 0) newCategory |= VehicleCategory.ParkTruck;
            if ((oldValue & (1ul << 14)) != 0) newCategory |= VehicleCategory.SnowTruck;
            if ((oldValue & (1ul << 15)) != 0) newCategory |= VehicleCategory.VacuumTruck;
            if ((oldValue & (1ul << 16)) != 0) newCategory |= VehicleCategory.PassengerPlane;
            if ((oldValue & (1ul << 17)) != 0) newCategory |= VehicleCategory.CargoPlane;
            if ((oldValue & (1ul << 18)) != 0) newCategory |= VehicleCategory.PrivatePlane;
            if ((oldValue & (1ul << 19)) != 0) newCategory |= VehicleCategory.PassengerCopter;
            if ((oldValue & (1ul << 20)) != 0) newCategory |= VehicleCategory.AmbulanceCopter;
            if ((oldValue & (1ul << 21)) != 0) newCategory |= VehicleCategory.FireCopter;
            if ((oldValue & (1ul << 22)) != 0) newCategory |= VehicleCategory.PoliceCopter;
            if ((oldValue & (1ul << 23)) != 0) newCategory |= VehicleCategory.DisasterCopter;
            if ((oldValue & (1ul << 24)) != 0) newCategory |= VehicleCategory.PassengerBalloon;
            if ((oldValue & (1ul << 25)) != 0) newCategory |= VehicleCategory.PassengerBlimp;
            if ((oldValue & (1ul << 26)) != 0) newCategory |= VehicleCategory.PassengerShip;
            if ((oldValue & (1ul << 27)) != 0) newCategory |= VehicleCategory.CargoShip;
            if ((oldValue & (1ul << 28)) != 0) newCategory |= VehicleCategory.PassengerFerry;
            if ((oldValue & (1ul << 29)) != 0) newCategory |= VehicleCategory.FishingBoat;
            if ((oldValue & (1ul << 30)) != 0) newCategory |= VehicleCategory.PassengerTrain;
            if ((oldValue & (1ul << 31)) != 0) newCategory |= VehicleCategory.CargoTrain;
            if ((oldValue & (1ul << 32)) != 0) newCategory |= VehicleCategory.MetroTrain;
            if ((oldValue & (1ul << 33)) != 0) newCategory |= VehicleCategory.Monorail;
            if ((oldValue & (1ul << 34)) != 0) newCategory |= VehicleCategory.Tram;
            if ((oldValue & (1ul << 35)) != 0) newCategory |= VehicleCategory.Rocket;
            if ((oldValue & (1ul << 36)) != 0) newCategory |= VehicleCategory.CableCar;

            return newCategory;
        }
    }
}
