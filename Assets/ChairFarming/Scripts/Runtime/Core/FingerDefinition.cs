using UnityEngine;

namespace ChairFarming.Runtime.Core
{
    [CreateAssetMenu(menuName = "Chair Farming/Finger Definition", fileName = "FingerDefinition")]
    public sealed class FingerDefinition : ScriptableObject
    {
        [SerializeField] private string id = "FINGER";
        [SerializeField] private string displayName = "Finger";
        [SerializeField] private ToeType toeType = ToeType.Normal;
        [SerializeField] private FootSide footSide = FootSide.Left;
        [SerializeField] private int multiplier = 1;
        [SerializeField] private float landingWeight = 1f;
        [SerializeField] private Sprite sprite;
        [SerializeField] private FingerAnimationDefinition animationDefinition;

        public string Id => id;
        public string DisplayName => displayName;
        public ToeType ToeType => toeType;
        public FootSide FootSide => footSide;
        public int Multiplier => multiplier;
        public float LandingWeight => landingWeight;
        public Sprite Sprite => sprite;
        public FingerAnimationDefinition AnimationDefinition => animationDefinition;

        private void OnValidate()
        {
            multiplier = Mathf.Max(1, multiplier);
            landingWeight = Mathf.Max(0.05f, landingWeight);
        }
    }
}
