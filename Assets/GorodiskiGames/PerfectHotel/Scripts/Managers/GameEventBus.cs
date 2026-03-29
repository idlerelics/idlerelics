using System;
using Game.Level.Area;
using Game.Level.Item;
using UnityEngine;
using Utilities;

namespace Game
{
    public sealed class GameEventBus : IDisposable
    {
        public event Action<Vector3, int> ADD_GAME_PROGRESS;
        public event Action<int> PROGRESS_CHANGED;
        public event Action<int> LEVEL_CHANGED;
        public event Action<AreaController> AREA_PURCHASED;
        public event Action<Vector3> FLY_TO_REMOVE_CASH;
        public event Action<Vector3, int> ON_NOTIFICATION_NEED_LVL;
        public event Action ELEVATOR_PURCHASED;
        public event Action<ItemController> ON_PLAYER_TRY_DROP_INVENTORY;
        public event Action<bool> ON_PLAYERS_HUD_OPEN;
        public event Action ON_TRY_SHOW_INTERSTITIAL;

        public void FireAddGameProgress(Vector3 position, int progressDelta)
        {
            ADD_GAME_PROGRESS.SafeInvoke(position, progressDelta);
        }

        public void FireProgressChanged(int progress)
        {
            PROGRESS_CHANGED.SafeInvoke(progress);
        }

        public void FireLevelChanged(int lvl)
        {
            LEVEL_CHANGED.SafeInvoke(lvl);
        }

        public void FireAreaPurchased(AreaController area)
        {
            AREA_PURCHASED.SafeInvoke(area);
        }

        public void FireFlyToRemoveCash(Vector3 endPosition)
        {
            FLY_TO_REMOVE_CASH.SafeInvoke(endPosition);
        }

        public void FireNotificationNeedLvl(Vector3 itemPosition, int lvl)
        {
            ON_NOTIFICATION_NEED_LVL.SafeInvoke(itemPosition, lvl);
        }

        public void FireElevatorPurchased()
        {
            ELEVATOR_PURCHASED?.Invoke();
        }

        public void FirePlayerTryDropInventory(ItemController item)
        {
            ON_PLAYER_TRY_DROP_INVENTORY?.Invoke(item);
        }

        public void FirePlayersHudOpen(bool value)
        {
            ON_PLAYERS_HUD_OPEN.SafeInvoke(value);
        }

        public void FireTryShowInterstitial()
        {
            ON_TRY_SHOW_INTERSTITIAL.SafeInvoke();
        }

        public void Dispose()
        {
        }
    }
}
