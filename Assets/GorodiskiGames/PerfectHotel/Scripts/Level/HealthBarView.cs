using Core;
using Game.UI.Hud;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Level.HealthBar
{
    public class HealthBarModel : Observable
    {
        public int Health;
        public int HealthNominal;
        public Color HealthBarColor;
    }

    public sealed class HealthBarView : BaseHudWithModel<HealthBarModel>
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private GameObject _holder;

        protected override void OnDisable()
        {

        }

        protected override void OnEnable()
        {
        }

        protected override void OnApplyModel(HealthBarModel model)
        {
            base.OnApplyModel(model);

            _fillImage.color = model.HealthBarColor;
        }

        protected override void OnModelChanged(HealthBarModel model)
        {
            _holder.SetActive(model.Health < model.HealthNominal && model.Health > 0);
            _fillImage.fillAmount = (float)model.Health / (float)model.HealthNominal;
            _healthText.text = model.Health.ToString();
        }
    }
}

