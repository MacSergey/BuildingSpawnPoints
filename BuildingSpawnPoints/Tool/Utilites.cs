using ColossalFramework;
using ModsCommon;
using System;

namespace BuildingSpawnPoints
{
    public abstract class SpawnPointsToolMode : BaseToolMode<SpawnPointsTool>, IToolMode<ToolModeType>, IToolModePanel
    {
        public abstract ToolModeType Type { get; }
        public virtual bool ShowPanel => true;
    }
    public enum ToolModeType
    {
        None = 0,
        Select = 1,
        Edit = 2,
        Drag = 4,
    }

    public class SpawnPointsShortcut : ToolShortcut<Mod, SpawnPointsTool, ToolModeType>
    {
        public SpawnPointsShortcut(string name, string labelKey, InputKey key, Action action = null, ToolModeType mode = ToolModeType.None) : base(name, labelKey, key, action, mode) { }
    }
    public class SpawnPointsToolThreadingExtension : BaseUUIThreadingExtension<SpawnPointsTool> { }
    public class SpawnPointsToolLoadingExtension : BaseUUIToolLoadingExtension<SpawnPointsTool> { }
}
