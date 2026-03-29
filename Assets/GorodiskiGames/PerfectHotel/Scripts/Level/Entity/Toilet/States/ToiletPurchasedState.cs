using Game.Config;
using Game.Level.Item;
using Game.Level.Unit;
using Injection;

namespace Game.Level.Toilet
{
    public sealed class ToiletPurchasedState : ToiletState
    {
        [Inject] private GameConfig _config;

        public override void Initialize()
        {
            _toilet.View.HideWallsHiddenState();

            _toilet.View.MeshesHolder.SetActive(true);
            _toilet.View.HudView.gameObject.SetActive(false);

            foreach (var cabine in _toilet.CabinesMap.Keys)
            {
                if (_toilet.CabinesMap[cabine])
                    SetCabineAvailable(cabine);
                else
                    SetCabineUsed(cabine);
            }

            OnLvlChanged(_gameManager.Model.LoadLvl());

            _gameManager.LEVEL_CHANGED += OnLvlChanged;
        }

        public override void Dispose()
        {
            _gameManager.LEVEL_CHANGED -= OnLvlChanged;
        }

        private void OnLvlChanged(int lvl)
        {
            _toilet.View.OutsideWalls.MeshesVisibilityLvl(lvl);
            _toilet.View.UpdateWallsPurchasedState(lvl);
        }

        private void SetCabineAvailable(ItemController item)
        {
            ItemToiletController cabine = item as ItemToiletController;

            cabine.View.SetVisual(true, _toilet.Model.VisualIndex);
            cabine.UNIT_LEFT_TOILET_CABINE += OnUnitLeftToiletCabine;
        }

        private void SetCabineUsed(ItemController item)
        {
            ItemToiletController cabine = item as ItemToiletController;

            cabine.View.SetVisual(false, _toilet.Model.VisualIndex);

            cabine.Model.Duration = _config.ToiletPaperFlyTime;
            cabine.Model.DurationNominal = cabine.Model.Duration;
            cabine.Model.SetChanged();

            _gameManager.AddItem(cabine);

            cabine.ITEM_FINISHED += OnToiletPaperDelivered;
        }

        private void OnToiletPaperDelivered(ItemController item)
        {
            ItemToiletController cabine = item as ItemToiletController;

            cabine.ITEM_FINISHED -= OnToiletPaperDelivered;

            cabine.VisitsCount = 0;
            _gameManager.Model.SaveVisitsCount(cabine.ID, cabine.VisitsCount);

            _toilet.CabinesMap[cabine] = true;
            
            SetCabineAvailable(cabine);

            CheckIfCustomerCanProceedToilet();
        }

        private void OnUnitLeftToiletCabine(ItemController item)
        {
            ItemToiletController cabine = item as ItemToiletController;

            cabine.UNIT_LEFT_TOILET_CABINE -= OnUnitLeftToiletCabine;

            _toilet.Model.Cash += _toilet.Model.StayFee;
            _toilet.Model.SetChanged();
            _gameManager.Model.SavePlaceCash(_toilet.Model.ID, _toilet.Model.Cash);

            cabine.VisitsCount++;
            _gameManager.Model.SaveVisitsCount(cabine.ID, cabine.VisitsCount);

            bool isAvailable = cabine.IsAvailable;

            _toilet.CabinesMap[cabine] = isAvailable;

            if (isAvailable)
                SetCabineAvailable(cabine);
            else
                SetCabineUsed(cabine);

            CheckIfCustomerCanProceedToilet();
        }

        private void CheckIfCustomerCanProceedToilet()
        {
            var customer = _toilet.Line.GetFirstCustomer();
            var cabine = _toilet.GetAvailableCabine();

            if (customer != null && cabine != null)
            {
                customer.SwitchToState(new UnitWalkToCabineState(_toilet, cabine));
                _toilet.Line.RearrangeCustomersLine();
            }
        }
    }
}


