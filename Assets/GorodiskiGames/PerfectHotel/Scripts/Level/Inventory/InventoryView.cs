using UnityEngine;

namespace Game.Level.Inventory
{
    public enum InventoryType
    {
        None,
        ToiletPaper,
        SodaCan
    }

    public class InventoryView : MonoBehaviour
    {
        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }
    }
}