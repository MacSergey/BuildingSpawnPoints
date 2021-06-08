using ModsCommon;
using MoveItIntegration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace BuildingSpawnPoints.Utilites
{
    public class MoveItIntegrationFactory : IMoveItIntegrationFactory
    {
        public MoveItIntegrationBase GetInstance() => new MoveItIntegration();
    }

    public class MoveItIntegration : MoveItIntegrationBase
    {
        public override string ID => $"CS.macsergey.{nameof(BuildingSpawnPoints)}";

        public override Version DataVersion => new Version(1, 0);

        public override object Copy(InstanceID sourceInstanceID)
        {
            if (SingletonManager<Manager>.Instance[sourceInstanceID.Building] is BuildingData data)
                return data.ToXml();
            else
                return null;
        }
        public override void Paste(InstanceID targetInstanceID, object record, Dictionary<InstanceID, InstanceID> sourceMap)
        {
            if (record is not XElement config || targetInstanceID.Building == 0)
                return;

            if (SingletonManager<Manager>.Instance[targetInstanceID.Building, Options.Create] is BuildingData data)
                data.FromXml(config);
        }

        public override string Encode64(object record) => record == null ? null : EncodeUtil.BinaryEncode64(record?.ToString());
        public override object Decode64(string record, Version dataVersion)
        {
            if (record == null || record.Length == 0)
                return null;

            using StringReader input = new StringReader((string)EncodeUtil.BinaryDecode64(record));
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                ProhibitDtd = false,
                XmlResolver = null
            };
            using XmlReader reader = XmlReader.Create(input, xmlReaderSettings);
            return XElement.Load(reader, LoadOptions.None);
        }
    }
}
