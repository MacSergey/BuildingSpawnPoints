using ModsCommon;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuildingSpawnPoints
{
    public class LoadingExtension : BaseLoadingExtension<Mod>
    {
        protected override void OnUnload()
        {
            base.OnUnload();
            SingletonManager<Manager>.Destroy();
        }
    }
}
