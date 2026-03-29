using System;
using System.Collections.Generic;
using Core;
using Game.Level.Entity;
using UnityEngine;
using Utilities;

namespace Game.Level.Cash
{
    public sealed class CashPileView : BehaviourWithModel<EntityModel>
    {
        private const int _dollarsPerPack = 2;

        private const float _packSizeX = 0.41f;
        private const float _packSizeY = 0.26f;
        private const float _packSizeZ = 0.66f;
        private const int _packsCountX = 3;
        private const int _packsCountZ = 2;

        public event Action<CashPileView, Vector3> CASH_FLY_TO_PILE;
        public event Action<CashPileView, int> CASH_FLY_TO_PLAYER;

        [SerializeField] private GameObject _marker;
        [SerializeField] private int _packsPerPile = 15;

        private List<Vector3> _packPositions = new List<Vector3>();
        private List<CashController> _cashes = new List<CashController>();
        private long _cash;

        public List<CashController> Cashes => _cashes;

        private void Awake()
        {
            _marker.SetActive(false);
        }

        protected override void OnModelChanged(EntityModel model)
        {
            UpdatePile(model.Cash);
        }

        public void UpdatePile(long newCash)
        {
            if (newCash == _cash)
                return;

            _cash = newCash;

            float packsShouldBeFloat = (float)_cash / _dollarsPerPack;
            int packsShouldBe = (int)packsShouldBeFloat;

            if (packsShouldBe > _packsPerPile)
                packsShouldBe = _packsPerPile;

            if (_packPositions.Count < packsShouldBe)
                GeneratePackPosition(packsShouldBe);

            int packs = _cashes.Count;
            int deltaPacks = packsShouldBe - packs;

            if (deltaPacks > 0)
            {
                for (int i = 0; i < deltaPacks; i++)
                {
                    int index = i + packs;
                    Vector3 endPosition = GetPackPosition(index);

                    CASH_FLY_TO_PILE.SafeInvoke(this, endPosition);
                }
            }
            else if (deltaPacks < 0)
            {
                for (int i = 0; i < Mathf.Abs(deltaPacks); i++)
                {
                    int index = packs - i - 1;
                    CASH_FLY_TO_PLAYER.SafeInvoke(this, index);
                }
            }
        }

        public Vector3 GetPackPosition(int index)
        {
            return _packPositions[index];
        }

        public Vector3 GeneratePackPosition(int packs)
        {
            _packPositions.Clear();
            float _directionX = _packSizeX;
            int packsPerFloor = _packsCountX * _packsCountZ;
            float _xPos = 0f, _yPos = 0f, _zPos = 0f;
            Vector3 _position = FirstPlacePos();

            for (int i = 0; i < packs; i++)
            {
                if (i != 0 && i % _packsCountX == 0)
                {
                    _zPos = _zPos + _packSizeZ;
                    _xPos = _xPos - _directionX;
                    _directionX = -_directionX;
                }
                else
                {
                    _zPos = 0f;
                    if (i == 0) _xPos = 0f;
                    else _xPos = _directionX;
                }
                _yPos = 0f;
                if (i != 0 && i % packsPerFloor == 0)
                {
                    _yPos = _yPos + _packSizeY;

                    _position.x = FirstPlacePos().x;
                    _position.z = FirstPlacePos().z;

                    _zPos = 0f;
                    _xPos = 0f;
                    _directionX = _packSizeX;
                }
                _position += new Vector3(_xPos, _yPos, _zPos);
                _packPositions.Add(_position);
            }
            return _position;
        }

        private Vector3 FirstPlacePos()
        {
            float xValue = (_packsCountX * _packSizeX * 0.5f) - (_packSizeX * 0.5f);
            float yValue = _packSizeY * 0.5f;
            float zValue = (_packsCountZ * _packSizeZ * 0.5f) - (_packSizeZ * 0.5f);

            return new Vector3(-xValue + transform.position.x, yValue + transform.position.y, -zValue + transform.position.z);
        }
    }
}



