using Core;
using Game.Config;
using Game.Level.Player;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Level.Unit
{
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

    public class UnitStaffView : UnitView
    {
        [SerializeField] private Transform _inventoryHolder;
        public Transform InventoryHolder => _inventoryHolder;
    }

    public class UnitView : BehaviourWithModel<PlayerModel>
    {
        [SerializeField] private Transform _localTransform;
        [SerializeField] private Animator _animator;
        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private UnitSexType _sex;

        public Transform LocalTransform => _localTransform;
        public NavMeshAgent NavMeshAgent => _navMeshAgent;
        public UnitSexType Sex => _sex;

        [HideInInspector] public int Index;

        private AnimationType _currentType;
        private float _timeValue;

        public void Walk(int inventories)
        {
            SetLayerWeight(inventories);
            PlayWalkAnimation();
        }

        private void PlayWalkAnimation()
        {
            PlayAnimation(AnimationType.Walk, GetRandomTime());
        }

        public void Idle(UnitSexType sex, int inventories)
        {
            SetLayerWeight(inventories);
            PlayIdleAnimation(sex);
        }

        private void SetLayerWeight(int inventories)
        {
            if (inventories > 0)
                _animator.SetLayerWeight(1, 1f);
            else _animator.SetLayerWeight(1, 0f);
        }

        private void PlayIdleAnimation(UnitSexType sex)
        {
            var animation = AnimationType.Idle;
            if (sex == UnitSexType.Female)
                animation = AnimationType.IdleFemale;

            PlayAnimation(animation, GetRandomTime());
        }

        public void Sleep()
        {
            PlayAnimation(AnimationType.Sleep, GetRandomTime());
        }

        public void Clean()
        {
            PlayAnimation(AnimationType.Idle, GetRandomTime());
        }

        public void Service(AnimationType animationType)
        {
            PlayAnimation(animationType, GetRandomTime());
        }

        public void Reception()
        {
            PlayAnimation(AnimationType.Reception, GetRandomTime());
        }

        public void Throw()
        {
            PlayAnimation(AnimationType.Idle, GetRandomTime());
        }

        private float GetRandomTime()
        {
            return Random.Range(0f, 1f);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Unhide()
        {
            gameObject.SetActive(true);
        }

        public void UpdateCurrentAnimation(int inventories)
        {
            SetLayerWeight(inventories);
            PlayAnimation(_currentType, _timeValue);
        }

        private void PlayAnimation(AnimationType animationType, float timeValue)
        {
            _currentType = animationType;
            _timeValue = timeValue;

            var nameHash = Animator.StringToHash(_currentType.ToString());
            _animator.PlayInFixedTime(nameHash, 0, timeValue);

            _animator.Update(0);
        }

        protected override void OnModelChanged(PlayerModel model)
        {

        }

        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        public Vector3 Euler
        {
            get { return transform.eulerAngles; }
            set { transform.eulerAngles = value; }
        }
    }
}