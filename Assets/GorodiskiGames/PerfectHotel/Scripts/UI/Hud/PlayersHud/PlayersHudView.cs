using Game.Level.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class PlayersHudView : BaseHudWithModel<PlayerModel>
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private RawImage _rawImage;
        [SerializeField] private ScrollView _playersScroll;
        [SerializeField] private ScrollView _attributesScroll;
        [SerializeField] private Button _selectButton;
        [SerializeField] private GameObject _messageHolder;
        [SerializeField] private TMP_Text _messageText;

        public Button CloseButton => _closeButton;
        public RawImage RawImage => _rawImage;
        public ScrollView PlayersScroll => _playersScroll;
        public ScrollView AttributesScroll => _attributesScroll;
        public Button SelectButton => _selectButton;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }

        protected override void OnModelChanged(PlayerModel model)
        {
            var isUnlocked = model.UnlockModel.IsUnlocked;
            _messageHolder.SetActive(!isUnlocked);

            _messageText.text = model.UnlockModel.Message;
        }
    }
}

