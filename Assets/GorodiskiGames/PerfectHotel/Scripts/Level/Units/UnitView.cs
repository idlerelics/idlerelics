using Core;
using Game.Config;
using Game.Level.Player;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Level.Unit
{
    /// <summary>
    /// Enum listing all animation types a unit can play.
    /// Each value maps to an animation state name in the Animator controller.
    /// The ToString() of each enum value is hashed and used to trigger the correct animation clip.
    /// </summary>
    public enum AnimationType
    {
        Walk,
        Idle,
        Sleep,
        Clean,
        Toilet,
        Reception,
        Carry,
        Sit,
        WalkFemale,
        IdleFemale
    }

    /// <summary>
    /// Extended UnitView for staff characters that can carry items.
    /// Adds an InventoryHolder transform where carried items are visually attached.
    /// </summary>
    public class UnitStaffView : UnitView
    {
        // SerializeField exposes a private field in the Unity Inspector so designers
        // can drag-and-drop a Transform reference without making the field public.
        [SerializeField] private Transform _inventoryHolder;

        /// <summary>The Transform where inventory items are visually parented/stacked.</summary>
        public Transform InventoryHolder => _inventoryHolder;
    }

    /// <summary>
    /// Visual representation of a unit (guest, receptionist, cleaner, etc.) in the game world.
    /// Handles animations, NavMesh navigation visibility, and position/rotation.
    ///
    /// Extends BehaviourWithModel&lt;PlayerModel&gt;, which is a base MonoBehaviour that
    /// holds a reference to a data model and calls OnModelChanged() when it updates.
    /// This follows the MVC pattern: UnitView is the "View" that reflects the "Model" data.
    /// </summary>
    public class UnitView : BehaviourWithModel<PlayerModel>
    {
        [SerializeField] private Transform _localTransform;    // Optional local transform for offset adjustments
        [SerializeField] private Animator _animator;            // Unity's animation state machine component
        [SerializeField] private NavMeshAgent _navMeshAgent;   // Unity AI component for pathfinding on a NavMesh
        [SerializeField] private UnitSexType _sex;             // Male or female -- affects which animations play

        /// <summary>Local transform reference for positioning adjustments.</summary>
        public Transform LocalTransform => _localTransform;

        /// <summary>
        /// NavMeshAgent handles AI pathfinding. Enable it to make the unit walk,
        /// disable it when the unit should stay in place (e.g., sitting in a room).
        /// </summary>
        public NavMeshAgent NavMeshAgent => _navMeshAgent;

        /// <summary>The sex/gender of this unit, used to select the correct animation set.</summary>
        public UnitSexType Sex => _sex;

        // HideInInspector prevents this field from showing in the Unity Inspector
        // even though it's public. Useful for fields set by code, not by designers.
        [HideInInspector] public int Index;

        private AnimationType _currentType;   // Tracks which animation is currently playing
        private float _timeValue;             // The normalized time offset used when starting an animation

        /// <summary>
        /// Plays the walk animation with the appropriate layer weight for carrying items.
        /// </summary>
        /// <param name="inventories">Number of items being carried (affects upper body animation layer).</param>
        public void Walk(int inventories)
        {
            SetLayerWeight(inventories);
            PlayWalkAnimation();
        }

        /// <summary>Starts the walk animation at a random time offset to avoid synchronized units.</summary>
        private void PlayWalkAnimation()
        {
            PlayAnimation(AnimationType.Walk, GetRandomTime());
        }

        /// <summary>
        /// Plays the idle animation, choosing male or female variant based on sex type.
        /// </summary>
        /// <param name="sex">Determines whether to use the male or female idle animation.</param>
        /// <param name="inventories">Number of items being carried (affects upper body layer).</param>
        public void Idle(UnitSexType sex, int inventories)
        {
            SetLayerWeight(inventories);
            PlayIdleAnimation(sex);
        }

        /// <summary>
        /// Sets the weight of animation layer 1 (the "carry" layer).
        /// Layer weight of 1 = fully active, 0 = inactive.
        /// When the unit carries items, the upper body layer overrides the base animation
        /// to show a carrying pose while still walking/idling with the lower body.
        /// </summary>
        private void SetLayerWeight(int inventories)
        {
            if (inventories > 0)
                _animator.SetLayerWeight(1, 1f);  // Activate carry layer
            else _animator.SetLayerWeight(1, 0f); // Deactivate carry layer
        }

        /// <summary>Plays the correct idle animation based on the unit's sex type.</summary>
        private void PlayIdleAnimation(UnitSexType sex)
        {
            var animation = AnimationType.Idle;
            if (sex == UnitSexType.Female)
                animation = AnimationType.IdleFemale;

            PlayAnimation(animation, GetRandomTime());
        }

        /// <summary>Plays the sleep animation (used when a guest is resting in a room).</summary>
        public void Sleep()
        {
            PlayAnimation(AnimationType.Sleep, GetRandomTime());
        }

        /// <summary>Plays an idle animation used during cleaning (cleaner units).</summary>
        public void Clean()
        {
            PlayAnimation(AnimationType.Idle, GetRandomTime());
        }

        /// <summary>
        /// Plays a service animation specified by the caller (e.g., Toilet, Sit).
        /// Used for context-specific animations at different facility types.
        /// </summary>
        public void Service(AnimationType animationType)
        {
            PlayAnimation(animationType, GetRandomTime());
        }

        /// <summary>Plays the reception desk animation (receptionist greeting guests).</summary>
        public void Reception()
        {
            PlayAnimation(AnimationType.Reception, GetRandomTime());
        }

        /// <summary>Plays an idle animation used when the unit is discarding/throwing items.</summary>
        public void Throw()
        {
            PlayAnimation(AnimationType.Idle, GetRandomTime());
        }

        /// <summary>
        /// Returns a random float between 0 and 1, used as a time offset when starting animations.
        /// This prevents multiple units from playing their animations perfectly in sync,
        /// which would look unnatural.
        /// </summary>
        private float GetRandomTime()
        {
            return Random.Range(0f, 1f);
        }

        /// <summary>Hides the unit by deactivating its entire GameObject.</summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>Shows the unit by reactivating its entire GameObject.</summary>
        public void Unhide()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Re-plays the current animation with updated layer weights.
        /// Called when the number of carried items changes so the animation
        /// layers reflect the new carry state without interrupting the current action.
        /// </summary>
        public void UpdateCurrentAnimation(int inventories)
        {
            SetLayerWeight(inventories);
            PlayAnimation(_currentType, _timeValue);
        }

        /// <summary>
        /// Core animation method. Converts the AnimationType enum to a hash and plays
        /// the corresponding Animator state at the given time offset.
        ///
        /// Animator.StringToHash() converts a string to an integer hash for faster lookups.
        /// PlayInFixedTime() starts the animation at a specific time in the clip.
        /// _animator.Update(0) forces the Animator to evaluate immediately (avoids 1-frame delay).
        /// </summary>
        private void PlayAnimation(AnimationType animationType, float timeValue)
        {
            _currentType = animationType;
            _timeValue = timeValue;

            // Convert the enum name (e.g., "Walk") to an integer hash for efficient lookup
            var nameHash = Animator.StringToHash(_currentType.ToString());
            // Play the animation on layer 0 (base layer) at the specified time
            _animator.PlayInFixedTime(nameHash, 0, timeValue);

            // Force immediate evaluation so the pose updates this frame
            _animator.Update(0);
        }

        /// <summary>
        /// Called when the data model changes. Currently empty because UnitView
        /// does not need to react to model changes -- animations are driven by
        /// explicit method calls from controllers/states instead.
        /// </summary>
        protected override void OnModelChanged(PlayerModel model)
        {

        }

        /// <summary>Shortcut property to get/set the unit's world position via its Transform.</summary>
        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        /// <summary>Shortcut property to get/set the unit's rotation as Euler angles (degrees).</summary>
        public Vector3 Euler
        {
            get { return transform.eulerAngles; }
            set { transform.eulerAngles = value; }
        }
    }
}
