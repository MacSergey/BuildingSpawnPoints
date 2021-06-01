using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using HarmonyLib;
using ICities;
using ModsCommon;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace BuildingSpawnPoints
{
    public class Mod : BasePatcherMod<Mod>
    {
        #region PROPERTIES
        public override string NameRaw => "Building Spawn Points";
        public override string Description => string.Empty;

        public override List<Version> Versions => new List<Version>()
        {
            new Version(1,0)
        };

        protected override ulong StableWorkshopId => 2504315382ul;
        protected override ulong BetaWorkshopId => 2504315382ul;

        protected override string IdRaw => nameof(BuildingSpawnPoints);

#if BETA
        public override bool IsBeta => true;
#else
        public override bool IsBeta => false;
#endif
        #endregion

        public override CultureInfo Culture
        {
            get => Localize.Culture;
            protected set => Localize.Culture = value;
        }

        protected override void GetSettings(UIHelperBase helper)
        {
            var settings = new Settings();
            settings.OnSettingsUI(helper);
        }

        public override string GetLocalizeString(string str, CultureInfo culture = null) => Localize.ResourceManager.GetString(str, culture ?? Culture);

        #region PATCHES

        protected override bool PatchProcess()
        {
            var success = true;

            success &= AddTool();
            success &= ToolOnEscape();
            //success &= AssetDataExtensionFix();

            Patch_BuildingAI(ref success);
            return success;
        }

        private bool AddTool()
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.ToolControllerAwakeTranspiler), typeof(ToolController), "Awake");
        }

        protected bool ToolOnEscape()
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.GameKeyShortcutsEscapeTranspiler), typeof(GameKeyShortcuts), "Escape");
        }

        //private bool AssetDataExtensionFix()
        //{
        //    return AddPostfix(typeof(Patcher), nameof(Patcher.LoadAssetPanelOnLoadPostfix), typeof(LoadAssetPanel), nameof(LoadAssetPanel.OnLoad));
        //}

        private void Patch_BuildingAI(ref bool success)
        {
            var parameters = new Type[] { typeof(ushort), typeof(Building).MakeByRefType(), typeof(Randomizer).MakeByRefType(), typeof(VehicleInfo), typeof(Vector3).MakeByRefType(), typeof(Vector3).MakeByRefType() };

            success &= Patch_BuildingAI_CalculateSpawnPosition(parameters);
            success &= Patch_BuildingAI_CalculateUnspawnPosition(parameters);
        }
        private bool Patch_BuildingAI_CalculateSpawnPosition(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.CalculateSpawnPositionTranspiler), typeof(BuildingAI), nameof(BuildingAI.CalculateSpawnPosition), parameters);
        }
        private bool Patch_BuildingAI_CalculateUnspawnPosition(Type[] parameters)
        {
            return AddTranspiler(typeof(Patcher), nameof(Patcher.CalculateUnpawnPositionTranspiler), typeof(BuildingAI), nameof(BuildingAI.CalculateUnspawnPosition), parameters);
        }

        #endregion
    }

    public static class Patcher
    {
        public static IEnumerable<CodeInstruction> ToolControllerAwakeTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions) => ModsCommon.Patcher.ToolControllerAwakeTranspiler<Mod, SpawnPointsTool>(generator, instructions);

        public static IEnumerable<CodeInstruction> GameKeyShortcutsEscapeTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions) => ModsCommon.Patcher.GameKeyShortcutsEscapeTranspiler<Mod, SpawnPointsTool>(generator, instructions);

        //public static void LoadAssetPanelOnLoadPostfix(LoadAssetPanel __instance, UIListBox ___m_SaveList) => ModsCommon.Patcher.LoadAssetPanelOnLoadPostfix<AssetDataExtension>(__instance, ___m_SaveList);

        public static IEnumerable<CodeInstruction> CalculateSpawnPositionTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => CalculatePositionTranspiler(PointType.Spawn, instructions, original);
        public static IEnumerable<CodeInstruction> CalculateUnpawnPositionTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original) => CalculatePositionTranspiler(PointType.Unspawn, instructions, original);
        private static IEnumerable<CodeInstruction> CalculatePositionTranspiler(PointType type, IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            var enumerator = instructions.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var instruction = enumerator.Current;
                yield return instruction;

                if (instruction.opcode == OpCodes.Ret)
                {
                    enumerator.MoveNext();
                    break;
                }
            }

            yield return new CodeInstruction(OpCodes.Nop) { labels = enumerator.Current.labels };
            yield return new CodeInstruction(OpCodes.Ldc_I4, (int)type);
            yield return original.GetLDArg("buildingID");
            yield return original.GetLDArg("data");
            yield return original.GetLDArg("randomizer");
            yield return original.GetLDArg("info");
            yield return original.GetLDArg("position");
            yield return original.GetLDArg("target");
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patcher), nameof(Patcher.GetPosition)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static void GetPosition(PointType type, ushort buildingID, ref Building data, ref Randomizer randomizer, VehicleInfo info, out Vector3 position, out Vector3 target)
        {
            if (SingletonManager<Manager>.Instance[buildingID] is BuildingData buildingData)
                buildingData.GetPosition(type, ref data, info, ref randomizer, out position, out target);
            else
            {
                position = data.CalculateSidewalkPosition(0f, 2f);
                target = position;
            }

            //var zOffset = info.m_class.m_service == ItemClass.Service.Industrial ? Settings.ZOffset : 2f;
            //position = data.CalculateSidewalkPosition(randomizer.Int32(-16, 16), zOffset);
            //target = position + new Vector3(Mathf.Cos(data.m_angle), 0f, Mathf.Sin(data.m_angle));
        }
    }
}
