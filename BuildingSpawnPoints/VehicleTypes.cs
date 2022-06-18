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
    public enum VehicleType : ulong
    {
        [NotItem]
        None = 0ul,


        [Description(nameof(Localize.VehicleType_Ambulance))]
        Ambulance = 1ul,

        [Description(nameof(Localize.VehicleType_Hearse))]
        Hearse = 1ul << 1,

        [Description(nameof(Localize.VehicleType_Police))]
        Police = 1ul << 3,

        [Description(nameof(Localize.VehicleType_DisasterResponse))]
        Disaster = 1ul << 4,


        [Description(nameof(Localize.VehicleType_Bus))]
        Bus = 1ul << 2,

        [Description(nameof(Localize.VehicleType_Trolleybus))]
        Trolleybus = 1ul << 5,

        [Description(nameof(Localize.VehicleType_Taxi))]
        Taxi = 1ul << 6,

        [Description(nameof(Localize.VehicleType_Bicycle))]
        Bicycle = 1ul << 7,



        [Description(nameof(Localize.VehicleType_CargoTruck))]
        CargoTruck = 1ul << 8,

        [Description(nameof(Localize.VehicleType_FireTruck))]
        FireTruck = 1ul << 9,

        [Description(nameof(Localize.VehicleType_PostTruck))]
        PostTruck = 1ul << 10,

        [Description(nameof(Localize.VehicleType_GarbageTruck))]
        GarbageTruck = 1ul << 11,

        [Description(nameof(Localize.VehicleType_RoadMaintenanceTruck))]
        RoadTruck = 1ul << 12,

        [Description(nameof(Localize.VehicleType_ParkMaintenanceTruck))]
        ParkTruck = 1ul << 13,

        [Description(nameof(Localize.VehicleType_SnowTruck))]
        SnowTruck = 1ul << 14,

        [Description(nameof(Localize.VehicleType_VacuumTruck))]
        VacuumTruck = 1ul << 15,



        [Description(nameof(Localize.VehicleType_PassengerPlane))]
        PassengerPlane = 1ul << 16,

        [Description(nameof(Localize.VehicleType_CargoPlane))]
        CargoPlane = 1ul << 17,

        [Description(nameof(Localize.VehicleType_PrivatePlane))]
        PrivatePlane = 1ul << 18,

        [Description(nameof(Localize.VehicleType_PassengerCopter))]
        PassengerCopter = 1ul << 19,

        [Description(nameof(Localize.VehicleType_AmbulanceCopter))]
        AmbulanceCopter = 1ul << 20,

        [Description(nameof(Localize.VehicleType_FireCopter))]
        FireCopter = 1ul << 21,

        [Description(nameof(Localize.VehicleType_PoliceCopter))]
        PoliceCopter = 1ul << 22,

        [Description(nameof(Localize.VehicleType_DisasterCopter))]
        DisasterCopter = 1ul << 23,

        [Description(nameof(Localize.VehicleType_Balloon))]
        PassengerBalloon = 1ul << 24,

        [Description(nameof(Localize.VehicleType_PassengerBlimp))]
        PassengerBlimp = 1ul << 25,



        [Description(nameof(Localize.VehicleType_PassengerShip))]
        PassengerShip = 1ul << 26,

        [Description(nameof(Localize.VehicleType_CargoShip))]
        CargoShip = 1ul << 27,

        [Description(nameof(Localize.VehicleType_PassengerFerry))]
        PassengerFerry = 1ul << 28,

        [Description(nameof(Localize.VehicleType_FishingBoat))]
        FishingBoat = 1ul << 29,



        [Description(nameof(Localize.VehicleType_PassengerTrain))]
        PassengerTrain = 1ul << 30,

        [Description(nameof(Localize.VehicleType_CargoTrain))]
        CargoTrain = 1ul << 31,

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
        None = VehicleType.None,
        Car = VehicleType.Cars | VehicleType.Trucks,
        Trolleybus = VehicleType.Trolleybus,
        Tram = VehicleType.Tram,
        Plane = VehicleType.Planes & ~VehicleType.PrivatePlane,
        Balloon = VehicleType.PassengerBalloon,
        Blimp = VehicleType.PassengerBlimp,
        Ship = VehicleType.Ships & ~VehicleType.PassengerFerry & ~VehicleType.FishingBoat,
        Ferry = VehicleType.PassengerFerry,
        Fishing = VehicleType.FishingBoat,
        Train = VehicleType.Trains & ~VehicleType.MetroTrain & ~VehicleType.Monorail,
        Metro = VehicleType.MetroTrain,
        Monorail = VehicleType.Monorail,
        CableCar = VehicleType.CableCar,
    }
    public enum VehicleGroupType : ulong
    {
        Planes = VehicleType.Planes,
        Copters = VehicleType.Copters,
        Trains = VehicleType.Trains,
        Ships = VehicleType.Ships,
        Trucks = VehicleType.Trucks,
        Passenger = VehicleType.Passenger,
        Service = VehicleType.Service,
    }
}
