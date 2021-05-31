using ModsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        protected override ulong StableWorkshopId => 0ul;
        protected override ulong BetaWorkshopId => 0ul;

        protected override string IdRaw => nameof(BuildingSpawnPoints);

#if BETA
        public override bool IsBeta => true;
#else
        public override bool IsBeta => false;
#endif
        #endregion

        protected override bool PatchProcess()
        {
            var success = true;
            return success;
        }
    }
}
