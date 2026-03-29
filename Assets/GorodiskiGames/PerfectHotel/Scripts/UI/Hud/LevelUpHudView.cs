using Game.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class LevelUpHudView : BaseHud
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_Text _lvlText;
        [SerializeField] private TMP_Text _rewardText;

        public Button CloseButton => _closeButton;

        protected override void OnEnable()
        {
        }

        protected override void OnDisable()
        {
        }

        internal void SetLvl(int lvl)
        {
            _lvlText.text = lvl.ToString();
        }

        internal void SetReward(int reward)
        {
            _rewardText.text = GameConstants.CashIcon + " " + reward.ToString();
        }
    }
}

