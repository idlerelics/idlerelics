using System;
using Core;
using Game;
using UnityEngine;
using Utilities;

namespace Game.Level.Item
{
    public class ItemModel : Observable
    {
        public float Duration;
        public float DurationNominal;
    }

    public class ItemController
    {
        private const int _amountDefault = 10;

        public int GetAmount(long cash)
        {
            var amount = _amountDefault;

            if (amount > cash)
                amount = (int)cash;

            return amount;
        }

        public event Action<ItemController> PLAYER_ON_ITEM;
        public event Action<ItemController> ITEM_FINISHED;
        public event Action<ItemController> UNIT_LEFT_TOILET_CABINE;

        private ItemModel _model;

        private Transform _transform;
        private float _radius;
        private ItemType _type;

        public Transform Transform => _transform;
        public float Radius => _radius;
        public ItemType Type => _type;
        public ItemModel Model => _model;
        public int Area;

        public ItemController(Transform transform, float radius, ItemType type)
        {
            _model = new ItemModel();

            _transform = transform;
            _radius = radius;
            _type = type;
        }

        internal virtual void FireItemFinished()
        {
            ITEM_FINISHED.SafeInvoke(this);
        }

        internal virtual void FirePlayerOnItem()
        {
            PLAYER_ON_ITEM.SafeInvoke(this);
        }

        internal void FireUnitLeftToiletCabine()
        {
            UNIT_LEFT_TOILET_CABINE.SafeInvoke(this);
        }
    }

    public sealed class ItemRoomController : ItemController
    {
        public ItemReusableView View;

        public ItemRoomController(Transform transform, float radius, ItemType type, ItemReusableView view, int area) : base(transform, radius, type)
        {
            View = view;
            View.Model = Model;

            Area = area;
        }
    }

    public sealed class ItemToiletController : ItemController
    {
        public string ID;
        public float StayDuration;
        public int VisitsCount;
        public int VisitsCountMax;

        public ItemToiletCabineView View;

        public ItemToiletController(Transform transform, float radius, ItemType type, ItemToiletCabineView view, string id, int area) : base(transform, radius, type)
        {
            View = view;
            View.Model = Model;

            ID = id;
            Area = area;
        }

        public bool IsAvailable => VisitsCount < VisitsCountMax;
    }

    public sealed class ItemReceptionController : ItemController
    {
        public ItemView View;

        public ItemReceptionController(Transform transform, float radius, ItemType type, ItemView view) : base(transform, radius, type)
        {
            View = view;
            View.Model = Model;
        }
    }
}

