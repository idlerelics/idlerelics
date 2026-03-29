using UnityEngine;

namespace Game.Level.Inventory
{
    /// <summary>
    /// Enum listing the types of inventory items the player can carry.
    /// An enum (enumeration) is a set of named constants -- it makes code more readable
    /// than using magic numbers (0, 1, 2) to represent item types.
    /// </summary>
    public enum InventoryType
    {
        None,         // No item / default value
        ToiletPaper,  // Toilet paper supply for restocking bathrooms
        SodaCan       // Soda can for vending machines
    }

    /// <summary>
    /// The visual representation of an inventory item in the scene.
    /// A simple MonoBehaviour that provides access to the item's world position.
    ///
    /// The Position property uses a "get/set" accessor pattern:
    /// - 'get' reads the transform.position from Unity's Transform component
    /// - 'set' writes a new position, moving the item in the 3D world
    /// </summary>
    public class InventoryView : MonoBehaviour
    {
        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }
    }
}
