using System.Collections.Generic;
using Game;
using UnityEngine;

namespace Game.Level
{
    public sealed class LevelView : MonoBehaviour
    {
        [SerializeField] private Transform _unitSpawnPoint;
        [SerializeField] private Transform _unitRemovePoint;

        private Dictionary<int, int> _progressMaxMap;

        public Transform UnitRemovePoint => _unitSpawnPoint;
        public Transform UnitSpawnPoint => _unitSpawnPoint;

        public int MaxLevels => _progressMaxMap.Count + 1;

        private void Awake()
        {
            _progressMaxMap = new Dictionary<int, int>();
        }

        public void AddLvl(int lvl)
        {
            _progressMaxMap.Add(lvl, 0);
        }

        public void AddReward(int lvl, int reward)
        {
            _progressMaxMap[lvl] += reward;
        }

        public int MaxProgress(int currentLvl)
        {
            int value = 0;
            foreach (var lvl in _progressMaxMap.Keys)
            {
                if (lvl <= currentLvl)
                    value += _progressMaxMap[lvl];
            }
            return value;
        }

        public void DedugLvlProgreses()
        {
            foreach (var lvl in _progressMaxMap.Keys)
            {
                Log.Info("Lvl: " + lvl + " Progress: " + _progressMaxMap[lvl]);
            }
        }
    }
}

