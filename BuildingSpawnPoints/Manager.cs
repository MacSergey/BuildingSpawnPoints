using BuildingSpawnPoints.Utilites;
using ModsCommon;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

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

        public BuildingData this[ushort id, Options options = Options.None]
        {
            get
            {
                if (Buffer[id] is not BuildingData data)
                {
                    if (options.IsSet(Options.Create))
                    {
                        data = new BuildingData(id, options.IsSet(Options.Init));
                        Buffer[id] = data;
                    }
                    else
                        data = null;
                }
                return data;
            }
        }

        public XElement ToXml()
        {
            var config = new XElement(nameof(BuildingSpawnPoints));

            config.AddAttr("V", SingletonMod<Mod>.Version);

            foreach (var data in Buffer)
            {
                if (data != null)
                    config.Add(data.ToXml());
            }

            return config;
        }
        public void FromXml(XElement config, ObjectsMap map)
        {
            foreach (var nodeConfig in config.Elements(BuildingData.XmlName))
            {
                if (BuildingData.FromXml(nodeConfig, map, out BuildingData data))
                    Buffer[data.Id] = data;
            }
        }
    }
    public enum Options
    {
        None = 0,
        Create = 1,
        Init = 2,
        Default = Create | Init,
    }
}
