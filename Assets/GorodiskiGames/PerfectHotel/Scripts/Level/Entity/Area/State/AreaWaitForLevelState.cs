using Injection;

namespace Game.Level.Area
{
    /// <summary>
    /// State for an area that is not yet purchasable because the player's level
    /// is too low. The area is completely hidden — no geometry, no HUD, no interaction.
    ///
    /// This state listens for level changes and area purchases. When the player
    /// reaches the required level (or a prerequisite area is purchased), it
    /// automatically transitions to AreaReadyToPurchaseState.
    /// </summary>
    public sealed class AreaWaitForLevelState : AreaState
    {
        [Inject] private GameManager _gameManager;

        private AreaController _prevArea;

        public override void Initialize()
        {
            int prevAreaNumber = _area.View.Config.Number - 1;
            if (prevAreaNumber > 1)
                _prevArea = _gameManager.FindArea(prevAreaNumber);

            _area.Model.IsLocked = true;
            _area.Model.SetChanged();

            // Hide everything — locked areas should be invisible
            _area.View.HudView.gameObject.SetActive(false);
            _area.View.HideFloors();
            _area.View.HideHidingWalls();
            _area.View.HidePermanentWalls();

            // Check if we already meet the level requirement (e.g., after loading a save)
            OnLvlChanged(_gameManager.Model.LoadLvl());

            _gameManager.LEVEL_CHANGED += OnLvlChanged;
            _gameManager.AREA_PURCHASED += OnAreaPurchased;
        }

        public override void Dispose()
        {
            _gameManager.LEVEL_CHANGED -= OnLvlChanged;
            _gameManager.AREA_PURCHASED -= OnAreaPurchased;
        }

        private void OnAreaPurchased(AreaController area)
        {
            if (_prevArea == null) return;
            if (area.View.Config.Number != _prevArea.View.Config.Number) return;

            OnLvlChanged(_gameManager.Model.LoadLvl());
        }

        private void OnLvlChanged(int lvl)
        {
            if (_area.IsPurchasable(lvl, _area.Model.TargetPurchaseValue))
                _area.SwitchToState(new AreaReadyToPurchaseState());
        }
    }
}
