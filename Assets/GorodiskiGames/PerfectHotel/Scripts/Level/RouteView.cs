using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Level.Line
{
    /// <summary>
    /// Defines a route (path) as a series of waypoints for NPCs/units to follow.
    /// Child transforms of this GameObject become the waypoints.
    ///
    /// This is an editor-only visualization tool: OnDrawGizmos() draws the path
    /// as connected spheres in the Unity Scene view (not visible in the actual game).
    ///
    /// Gizmos are debug drawing tools that only appear in the Unity Editor's Scene view.
    /// They're great for visualizing paths, radii, and other spatial data while designing levels.
    /// </summary>
    public sealed class RouteView : MonoBehaviour
    {
        [SerializeField] private Color _rayColor = Color.black;   // Color of the route line in Scene view
        [SerializeField] private float _sphereSize = 0.1f;        // Size of the waypoint spheres
        [SerializeField] private List<Transform> _points = new List<Transform>(); // The waypoint list
        private Transform[] _theArray;

        /// <summary>Public access to the route's waypoints, in order.</summary>
        public List<Transform> Points => _points;

        /// <summary>
        /// Called by Unity in the Editor to draw debug visuals in the Scene view.
        /// Finds all child transforms, treats each as a waypoint, and draws:
        /// - A wire sphere at each waypoint position
        /// - A line connecting consecutive waypoints
        ///
        /// GetComponentsInChildren finds all Transform components on this GameObject
        /// and all of its children. We skip 'this.transform' because the parent
        /// itself is not a waypoint -- only the children are.
        /// </summary>
        void OnDrawGizmos()
        {
            Gizmos.color = _rayColor;
            _theArray = GetComponentsInChildren<Transform>();
            _points.Clear();

            // Collect all child transforms as waypoints (skip the parent itself)
            foreach (Transform path_obj in _theArray)
            {
                if (path_obj != this.transform)
                {
                    _points.Add(path_obj);
                }
            }

            // Draw each waypoint and connect them with lines
            for (int i = 0; i < _points.Count; i++)
            {
                Vector3 position = _points[i].position;
                Gizmos.DrawWireSphere(position, _sphereSize); // Draw sphere at waypoint

                if (i > 0)
                {
                    Vector3 previous = _points[i - 1].position;
                    Gizmos.DrawLine(previous, position); // Draw line from previous to current
                }
            }
        }
    }
}
