using Game.Core;
using Game.UI;
using Injection;
using UnityEngine;

namespace Game.Level.Loader.LoaderStates
{
    public abstract class LoaderState : State
    {
        [Inject] protected LoaderController _loader;
        [Inject] protected Timer _timer;
        [Inject] protected GameManager _gameManager;
        [Inject] protected GameView _gameView;

        //internal void FindUsedItem()
        //{
        //    var targetItem = ItemType.ToiletCabine;
        //    var item = _gameManager.FindUsedItem(-1, targetItem);

        //    if (item == null) return;

        //    Vector3 position = _gameManager.Utility.ItemsMap[InventoryType.ToiletPaper].Position;
        //    _loader.SwitchToState(new LoaderWalkToUtilityState(position));
        //}
    }
}


