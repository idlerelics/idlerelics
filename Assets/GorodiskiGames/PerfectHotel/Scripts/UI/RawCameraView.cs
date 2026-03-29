using UnityEngine;

namespace Game.UI.Hud
{
    public sealed class RawCameraView : MonoBehaviour
    {
        [SerializeField] private Transform _spawnPlace;
        [SerializeField] private Camera _camera;

        public Transform SpawnPlace => _spawnPlace;
        public Camera Camera => _camera;
    }
}

