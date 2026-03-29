using System;
using TMPro;
using UnityEngine;

namespace Game.UI.Hud
{
    public sealed class RoomUpgradeHudView : BaseHud
    {
        private const string _lvlWord = "Level ";

        public event Action ON_APP_QUIT;

        [SerializeField] private GameObject _roomSlotPrefab;
        [SerializeField] private TMP_Text _lvlText;
        [SerializeField] private RectTransform _container;

        public RectTransform Container => _container;
        public GameObject RoomSlotPrefab => _roomSlotPrefab;

        protected override void OnEnable()
        {
        }

        protected override void OnDisable()
        {
        }

        internal void SetLvl(int lvl)
        {
            int lvlNice = lvl;
            lvlNice++;
            _lvlText.text = _lvlWord + lvlNice;
        }

        private void OnApplicationQuit()
        {
            ON_APP_QUIT?.Invoke();
        }
    }
}

