using ModsCommon;
using ModsCommon.UI;
using BuildingSpawnPoints.Utilities;
using static ModsCommon.UI.ComponentStyle;

namespace BuildingSpawnPoints.UI
{
    public class PanelHeader : HeaderMoveablePanel<BaseHeaderContent>
    {
        private HeaderButtonInfo<HeaderButton> PasteButton { get; set; }

        protected override void FillContent()
        {
            Content.AddButton(new HeaderButtonInfo<HeaderButton>("Copy", HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.CopyHeaderButton, BuildingSpawnPoints.Localize.Panel_Copy, OnCopy));

            PasteButton = new HeaderButtonInfo<HeaderButton>("Paste", HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.PasteHeaderButton, BuildingSpawnPoints.Localize.Panel_Paste, OnPaste);
            Content.AddButton(PasteButton);

            Content.AddButton(new HeaderButtonInfo<HeaderButton>("Apply to all", HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.ApplyAllHeaderButton, BuildingSpawnPoints.Localize.Panel_ApplyToAll, OnApplyToAll));

            Content.AddButton(new HeaderButtonInfo<HeaderButton>("Reset", HeaderButtonState.Main, SpawnPointsTextures.Atlas, SpawnPointsTextures.ResetHeaderButton, BuildingSpawnPoints.Localize.Panel_ResetToDefault, OnResetToDefault));
        }

        private void OnCopy() => SingletonTool<SpawnPointsTool>.Instance.Copy();
        private void OnPaste() => SingletonTool<SpawnPointsTool>.Instance.Paste();
        private void OnApplyToAll() => SingletonTool<SpawnPointsTool>.Instance.ApplyToAll();
        private void OnResetToDefault() => SingletonTool<SpawnPointsTool>.Instance.ResetToDefault();

        public override void Refresh()
        {
            PasteButton.Enable = !SingletonTool<SpawnPointsTool>.Instance.IsBufferEmpty;
            base.Refresh();
        }
        public void Init(float height) => base.Init(height);
    }
}
