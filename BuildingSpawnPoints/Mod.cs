using ColossalFramework.Math;
using ICities;
using ModsCommon;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using static ColossalFramework.Plugins.PluginManager;

namespace BuildingSpawnPoints
{
    public class Mod : BasePatcherMod<Mod>
    {
        #region PROPERTIES
        public override string NameRaw => "Building Spawn Points";
        public override string Description => !IsBeta ? Localize.Mod_Description : CommonLocalize.Mod_DescriptionBeta;

        public override List<ModVersion> Versions => new List<ModVersion>()
        {
            new ModVersion(new Version(1,4,2), new DateTime(2024, 10, 26)),
            new ModVersion(new Version(1,4,1), new DateTime(2023, 5, 27)),
            new ModVersion(new Version(1,4), new DateTime(2023, 4, 9)),
            new ModVersion(new Version(1,3,2), new DateTime(2022, 12, 13)),
            new ModVersion(new Version(1,3,1), new DateTime(2022, 11, 16)),
            new ModVersion(new Version(1,3), new DateTime(2022, 9, 14)),
            new ModVersion(new Version(1,2,4), new DateTime(2022, 6, 19)),
            new ModVersion(new Version(1,2,3), new DateTime(2021, 8, 25)),
            new ModVersion(new Version(1,2,2), new DateTime(2021, 8, 7)),
            new ModVersion(new Version(1,2,1), new DateTime(2021, 8, 1)),
            new ModVersion(new Version(1,2), new DateTime(2021, 7, 16)),
            new ModVersion(new Version(1,1), new DateTime(2021, 6, 19)),
            new ModVersion(new Version(1,0,2), new DateTime(2021, 6, 12)),
            new ModVersion(new Version(1,0,1), new DateTime(2021, 6, 10)),
            new ModVersion(new Version(1,0), new DateTime(2021, 6, 8)),
        };
        protected override Version RequiredGameVersion => new Version(1, 18, 1, 3);

        protected override ulong StableWorkshopId => 2511258910ul;
        protected override ulong BetaWorkshopId => 2504315382ul;
        public override string CrowdinUrl => "https://crowdin.com/translate/macsergey-other-mods/106";

        protected override string IdRaw => nameof(BuildingSpawnPoints);

#if BETA
        public override bool IsBeta => true;
#else
        public override bool IsBeta => false;
#endif
        #endregion
        protected override LocalizeManager LocalizeManager => Localize.LocaleManager;
        protected override bool NeedMonoDevelopImpl => true;

        private static string METMName => "More Effective Transfer Manager";
        private static ulong METMId => 1680840913ul;
        private static PluginSearcher METMSearcher { get; } = PluginUtilities.GetSearcher(METMName, METMId);
        public static PluginInfo METM => PluginUtilities.GetPlugin(METMSearcher);

        protected override void GetSettings(UIHelperBase helper)
        {
            var settings = new Settings();
            settings.OnSettingsUI(helper);
        }
        protected override void SetCulture(CultureInfo culture) => Localize.Culture = culture;

        #region PATCHES

        protected override bool PatchProcess()
        {
            var success = true;

            success &= AddTool();
            success &= ToolOnEscape();
            success &= Patch_BuildingWorldInfoPanel_Start();

            PatchBuildings(ref success);
            PatchVehicles(ref success);

            if (METM is null)
                Logger.Debug("METM not exist, skip patches");
            else
                Patch_METM(ref success);

            return success;
        }

        private bool AddTool()
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.ToolControllerAwakeTranspiler), typeof(ToolController), "Awake");
        }

        private bool ToolOnEscape()
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.GameKeyShortcutsEscapeTranspiler), typeof(GameKeyShortcuts), "Escape");
        }

        #region BUILDINGS

        private void PatchBuildings(ref bool success)
        {
            var parameters = new Type[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(Randomizer).MakeByRefType(), typeof(VehicleInfo), typeof(Vector3).MakeByRefType(), typeof(Vector3).MakeByRefType() };

            success &= Patch_BuildingAI_CalculateSpawnPosition(parameters);
            success &= Patch_BuildingAI_CalculateUnspawnPosition(parameters);

            success &= Patch_CalculateSpawnPosition(typeof(DepotAI), parameters);
            success &= Patch_CalculateUnspawnPosition(typeof(DepotAI), parameters);

            success &= Patch_CalculateSpawnPosition(typeof(CargoStationAI), parameters);
            success &= Patch_CalculateUnspawnPosition(typeof(CargoStationAI), parameters);

            success &= Patch_CalculateSpawnPosition(typeof(FishingHarborAI), parameters);
            success &= Patch_CalculateUnspawnPosition(typeof(FishingHarborAI), parameters);

            success &= Patch_CalculateSpawnPosition(typeof(PostOfficeAI), parameters);
            success &= Patch_CalculateUnspawnPosition(typeof(PostOfficeAI), parameters);

            success &= Patch_CalculateSpawnPosition(typeof(BankOfficeAI), parameters);
            success &= Patch_CalculateUnspawnPosition(typeof(BankOfficeAI), parameters);

            success &= Patch_CalculateSpawnPosition(typeof(TaxiStandAI), parameters);
            success &= Patch_CalculateUnspawnPosition(typeof(TaxiStandAI), parameters);

            success &= Patch_CalculateSpawnPosition(typeof(MaintenanceDepotAI), parameters);
            success &= Patch_CalculateUnspawnPosition(typeof(MaintenanceDepotAI), parameters);

            success &= Patch_CalculateSpawnPosition(typeof(ShelterAI), parameters);
            success &= Patch_CalculateUnspawnPosition(typeof(ShelterAI), parameters);

            success &= Patch_CalculateSpawnPosition(typeof(ServicePointAI), parameters);
            success &= Patch_CalculateUnspawnPosition(typeof(ServicePointAI), parameters);

            success &= Patch_CalculateUnspawnPosition(typeof(TourBuildingAI), parameters);
        }
        private bool Patch_BuildingWorldInfoPanel_Start()
        {
            return AddPostfix(typeof(Patcher), nameof(Patcher.BuildingWorldInfoPanelStartPostfix), typeof(BuildingWorldInfoPanel), "Start");
        }

        private bool Patch_BuildingAI_CalculateSpawnPosition(Type[] parameters)
        {
            return AddPrefix(typeof(Patcher), nameof(Patcher.BuildingAI_CalculateSpawnPosition_Prefix), typeof(BuildingAI), nameof(BuildingAI.CalculateSpawnPosition), parameters);
        }
        private bool Patch_BuildingAI_CalculateUnspawnPosition(Type[] parameters)
        {
            return AddPrefix(typeof(Patcher), nameof(Patcher.BuildingAI_CalculateUnpawnPosition_Prefix), typeof(BuildingAI), nameof(BuildingAI.CalculateUnspawnPosition), parameters);
        }

        private bool Patch_CalculateSpawnPosition(Type type, Type[] parameters)
        {
            return AddPrefix(typeof(Patcher), nameof(Patcher.CalculateSpawnPosition_Prefix), type, nameof(BuildingAI.CalculateSpawnPosition), parameters);
        }
        private bool Patch_CalculateUnspawnPosition(Type type, Type[] parameters)
        {
            return AddPrefix(typeof(Patcher), nameof(Patcher.CalculateUnspawnPosition_Prefix), type, nameof(BuildingAI.CalculateUnspawnPosition), parameters);
        }

        #endregion

        #region VEHICLES

        private void PatchVehicles(ref bool success)
        {
            var startPathFindParams = new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() };
            success &= Patch_PoliceCarAI_StartPathFind(startPathFindParams);
            success &= Patch_FireTruckAI_StartPathFind(startPathFindParams);
            success &= Patch_DisasterResponseVehicleAI_StartPathFind(startPathFindParams);
            success &= Patch_ParkMaintenanceVehicleAI_StartPathFind(startPathFindParams);
            success &= Patch_BusAI_StartPathFind(startPathFindParams);
            success &= Patch_PassengerTrainAI_StartPathFind(startPathFindParams);
            success &= Patch_PassengerPlaneAI_StartPathFind(startPathFindParams);
            success &= Patch_PassengerBlimpAI_StartPathFind(startPathFindParams);
            success &= Patch_PassengerHelicopterAI_StartPathFind(startPathFindParams);
            success &= Patch_CargoPlaneAI_StartPathFind(startPathFindParams);

            success &= Patch_FireTruckAI_SetSource();
            success &= Patch_DisasterResponseVehicleAI_SetSource();
            //success &= Patch_BalloonAI_SetSource();
            success &= Patch_CargoTrainAI_SetSource();

            success &= Patch_CargoPlaneAI_UpdateBuildingTargetPositions();
        }

        private bool Patch_PoliceCarAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.Service_StartPathFind_Transpiler), typeof(PoliceCarAI), "StartPathFind", parameters);
        }
        private bool Patch_FireTruckAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.Service_StartPathFind_Transpiler), typeof(FireTruckAI), "StartPathFind", parameters);
        }
        private bool Patch_DisasterResponseVehicleAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.Service_StartPathFind_Transpiler), typeof(DisasterResponseVehicleAI), "StartPathFind", parameters);
        }
        private bool Patch_ParkMaintenanceVehicleAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.Service_StartPathFind_Transpiler), typeof(ParkMaintenanceVehicleAI), "StartPathFind", parameters);
        }
        private bool Patch_BusAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.Bus_StartPathFind_Transpiler), typeof(BusAI), "StartPathFind", parameters);
        }
        private bool Patch_PassengerTrainAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.Train_StartPathFind_Transpiler), typeof(PassengerTrainAI), "StartPathFind", parameters);
        }
        private bool Patch_PassengerPlaneAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.Plane_StartPathFind_Transpiler), typeof(PassengerPlaneAI), "StartPathFind", parameters);
        }
        private bool Patch_PassengerBlimpAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.Blimp_StartPathFind_Transpiler), typeof(PassengerBlimpAI), "StartPathFind", parameters);
        }
        private bool Patch_PassengerHelicopterAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.Copter_StartPathFind_Transpiler), typeof(PassengerHelicopterAI), "StartPathFind", parameters);
        }

        private bool Patch_FireTruckAI_SetSource()
        {
            return AddPrefix(typeof(Patcher), nameof(Patcher.FireTruckAI_SetSource_Prefix), typeof(FireTruckAI), nameof(FireTruckAI.SetSource));
        }
        private bool Patch_DisasterResponseVehicleAI_SetSource()
        {
            return AddPrefix(typeof(Patcher), nameof(Patcher.DisasterResponseAI_SetSource_Prefix), typeof(DisasterResponseVehicleAI), nameof(DisasterResponseVehicleAI.SetSource));
        }
        private bool Patch_BalloonAI_SetSource()
        {
            return AddPrefix(typeof(Patcher), nameof(Patcher.BalloonAI_SetSource_Prefix), typeof(BalloonAI), nameof(BalloonAI.SetSource));
        }
        private bool Patch_CargoTrainAI_SetSource()
        {
            return AddPrefix(typeof(Patcher), nameof(Patcher.CargoTrainAI_SetSource_Prefix), typeof(CargoTrainAI), nameof(CargoTrainAI.SetSource));
        }

        private bool Patch_CargoPlaneAI_UpdateBuildingTargetPositions()
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.CargoPlaneAI_FixRandomizer_Transpiler), typeof(CargoPlaneAI), nameof(CargoPlaneAI.UpdateBuildingTargetPositions));
        }
        private bool Patch_CargoPlaneAI_StartPathFind(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.CargoPlaneAI_FixRandomizer_Transpiler), typeof(CargoPlaneAI), "StartPathFind", parameters);
        }

        #endregion

        #region METM

        private void Patch_METM(ref bool success)
        {
            success &= AddTranspiler(typeof(Patcher), nameof(Patcher.METMTargetMethodTranspiler), Type.GetType("MoreEffectiveTransfer.Patch.WarehouseAICalculateSpawnPositionPatch"), "TargetMethod");
            success &= AddTranspiler(typeof(Patcher), nameof(Patcher.METMTargetMethodTranspiler), Type.GetType("MoreEffectiveTransfer.Patch.WarehouseAICalculateUnspawnPositionPatch"), "TargetMethod");
            success &= AddTranspiler(typeof(Patcher), nameof(Patcher.METMPostfixTranspiler), Type.GetType("MoreEffectiveTransfer.Patch.WarehouseAICalculateSpawnPositionPatch"), "Postfix");
            success &= AddTranspiler(typeof(Patcher), nameof(Patcher.METMPostfixTranspiler), Type.GetType("MoreEffectiveTransfer.Patch.WarehouseAICalculateUnspawnPositionPatch"), "Postfix");
        }

        #endregion

        #endregion
    }
}
