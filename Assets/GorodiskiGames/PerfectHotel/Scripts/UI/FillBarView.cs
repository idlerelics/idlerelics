using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class FillBarView : MonoBehaviour
    {
        [SerializeField] private GameObject _holder;
        [SerializeField] private GameObject _marker;
        [SerializeField] private Image _fillImage;

        public GameObject Holder => _holder;
        public Image FillImage => _fillImage;
        public GameObject Marker => _marker;
    }
}

