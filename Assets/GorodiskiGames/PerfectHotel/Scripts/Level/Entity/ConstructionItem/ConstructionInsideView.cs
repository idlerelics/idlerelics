using UnityEngine;

namespace Game.Level.Place
{
    public class ConstructionInsideView : ConstructionItemView
    {
        [SerializeField] private Sprite[] _icons;
        public Sprite[] Icons => _icons;
    }

}

