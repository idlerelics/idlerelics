using Game.UI.Hud;
using UnityEngine;

namespace Game.Level.Item
{
    public class ItemFillBarView : ItemView
    {
        [SerializeField] private FillBarView _fillBar;

        protected override void OnModelChanged(ItemModel model)
        {
            _fillBar.Holder.SetActive(model.Duration > 0);
            _fillBar.Marker.SetActive(model.Duration > 0);

            _fillBar.FillImage.fillAmount = model.Duration / model.DurationNominal;
        }

        private void Update()
        {
            var rotation = Quaternion.LookRotation(Camera.main.transform.position - _fillBar.transform.position);
            rotation.x = 0;
            rotation *= Quaternion.Euler(0, 180, 0);
            _fillBar.transform.rotation = Quaternion.Slerp(_fillBar.transform.rotation, rotation, Time.deltaTime * 10f);
        }
    }
}

