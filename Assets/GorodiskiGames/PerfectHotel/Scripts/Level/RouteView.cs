using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Level.Line
{
    public sealed class RouteView : MonoBehaviour
    {
        [SerializeField] private Color _rayColor = Color.black;
        [SerializeField] private float _sphereSize = 0.1f;
        [SerializeField] private List<Transform> _points = new List<Transform>();
        private Transform[] _theArray;

        public List<Transform> Points => _points;

        void OnDrawGizmos()
        {
            Gizmos.color = _rayColor;
            _theArray = GetComponentsInChildren<Transform>();
            _points.Clear();

            foreach (Transform path_obj in _theArray)
            {
                if (path_obj != this.transform)
                {
                    _points.Add(path_obj);
                }
            }

            for (int i = 0; i < _points.Count; i++)
            {
                Vector3 position = _points[i].position;
                Gizmos.DrawWireSphere(position, _sphereSize);

                if (i > 0)
                {
                    Vector3 previous = _points[i - 1].position;
                    Gizmos.DrawLine(previous, position);
                }
            }
        }
    }
}


