using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuildingSpawnPoints.Utilites
{
    public class ObjectsMap : BaseObjectsMap<ObjectId>
    {
        public ObjectsMap(bool isSimple = false) : base(isSimple) { }

        public bool TryGetBuilding(ushort buildingIdKey, out ushort buildingIdValue)
        {
            if (Map.TryGetValue(new ObjectId() { Building = buildingIdKey }, out ObjectId value))
            {
                buildingIdValue = value.Building;
                return true;
            }
            else
            {
                buildingIdValue = default;
                return false;
            }
        }

        public void AddBuilding(ushort source, ushort target) => this[new ObjectId() { Building = source }] = new ObjectId() { Building = target };
    }
    public class ObjectId : ModsCommon.Utilities.ObjectId
    {
        public static long BuildingType = 1L << 32;

        public ushort Building
        {
            get => (Id & BuildingType) == 0 ? 0 : (ushort)(Id & DataMask);
            set => Id = BuildingType | value;
        }

        public override string ToString()
        {
            if (Type == BuildingType)
                return $"{nameof(Building)}: {Building}";
            else
                return base.ToString();
        }
    }
}
