using UnityEngine;

namespace Game.Level.Cash
{
    public class CashView : MonoBehaviour
    {
        [SerializeField] private GameObject _goTrail;
        [SerializeField] private Transform _rotationNode;

        public Transform Rotation => _rotationNode;


        public void ShowTrail()
        {
            _goTrail.SetActive(true);
        }

        public void HideTrail()
        {
            _goTrail.SetActive(false);
        }
    }
}