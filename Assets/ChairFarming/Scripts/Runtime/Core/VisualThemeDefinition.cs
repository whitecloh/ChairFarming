using UnityEngine;

namespace ChairFarming.Runtime.Core
{
    [CreateAssetMenu(menuName = "Chair Farming/Visual Theme", fileName = "VisualThemeDefinition")]
    public sealed class VisualThemeDefinition : ScriptableObject
    {
        [SerializeField] private string id = "THEME";
        [SerializeField] private Sprite boardBackgroundSprite;
        [SerializeField] private Sprite pinSprite;
        [SerializeField] private Sprite releaseButtonIcon;
        [SerializeField] private Sprite rerollButtonIcon;
        [SerializeField] private Sprite menuBackgroundSprite;
        [SerializeField] private Sprite menuCardSprite;

        public string Id => id;
        public Sprite BoardBackgroundSprite => boardBackgroundSprite;
        public Sprite PinSprite => pinSprite;
        public Sprite ReleaseButtonIcon => releaseButtonIcon;
        public Sprite RerollButtonIcon => rerollButtonIcon;
        public Sprite MenuBackgroundSprite => menuBackgroundSprite;
        public Sprite MenuCardSprite => menuCardSprite;
    }
}
