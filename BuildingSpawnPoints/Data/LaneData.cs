using System.Collections.Generic;

namespace BuildingSpawnPoints
{
    public struct VehicleLaneData
    {
        public ItemClass.Service Service;
        public NetInfo.LaneType Lane;
        public VehicleInfo.VehicleType Type;
        public VehicleInfo.VehicleCategory VehicleCategory;
        public float Distance;

        public VehicleLaneData(ItemClass.Service service, NetInfo.LaneType lane, VehicleInfo.VehicleType type, VehicleInfo.VehicleCategory vehicleCategory, float distance)
        {
            Service = service;
            Lane = lane;
            Type = type;
            VehicleCategory = vehicleCategory;
            Distance = distance;
        }
        private static Dictionary<VehicleService, VehicleLaneData> Dictinary { get; } = new Dictionary<VehicleService, VehicleLaneData>()
        {
            {VehicleService.Car, new VehicleLaneData(ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, VehicleInfo.VehicleCategory.RoadTransport, 32f) },
            {VehicleService.Trolleybus, new VehicleLaneData(ItemClass.Service.Road, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Trolleybus, VehicleInfo.VehicleCategory.Trolleybus, 32f) },
            {VehicleService.Tram, new VehicleLaneData(ItemClass.Service.Road, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Tram, VehicleInfo.VehicleCategory.Tram, 32f) },
            {VehicleService.Plane, new VehicleLaneData(ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Plane, VehicleInfo.VehicleCategory.Planes, 16f) },
            {VehicleService.Balloon, new VehicleLaneData(ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Balloon, VehicleInfo.VehicleCategory.PassengerBalloon, 64f) },
            {VehicleService.Blimp, new VehicleLaneData(ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Blimp, VehicleInfo.VehicleCategory.PassengerBlimp, 64f) },
            {VehicleService.Ship, new VehicleLaneData(ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Ship, VehicleInfo.VehicleCategory.Ships, 64f) },
            {VehicleService.Ferry, new VehicleLaneData(ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Ferry, VehicleInfo.VehicleCategory.PassengerFerry, 64f) },
            {VehicleService.Fishing, new VehicleLaneData(ItemClass.Service.Fishing, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Ship, VehicleInfo.VehicleCategory.FishingBoat, 40f) },
            {VehicleService.Train, new VehicleLaneData(ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Train, VehicleInfo.VehicleCategory.Trains, 32f) },
            {VehicleService.Metro, new VehicleLaneData(ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Metro, VehicleInfo.VehicleCategory.MetroTrain, 32f) },
            {VehicleService.Monorail, new VehicleLaneData(ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.Monorail, VehicleInfo.VehicleCategory.Monorail, 32f) },
            {VehicleService.CableCar, new VehicleLaneData(ItemClass.Service.PublicTransport, NetInfo.LaneType.Vehicle, VehicleInfo.VehicleType.CableCar, VehicleInfo.VehicleCategory.CableCar, 32f) },
        };

        public static bool TryGet(VehicleService type, out VehicleLaneData value) => Dictinary.TryGetValue(type, out value);
    }
}
