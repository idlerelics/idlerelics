using Game.Level.Area;
using Game.Level.Cleaner;
using Game.Level.Elevator;
using Game.Level.Room;
using Game.Level.Toilet;
using Game.Level.Utility;
using UnityEngine;

namespace Game.Level.Entity
{
    public sealed class EntityModuleView : MonoBehaviour
    {
        [HideInInspector] public AreaView[] AreaViews;
        [HideInInspector] public RoomView[] RoomViews;
        [HideInInspector] public CleanerView[] CleanerViews;
        [HideInInspector] public ToiletView[] ToiletViews;
        [HideInInspector] public UtilityView UtilityView;
        [HideInInspector] public ElevatorView[] ElevatorViews;

        private void Awake()
        {
            AreaViews = GetComponentsInChildren<AreaView>();
            RoomViews = GetComponentsInChildren<RoomView>();
            CleanerViews = GetComponentsInChildren<CleanerView>();
            ToiletViews = GetComponentsInChildren<ToiletView>();
            UtilityView = GetComponentInChildren<UtilityView>();
            ElevatorViews = GetComponentsInChildren<ElevatorView>();
        }
    }
}
