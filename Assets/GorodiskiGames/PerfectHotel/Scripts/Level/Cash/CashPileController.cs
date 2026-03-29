using Game.Level.Entity;

namespace Game.Level.Cash
{
    public sealed class CashPileController
    {
        private readonly CashPileView _view;
        private readonly EntityModel _model;

        public CashPileView View => _view;
        public EntityModel Model => _model;

        public CashPileController(CashPileView view, EntityModel model)
        {
            _view = view;
            _model = model;

            _view.Model = model;
        }
    }
}

