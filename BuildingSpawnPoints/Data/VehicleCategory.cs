using ModsCommon.Utilities;
using System;
using System.ComponentModel;
using static BuildingSpawnPoints.VehicleCategory;


namespace BuildingSpawnPoints
{
    [Flags]
    public enum VehicleCategory : ulong
    {
        [NotItem]
        None = 0ul,


        [Description(nameof(Localize.VehicleType_Ambulance))]
        [Function(VehicleFunction.Emergency)]
        [Service(VehicleService.Car)]
        Ambulance = VehicleInfo.VehicleCategory.Ambulance,

        [Description(nameof(Localize.VehicleType_Hearse))]
        [Function(VehicleFunction.Emergency)]
        [Service(VehicleService.Car)]
        Hearse = VehicleInfo.VehicleCategory.Hearse,

        [Description(nameof(Localize.VehicleType_Police))]
        [Function(VehicleFunction.Emergency)]
        [Service(VehicleService.Car)]
        Police = VehicleInfo.VehicleCategory.Police,

        [Description(nameof(Localize.VehicleType_DisasterResponse))]
        [Function(VehicleFunction.Emergency)]
        [Service(VehicleService.Car)]
        Disaster = VehicleInfo.VehicleCategory.Disaster,


        [Description(nameof(Localize.VehicleType_Bus))]
        [Function(VehicleFunction.Public)]
        [Service(VehicleService.Car)]
        Bus = VehicleInfo.VehicleCategory.Bus,

        [Description(nameof(Localize.VehicleType_Trolleybus))]
        [Function(VehicleFunction.Public)]
        [Service(VehicleService.Trolleybus)]
        Trolleybus = VehicleInfo.VehicleCategory.Trolleybus,

        [Description(nameof(Localize.VehicleType_Taxi))]
        [Function(VehicleFunction.Public)]
        [Service(VehicleService.Car)]
        Taxi = VehicleInfo.VehicleCategory.Taxi,

        [Description(nameof(Localize.VehicleType_Bicycle))]
        [Function(VehicleFunction.Public)]
        [Service(VehicleService.Bicycle)]
        Bicycle = VehicleInfo.VehicleCategory.Bicycle,



        [Description(nameof(Localize.VehicleType_CargoTruck))]
        [Function(VehicleFunction.Trucks)]
        [Service(VehicleService.Car)]
        CargoTruck = VehicleInfo.VehicleCategory.CargoTruck,

        [Description(nameof(Localize.VehicleType_FireTruck))]
        [Function(VehicleFunction.Emergency)]
        [Service(VehicleService.Car)]
        FireTruck = VehicleInfo.VehicleCategory.FireTruck,

        [Description(nameof(Localize.VehicleType_PostTruck))]
        [Function(VehicleFunction.Trucks)]
        [Service(VehicleService.Car)]
        PostTruck = VehicleInfo.VehicleCategory.PostTruck,

        [Description(nameof(Localize.VehicleType_GarbageTruck))]
        [Function(VehicleFunction.Trucks)]
        [Service(VehicleService.Car)]
        GarbageTruck = VehicleInfo.VehicleCategory.GarbageTruck,

        [Description(nameof(Localize.VehicleType_RoadMaintenanceTruck))]
        [Function(VehicleFunction.Trucks)]
        [Service(VehicleService.Car)]
        RoadTruck = VehicleInfo.VehicleCategory.MaintenanceTruck,

        [Description(nameof(Localize.VehicleType_ParkMaintenanceTruck))]
        [Function(VehicleFunction.Trucks)]
        [Service(VehicleService.Car)]
        ParkTruck = VehicleInfo.VehicleCategory.ParkTruck,

        [Description(nameof(Localize.VehicleType_SnowTruck))]
        [Function(VehicleFunction.Trucks)]
        [Service(VehicleService.Car)]
        SnowTruck = VehicleInfo.VehicleCategory.SnowTruck,

        [Description(nameof(Localize.VehicleType_VacuumTruck))]
        [Function(VehicleFunction.Trucks)]
        [Service(VehicleService.Car)]
        VacuumTruck = VehicleInfo.VehicleCategory.VacuumTruck,

        [Description(nameof(Localize.VehicleType_BankTruck))]
        [Function(VehicleFunction.Trucks)]
        [Service(VehicleService.Car)]
        BankTruck = VehicleInfo.VehicleCategory.BankTruck,



        [Description(nameof(Localize.VehicleType_PassengerPlane))]
        [Function(VehicleFunction.Public)]
        [Service(VehicleService.Plane)]
        PassengerPlane = VehicleInfo.VehicleCategory.PassengerPlane,

        [Description(nameof(Localize.VehicleType_CargoPlane))]
        [Function(VehicleFunction.Planes)]
        [Service(VehicleService.Plane)]
        CargoPlane = VehicleInfo.VehicleCategory.CargoPlane,

        [Description(nameof(Localize.VehicleType_PrivatePlane))]
        [Function(VehicleFunction.Planes)]
        [Service(VehicleService.Plane)]
        PrivatePlane = VehicleInfo.VehicleCategory.PrivatePlane,

        [Description(nameof(Localize.VehicleType_PassengerCopter))]
        [Function(VehicleFunction.Public)]
        [Service(VehicleService.Copters)]
        PassengerCopter = VehicleInfo.VehicleCategory.PassengerCopter,

        [Description(nameof(Localize.VehicleType_AmbulanceCopter))]
        [Function(VehicleFunction.Copters)]
        [Service(VehicleService.Copters)]
        AmbulanceCopter = VehicleInfo.VehicleCategory.AmbulanceCopter,

        [Description(nameof(Localize.VehicleType_FireCopter))]
        [Function(VehicleFunction.Copters)]
        [Service(VehicleService.Copters)]
        FireCopter = VehicleInfo.VehicleCategory.FireCopter,

        [Description(nameof(Localize.VehicleType_PoliceCopter))]
        [Function(VehicleFunction.Copters)]
        [Service(VehicleService.Copters)]
        PoliceCopter = VehicleInfo.VehicleCategory.PoliceCopter,

        [Description(nameof(Localize.VehicleType_DisasterCopter))]
        [Function(VehicleFunction.Copters)]
        [Service(VehicleService.Copters)]
        DisasterCopter = VehicleInfo.VehicleCategory.DisasterCopter,

        [Description(nameof(Localize.VehicleType_Balloon))]
        [Function(VehicleFunction.Public)]
        [Service(VehicleService.Balloon)]
        PassengerBalloon = VehicleInfo.VehicleCategory.PassengerBalloon,

        [Description(nameof(Localize.VehicleType_PassengerBlimp))]
        [Function(VehicleFunction.Public)]
        [Service(VehicleService.Blimp)]
        PassengerBlimp = VehicleInfo.VehicleCategory.PassengerBlimp,



        [Description(nameof(Localize.VehicleType_PassengerShip))]
        [Function(VehicleFunction.Public)]
        [Service(VehicleService.Ship)]
        PassengerShip = VehicleInfo.VehicleCategory.PassengerShip,

        [Description(nameof(Localize.VehicleType_CargoShip))]
        [Function(VehicleFunction.Ships)]
        [Service(VehicleService.Ship)]
        CargoShip = VehicleInfo.VehicleCategory.CargoShip,

        [Description(nameof(Localize.VehicleType_PassengerFerry))]
        [Function(VehicleFunction.Public)]
        [Service(VehicleService.Ferry)]
        PassengerFerry = VehicleInfo.VehicleCategory.PassengerFerry,

        [Description(nameof(Localize.VehicleType_FishingBoat))]
        [Function(VehicleFunction.Ships)]
        [Service(VehicleService.Fishing)]
        FishingBoat = VehicleInfo.VehicleCategory.FishingBoat,



        [Description(nameof(Localize.VehicleType_PassengerTrain))]
        [Function(VehicleFunction.Public)]
        [Service(VehicleService.Train)]
        PassengerTrain = VehicleInfo.VehicleCategory.PassengerTrain,

        [Description(nameof(Localize.VehicleType_CargoTrain))]
        [Function(VehicleFunction.Trains)]
        [Service(VehicleService.Train)]
        CargoTrain = VehicleInfo.VehicleCategory.CargoTrain,

        [Description(nameof(Localize.VehicleType_MetroTrain))]
        [Function(VehicleFunction.Public)]
        [Service(VehicleService.Metro)]
        MetroTrain = VehicleInfo.VehicleCategory.MetroTrain,

        [Description(nameof(Localize.VehicleType_Monorail))]
        [Function(VehicleFunction.Public)]
        [Service(VehicleService.Monorail)]
        Monorail = VehicleInfo.VehicleCategory.Monorail,

        [Description(nameof(Localize.VehicleType_Tram))]
        [Function(VehicleFunction.Public)]
        [Service(VehicleService.Train)]
        Tram = VehicleInfo.VehicleCategory.Tram,



        [Description(nameof(Localize.VehicleType_Rocket))]
        [Function(VehicleFunction.Planes)]
        [Service(VehicleService.Plane)]
        Rocket = VehicleInfo.VehicleCategory.Rocket,

        [Description(nameof(Localize.VehicleType_CableCar))]
        [Function(VehicleFunction.Public)]
        [Service(VehicleService.CableCar)]
        CableCar = VehicleInfo.VehicleCategory.CableCar,

        [NotItem]
        [Description(nameof(Localize.VehicleTypeGroup_Default))]
        Default = Police | FireTruck | GarbageTruck | Ambulance | Hearse | Taxi | Disaster,

        [NotItem]
        [Description(nameof(Localize.VehicleType_All))]
        All = ulong.MaxValue,
    }

    [Flags]
    public enum VehicleService : ulong
    {
        [NotItem]
        None = 0,

        [Category(VehicleCategory.Bicycle)]
        Bicycle = 1 << 0,

        [Category(CargoTruck | FireTruck | SnowTruck | VacuumTruck | GarbageTruck | RoadTruck | Ambulance | Bus | Disaster | Hearse | ParkTruck | Police | PostTruck | BankTruck | Taxi)]
        Car = 1 << 1,

        [Category(VehicleCategory.Trolleybus)]
        Trolleybus = 1 << 2,

        [Category(VehicleCategory.Tram)]
        Tram = 1 << 3,

        [Category(CargoPlane | PassengerPlane)]
        Plane = 1 << 4,

        [Category(PassengerCopter | AmbulanceCopter | DisasterCopter | FireCopter | PoliceCopter)]
        Copters = 1 << 5,

        [Category(PassengerBalloon)]
        Balloon = 1 << 6,

        [Category(PassengerBlimp)]
        Blimp = 1 << 7,

        [Category(CargoShip | PassengerShip)]
        Ship = 1 << 8,

        [Category(PassengerFerry)]
        Ferry = 1 << 9,

        [Category(FishingBoat)]
        Fishing = 1 << 10,

        [Category(CargoTrain | PassengerTrain)]
        Train = 1 << 11,

        [Category(MetroTrain)]
        Metro = 1 << 12,

        [Category(VehicleCategory.Monorail)]
        Monorail = 1 << 13,

        [Category(VehicleCategory.CableCar)]
        CableCar = 1 << 14,
    }

    [Flags]
    public enum VehicleFunction : ulong
    {
        [NotItem]
        None = 0,

        [Description(nameof(Localize.VehicleTypeGroup_Planes))]
        [Category(PassengerPlane | CargoPlane | PrivatePlane)]
        Planes = 1 << 0,

        [Description(nameof(Localize.VehicleTypeGroup_Copters))]
        [Category(PassengerCopter | AmbulanceCopter | DisasterCopter | FireCopter | PoliceCopter)]
        Copters = 1 << 1,

        [Description(nameof(Localize.VehicleTypeGroup_Trains))]
        [Category(CargoTrain | PassengerTrain | MetroTrain | Monorail)]
        Trains = 1 << 2,

        [Description(nameof(Localize.VehicleTypeGroup_Ships))]
        [Category(CargoShip | PassengerShip | PassengerFerry | FishingBoat)]
        Ships = 1 << 3,

        [Description(nameof(Localize.VehicleTypeGroup_Trucks))]
        [Category(CargoTruck | FireTruck | SnowTruck | VacuumTruck | GarbageTruck | RoadTruck | ParkTruck | PostTruck | BankTruck)]
        Trucks = 1 << 4,

        [Description(nameof(Localize.VehicleTypeGroup_Public))]
        [Category(Bus | Taxi | Trolleybus | Bicycle | PassengerPlane | PassengerCopter | PassengerBlimp | PassengerFerry | PassengerShip | PassengerTrain | MetroTrain | Monorail | Tram | PassengerBalloon)]
        Public = 1 << 5,

        [Description(nameof(Localize.VehicleTypeGroup_Emergency))]
        [Category(Ambulance | Disaster | Hearse | Police | FireTruck)]
        Emergency = 1 << 6,

        [Description(nameof(Localize.VehicleTypeGroup_Cargo))]
        [Category(CargoTruck | CargoPlane | CargoShip | CargoTrain)]
        Cargo = 1 << 7,

        [Description(nameof(Localize.VehicleTypeGroup_Service))]
        [Category(PostTruck | GarbageTruck | RoadTruck | ParkTruck | SnowTruck | VacuumTruck | BankTruck)]
        Service = 1 << 8,
    }

    public class CategoryAttribute : Attribute
    {
        public VehicleCategory Category { get; set; }
        public CategoryAttribute(VehicleCategory category)
        {
            Category = category;
        }
    }
    public class FunctionAttribute : Attribute
    {
        public VehicleFunction Function { get; set; }
        public FunctionAttribute(VehicleFunction function)
        {
            Function = function;
        }
    }
    public class ServiceAttribute : Attribute
    {
        public VehicleService Service { get; set; }
        public ServiceAttribute(VehicleService service)
        {
            Service = service;
        }
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
