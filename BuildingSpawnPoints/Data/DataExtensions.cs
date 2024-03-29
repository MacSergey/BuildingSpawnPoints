﻿using BuildingSpawnPoints.Utilites;
using ModsCommon;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BuildingSpawnPoints
{
    public class LoadingExtension : BaseLoadingExtension<Mod>
    {
        protected override void OnUnload()
        {
            base.OnUnload();
            SingletonManager<Manager>.Destroy();
            BuildingSpawnPoint.DestroyMarker();
        }
        protected override void OnPreLoaded()
        {
            BuildingSpawnPoint.CreateMarker();
        }
    }
    public class SerializableDataExtension : BaseSerializableDataExtension<SerializableDataExtension, Mod>
    {
        protected override string Id => nameof(BuildingSpawnPoints);

        protected override XElement GetSaveData() => SingletonManager<Manager>.Instance.ToXml();

        protected override void SetLoadData(XElement config) => SingletonManager<Manager>.Instance.FromXml(config, new ObjectsMap(true));
    }
}
