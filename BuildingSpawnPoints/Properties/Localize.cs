namespace BuildingSpawnPoints
{
	public class Localize
	{
		public static System.Globalization.CultureInfo Culture {get; set;}
		public static ModsCommon.LocalizeManager LocaleManager {get;} = new ModsCommon.LocalizeManager("Localize", typeof(Localize).Assembly);

		/// <summary>
		/// Change and add vehicles spawn points
		/// </summary>
		public static string Mod_Description => LocaleManager.GetString("Mod_Description", Culture);

		/// <summary>
		/// [FIXED] Resolved conflict with More Effective Transfer Manager mod.
		/// </summary>
		public static string Mod_WhatsNewMessage1_0_1 => LocaleManager.GetString("Mod_WhatsNewMessage1_0_1", Culture);

		/// <summary>
		/// [TRANSLATION] Added Italian translation.
		/// </summary>
		public static string Mod_WhatsNewMessage1_0_2 => LocaleManager.GetString("Mod_WhatsNewMessage1_0_2", Culture);

		/// <summary>
		/// [NEW] Added depot (bus, trolley, tram, train, copter, blimp, etc). Now posible make custom depots.
		/// </summary>
		public static string Mod_WhatsNewMessage1_1 => LocaleManager.GetString("Mod_WhatsNewMessage1_1", Culture);

		/// <summary>
		/// [NEW] Added a warning if the point is too far from the road. The point should be no further than 32 
		/// </summary>
		public static string Mod_WhatsNewMessage1_2 => LocaleManager.GetString("Mod_WhatsNewMessage1_2", Culture);

		/// <summary>
		/// [TRANSLATION] Added Korean translation.
		/// </summary>
		public static string Mod_WhatsNewMessage1_2_1 => LocaleManager.GetString("Mod_WhatsNewMessage1_2_1", Culture);

		/// <summary>
		/// [TRANSLATION] Added Hungarian translation.
		/// </summary>
		public static string Mod_WhatsNewMessage1_2_2 => LocaleManager.GetString("Mod_WhatsNewMessage1_2_2", Culture);

		/// <summary>
		/// [TRANSLATION] Added Danish and Turkish translations.
		/// </summary>
		public static string Mod_WhatsNewMessage1_2_3 => LocaleManager.GetString("Mod_WhatsNewMessage1_2_3", Culture);

		/// <summary>
		/// [TRANSLATION] Added Czech and Romanian translations.
		/// </summary>
		public static string Mod_WhatsNewMessage1_2_4 => LocaleManager.GetString("Mod_WhatsNewMessage1_2_4", Culture);

		/// <summary>
		/// [UPDATED] Added Plazas & Promenades DLC support.
		/// </summary>
		public static string Mod_WhatsNewMessage1_3 => LocaleManager.GetString("Mod_WhatsNewMessage1_3", Culture);

		/// <summary>
		/// [FIXED] Police car spawn point.
		/// </summary>
		public static string Mod_WhatsNewMessage1_3_1 => LocaleManager.GetString("Mod_WhatsNewMessage1_3_1", Culture);

		/// <summary>
		/// Add all types of vehicles
		/// </summary>
		public static string Panel_AddAllVehicle => LocaleManager.GetString("Panel_AddAllVehicle", Culture);

		/// <summary>
		/// Add point
		/// </summary>
		public static string Panel_AddPoint => LocaleManager.GetString("Panel_AddPoint", Culture);

		/// <summary>
		/// Add vehicle type
		/// </summary>
		public static string Panel_AddVehicle => LocaleManager.GetString("Panel_AddVehicle", Culture);

		/// <summary>
		/// Add vehicle type group
		/// </summary>
		public static string Panel_AddVehicleGroup => LocaleManager.GetString("Panel_AddVehicleGroup", Culture);

		/// <summary>
		/// {0}°
		/// </summary>
		public static string Panel_AngleFormat => LocaleManager.GetString("Panel_AngleFormat", Culture);

		/// <summary>
		/// Apply to all buildings of this type
		/// </summary>
		public static string Panel_ApplyToAll => LocaleManager.GetString("Panel_ApplyToAll", Culture);

		/// <summary>
		/// Copy
		/// </summary>
		public static string Panel_Copy => LocaleManager.GetString("Panel_Copy", Culture);

		/// <summary>
		/// Copy point
		/// </summary>
		public static string Panel_CopyPoint => LocaleManager.GetString("Panel_CopyPoint", Culture);

		/// <summary>
		/// Duplicate point
		/// </summary>
		public static string Panel_DuplicatePoint => LocaleManager.GetString("Panel_DuplicatePoint", Culture);

		/// <summary>
		/// No point in this building contains these types of vehicles:
		/// </summary>
		public static string Panel_NoPointWarning => LocaleManager.GetString("Panel_NoPointWarning", Culture);

		/// <summary>
		/// They will spawn in default position.
		/// </summary>
		public static string Panel_NoPointWarningContinue => LocaleManager.GetString("Panel_NoPointWarningContinue", Culture);

		/// <summary>
		/// Paste
		/// </summary>
		public static string Panel_Paste => LocaleManager.GetString("Panel_Paste", Culture);

		/// <summary>
		/// Paste point (append vehicle types)
		/// </summary>
		public static string Panel_PastePointAppend => LocaleManager.GetString("Panel_PastePointAppend", Culture);

		/// <summary>
		/// Paste point (replace vehicle types)
		/// </summary>
		public static string Panel_PastePointReplace => LocaleManager.GetString("Panel_PastePointReplace", Culture);

		/// <summary>
		/// {0}m
		/// </summary>
		public static string Panel_PositionFormat => LocaleManager.GetString("Panel_PositionFormat", Culture);

		/// <summary>
		/// Reset to default
		/// </summary>
		public static string Panel_ResetToDefault => LocaleManager.GetString("Panel_ResetToDefault", Culture);

		/// <summary>
		/// Building #{0}
		/// </summary>
		public static string Panel_Title => LocaleManager.GetString("Panel_Title", Culture);

		/// <summary>
		/// This point placed too far from road for some type of vehicles, move it closer, otherwise it will not
		/// </summary>
		public static string Panel_TooFarPoint => LocaleManager.GetString("Panel_TooFarPoint", Culture);

		/// <summary>
		/// Middle
		/// </summary>
		public static string PointType_Middle => LocaleManager.GetString("PointType_Middle", Culture);

		/// <summary>
		/// Spawn
		/// </summary>
		public static string PointType_Spawn => LocaleManager.GetString("PointType_Spawn", Culture);

		/// <summary>
		/// Unspawn
		/// </summary>
		public static string PointType_Unspawn => LocaleManager.GetString("PointType_Unspawn", Culture);

		/// <summary>
		/// Type
		/// </summary>
		public static string Property_PointType => LocaleManager.GetString("Property_PointType", Culture);

		/// <summary>
		/// Position
		/// </summary>
		public static string Property_Position => LocaleManager.GetString("Property_Position", Culture);

		/// <summary>
		/// Apply to all buildings of type
		/// </summary>
		public static string Tool_ApplyToAllCaption => LocaleManager.GetString("Tool_ApplyToAllCaption", Culture);

		/// <summary>
		/// Do you really want apply this setting to all buildings of this type?
		/// </summary>
		public static string Tool_ApplyToAllMessage => LocaleManager.GetString("Tool_ApplyToAllMessage", Culture);

		/// <summary>
		/// Building #{0}
		/// </summary>
		public static string Tool_InfoHoverBuilding => LocaleManager.GetString("Tool_InfoHoverBuilding", Culture);

		/// <summary>
		/// Select building
		/// </summary>
		public static string Tool_InfoSelectBuilding => LocaleManager.GetString("Tool_InfoSelectBuilding", Culture);

		/// <summary>
		/// Air vehicles
		/// </summary>
		public static string VehicleTypeGroup_Air => LocaleManager.GetString("VehicleTypeGroup_Air", Culture);

		/// <summary>
		/// Cargo vehicles
		/// </summary>
		public static string VehicleTypeGroup_Cargo => LocaleManager.GetString("VehicleTypeGroup_Cargo", Culture);

		/// <summary>
		/// Copters
		/// </summary>
		public static string VehicleTypeGroup_Copters => LocaleManager.GetString("VehicleTypeGroup_Copters", Culture);

		/// <summary>
		/// Default vehicles
		/// </summary>
		public static string VehicleTypeGroup_Default => LocaleManager.GetString("VehicleTypeGroup_Default", Culture);

		/// <summary>
		/// Passenger vehicles
		/// </summary>
		public static string VehicleTypeGroup_Passenger => LocaleManager.GetString("VehicleTypeGroup_Passenger", Culture);

		/// <summary>
		/// Passenger air vehicles
		/// </summary>
		public static string VehicleTypeGroup_PassengerAir => LocaleManager.GetString("VehicleTypeGroup_PassengerAir", Culture);

		/// <summary>
		/// Passenger rail vehicles
		/// </summary>
		public static string VehicleTypeGroup_PassengerRail => LocaleManager.GetString("VehicleTypeGroup_PassengerRail", Culture);

		/// <summary>
		/// Passenger road vehicles
		/// </summary>
		public static string VehicleTypeGroup_PassengerRoad => LocaleManager.GetString("VehicleTypeGroup_PassengerRoad", Culture);

		/// <summary>
		/// Passenger water vehicles
		/// </summary>
		public static string VehicleTypeGroup_PassengerWater => LocaleManager.GetString("VehicleTypeGroup_PassengerWater", Culture);

		/// <summary>
		/// Planes
		/// </summary>
		public static string VehicleTypeGroup_Planes => LocaleManager.GetString("VehicleTypeGroup_Planes", Culture);

		/// <summary>
		/// Rail vehicles
		/// </summary>
		public static string VehicleTypeGroup_Rail => LocaleManager.GetString("VehicleTypeGroup_Rail", Culture);

		/// <summary>
		/// Road vehicles
		/// </summary>
		public static string VehicleTypeGroup_Road => LocaleManager.GetString("VehicleTypeGroup_Road", Culture);

		/// <summary>
		/// Service vehicles
		/// </summary>
		public static string VehicleTypeGroup_Service => LocaleManager.GetString("VehicleTypeGroup_Service", Culture);

		/// <summary>
		/// Ships
		/// </summary>
		public static string VehicleTypeGroup_Ships => LocaleManager.GetString("VehicleTypeGroup_Ships", Culture);

		/// <summary>
		/// Trains
		/// </summary>
		public static string VehicleTypeGroup_Trains => LocaleManager.GetString("VehicleTypeGroup_Trains", Culture);

		/// <summary>
		/// Trucks
		/// </summary>
		public static string VehicleTypeGroup_Trucks => LocaleManager.GetString("VehicleTypeGroup_Trucks", Culture);

		/// <summary>
		/// Water vehicles
		/// </summary>
		public static string VehicleTypeGroup_Water => LocaleManager.GetString("VehicleTypeGroup_Water", Culture);

		/// <summary>
		/// All
		/// </summary>
		public static string VehicleType_All => LocaleManager.GetString("VehicleType_All", Culture);

		/// <summary>
		/// Ambulance
		/// </summary>
		public static string VehicleType_Ambulance => LocaleManager.GetString("VehicleType_Ambulance", Culture);

		/// <summary>
		/// Ambulance copter
		/// </summary>
		public static string VehicleType_AmbulanceCopter => LocaleManager.GetString("VehicleType_AmbulanceCopter", Culture);

		/// <summary>
		/// Balloon
		/// </summary>
		public static string VehicleType_Balloon => LocaleManager.GetString("VehicleType_Balloon", Culture);

		/// <summary>
		/// Bicycle
		/// </summary>
		public static string VehicleType_Bicycle => LocaleManager.GetString("VehicleType_Bicycle", Culture);

		/// <summary>
		/// Bus
		/// </summary>
		public static string VehicleType_Bus => LocaleManager.GetString("VehicleType_Bus", Culture);

		/// <summary>
		/// Cable car
		/// </summary>
		public static string VehicleType_CableCar => LocaleManager.GetString("VehicleType_CableCar", Culture);

		/// <summary>
		/// Cargo plane
		/// </summary>
		public static string VehicleType_CargoPlane => LocaleManager.GetString("VehicleType_CargoPlane", Culture);

		/// <summary>
		/// Cargo ship
		/// </summary>
		public static string VehicleType_CargoShip => LocaleManager.GetString("VehicleType_CargoShip", Culture);

		/// <summary>
		/// Cargo train
		/// </summary>
		public static string VehicleType_CargoTrain => LocaleManager.GetString("VehicleType_CargoTrain", Culture);

		/// <summary>
		/// Cargo truck
		/// </summary>
		public static string VehicleType_CargoTruck => LocaleManager.GetString("VehicleType_CargoTruck", Culture);

		/// <summary>
		/// Disaster response copter
		/// </summary>
		public static string VehicleType_DisasterCopter => LocaleManager.GetString("VehicleType_DisasterCopter", Culture);

		/// <summary>
		/// Disaster response
		/// </summary>
		public static string VehicleType_DisasterResponse => LocaleManager.GetString("VehicleType_DisasterResponse", Culture);

		/// <summary>
		/// Fire copter
		/// </summary>
		public static string VehicleType_FireCopter => LocaleManager.GetString("VehicleType_FireCopter", Culture);

		/// <summary>
		/// Fire truck
		/// </summary>
		public static string VehicleType_FireTruck => LocaleManager.GetString("VehicleType_FireTruck", Culture);

		/// <summary>
		/// Fishing boat
		/// </summary>
		public static string VehicleType_FishingBoat => LocaleManager.GetString("VehicleType_FishingBoat", Culture);

		/// <summary>
		/// Garbage truck
		/// </summary>
		public static string VehicleType_GarbageTruck => LocaleManager.GetString("VehicleType_GarbageTruck", Culture);

		/// <summary>
		/// Hearse
		/// </summary>
		public static string VehicleType_Hearse => LocaleManager.GetString("VehicleType_Hearse", Culture);

		/// <summary>
		/// Metro
		/// </summary>
		public static string VehicleType_MetroTrain => LocaleManager.GetString("VehicleType_MetroTrain", Culture);

		/// <summary>
		/// Monorail
		/// </summary>
		public static string VehicleType_Monorail => LocaleManager.GetString("VehicleType_Monorail", Culture);

		/// <summary>
		/// Park maintenance
		/// </summary>
		public static string VehicleType_ParkMaintenanceTruck => LocaleManager.GetString("VehicleType_ParkMaintenanceTruck", Culture);

		/// <summary>
		/// Passenger blimp
		/// </summary>
		public static string VehicleType_PassengerBlimp => LocaleManager.GetString("VehicleType_PassengerBlimp", Culture);

		/// <summary>
		/// Passenger copter
		/// </summary>
		public static string VehicleType_PassengerCopter => LocaleManager.GetString("VehicleType_PassengerCopter", Culture);

		/// <summary>
		/// Passenger ferry
		/// </summary>
		public static string VehicleType_PassengerFerry => LocaleManager.GetString("VehicleType_PassengerFerry", Culture);

		/// <summary>
		/// Passenger plane
		/// </summary>
		public static string VehicleType_PassengerPlane => LocaleManager.GetString("VehicleType_PassengerPlane", Culture);

		/// <summary>
		/// Passenger ship
		/// </summary>
		public static string VehicleType_PassengerShip => LocaleManager.GetString("VehicleType_PassengerShip", Culture);

		/// <summary>
		/// Passenger train
		/// </summary>
		public static string VehicleType_PassengerTrain => LocaleManager.GetString("VehicleType_PassengerTrain", Culture);

		/// <summary>
		/// Police
		/// </summary>
		public static string VehicleType_Police => LocaleManager.GetString("VehicleType_Police", Culture);

		/// <summary>
		/// Police copter
		/// </summary>
		public static string VehicleType_PoliceCopter => LocaleManager.GetString("VehicleType_PoliceCopter", Culture);

		/// <summary>
		/// Post
		/// </summary>
		public static string VehicleType_PostTruck => LocaleManager.GetString("VehicleType_PostTruck", Culture);

		/// <summary>
		/// Private plane
		/// </summary>
		public static string VehicleType_PrivatePlane => LocaleManager.GetString("VehicleType_PrivatePlane", Culture);

		/// <summary>
		/// Road maintenance
		/// </summary>
		public static string VehicleType_RoadMaintenanceTruck => LocaleManager.GetString("VehicleType_RoadMaintenanceTruck", Culture);

		/// <summary>
		/// Rocket
		/// </summary>
		public static string VehicleType_Rocket => LocaleManager.GetString("VehicleType_Rocket", Culture);

		/// <summary>
		/// Snowplow
		/// </summary>
		public static string VehicleType_SnowTruck => LocaleManager.GetString("VehicleType_SnowTruck", Culture);

		/// <summary>
		/// Taxi
		/// </summary>
		public static string VehicleType_Taxi => LocaleManager.GetString("VehicleType_Taxi", Culture);

		/// <summary>
		/// Tram
		/// </summary>
		public static string VehicleType_Tram => LocaleManager.GetString("VehicleType_Tram", Culture);

		/// <summary>
		/// Trolleybus
		/// </summary>
		public static string VehicleType_Trolleybus => LocaleManager.GetString("VehicleType_Trolleybus", Culture);

		/// <summary>
		/// Vacuum truck
		/// </summary>
		public static string VehicleType_VacuumTruck => LocaleManager.GetString("VehicleType_VacuumTruck", Culture);
	}
}