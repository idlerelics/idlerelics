using UnityEngine;

namespace Game.Level.Player
{
    public sealed class PlayerSelectionState : PlayerState
    {
        private Vector3 _position;
        private Vector3 _positionCached;

        public PlayerSelectionState(Vector3 position)
        {
            _position = position;
        }

        public override void Initialize()
        {
            _positionCached = _player.View.Position;

            _player.View.NavMeshAgent.enabled = false;

            _player.View.Position = _position;
            _player.View.Euler = new Vector3(0f, 170f, 0f);

            _player.View.Idle(_player.Model.Sex, 0);
        }

        public override void Dispose()
        {
            _player.View.Position = _positionCached;
        }
    }
}

