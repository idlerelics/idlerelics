using UnityEngine;

namespace Game.Level.Place
{
    public class ConstructionItemView : MonoBehaviour
    {
        [SerializeField] private GameObject[] _meshes;

        public void HideAllMeshes()
        {
            foreach (var mesh in _meshes)
            {
                mesh.SetActive(false);
            }
        }

        internal void MeshesVisibilityLvl(int lvl)
        {
            int targetIndex = lvl - 1;
            int meshCount = _meshes.Length;

            if (lvl > meshCount)
                targetIndex = meshCount - 1;

            for (int i = 0; i < meshCount; i++)
            {
                bool value = i == targetIndex;
                _meshes[i].SetActive(value);
            }
        }

        internal void MeshesVisibilityIndex(int visualIndex)
        {
            for (int i = 0; i < _meshes.Length; i++)
            {
                _meshes[i].SetActive(i == visualIndex);
            }
        }
    }
}

