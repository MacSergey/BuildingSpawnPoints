using ModsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuildingSpawnPoints
{
    public class Manager : IManager
    {
        private BuildingData[] Buffer { get; set; }

        public Manager()
        {
            SingletonMod<Mod>.Logger.Debug("Create manager");
            Buffer = new BuildingData[BuildingManager.MAX_BUILDING_COUNT];
        }

        public BuildingData this[ushort id, bool create = false]
        {
            get
            {
                if (Buffer[id] is not BuildingData data)
                {
                    if (create)
                    {
                        data = new BuildingData(id);
                        Buffer[id] = data;
                    }
                    else
                        data = null;
                }
                return data;
            }
        }
    }
}
