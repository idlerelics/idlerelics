using System;
using System.Collections;
using Game.UI.Hud;
using Game.UI.Pool;
using UnityEngine;

namespace Game.Modules.UISpritesModule
{
    public class UISpritesModuleView : MonoBehaviour
    {
        public event Action ON_ONE_FINISHED;
        public event Action<int> ON_ALL_FINISHED;

        private const float _spawnDelay = 0.1f;
        private const float _spriteRadius = 325f;

        [SerializeField] private RectTransform _progressSprite;
        [SerializeField] private ComponentPoolFactory _progressSpritePool;

        private int _displayed;
        private int _count;

        public void ShowParticles(int count, Vector3 startPosition)
        {
            StartCoroutine(CoroutineShowParticles(count, startPosition));
        }

        private IEnumerator CoroutineShowParticles(int count, Vector3 startPosition)
        {
            _count = count;
            _displayed = 0;

            var wait = new WaitForSeconds(_spawnDelay);

            for (int i = 0; i < count; i++)
            {
                var sprite = _progressSpritePool.Get<SpriteView>();

                sprite.Transform.position = startPosition;
                sprite.Transform.localScale = Vector3.zero;

                Vector3 endIntroPosition = startPosition + UnityEngine.Random.insideUnitSphere * _spriteRadius;
                Vector3 endPosition = _progressSprite.position;

                sprite.DoIntroAnimation(endIntroPosition, endPosition);
                sprite.ON_MOVE_COMPLETE += OnMoveComplete;

                yield return wait;
            }
        }

        private void OnMoveComplete(SpriteView sprite)
        {
            sprite.ON_MOVE_COMPLETE -= OnMoveComplete;

            ON_ONE_FINISHED?.Invoke();

            _progressSpritePool.Release(sprite);

            _displayed++;

            if (_displayed == _count)
                ON_ALL_FINISHED?.Invoke(_count);
        }
    }
}

