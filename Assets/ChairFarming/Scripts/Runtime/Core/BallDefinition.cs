using UnityEngine;

namespace ChairFarming.Runtime.Core
{
    [CreateAssetMenu(menuName = "Chair Farming/Ball Definition", fileName = "BallDefinition")]
    public sealed class BallDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id = "BALL";
        [SerializeField] private string displayName = "Ball";
        [SerializeField] private string shortEffectText = "+1 per hit";
        [SerializeField] [TextArea(2, 5)] private string description = "Ball description";
        [SerializeField] private Sprite icon;
        [SerializeField] private Sprite flyingSprite;

        [Header("Base")]
        [SerializeField] private BallCategory category = BallCategory.Money;
        [SerializeField] private int cost = 0;
        [SerializeField] private int weight = 1;
        [SerializeField] private int valuePerHit = 1;
        [SerializeField] private bool usesFingerMultiplier = true;
        [SerializeField] private bool enabledInOfferPool = true;

        [Header("Controlled Drop")]
        [SerializeField] private float[] bucketBias = new float[10];

        [Header("Proc")]
        [SerializeField] [Range(0f, 1f)] private float procChance = 0f;
        [SerializeField] private int procMultiplier = 2;

        [Header("Target Bonuses")]
        [SerializeField] private ToeType toeBonusType = ToeType.Normal;
        [SerializeField] private int toeBonusMultiplier = 1;
        [SerializeField] private FootSide footBonusSide = FootSide.None;
        [SerializeField] private int footBonusMultiplier = 1;

        [Header("Special")]
        [SerializeField] private BallSpecialMode specialMode = BallSpecialMode.None;
        [SerializeField] private int specialIntValue = 0;

        public string Id => id;
        public string DisplayName => displayName;
        public string ShortEffectText => shortEffectText;
        public string Description => description;
        public Sprite Icon => icon;
        public Sprite FlyingSprite => flyingSprite;
        public BallCategory Category => category;
        public int Cost => cost;
        public int Weight => weight;
        public int ValuePerHit => valuePerHit;
        public bool UsesFingerMultiplier => usesFingerMultiplier;
        public bool EnabledInOfferPool => enabledInOfferPool;
        public float[] BucketBias => bucketBias;
        public float ProcChance => procChance;
        public int ProcMultiplier => procMultiplier;
        public ToeType ToeBonusType => toeBonusType;
        public int ToeBonusMultiplier => toeBonusMultiplier;
        public FootSide FootBonusSide => footBonusSide;
        public int FootBonusMultiplier => footBonusMultiplier;
        public BallSpecialMode SpecialMode => specialMode;
        public int SpecialIntValue => specialIntValue;

        private void OnValidate()
        {
            cost = Mathf.Max(0, cost);
            weight = Mathf.Max(1, weight);
            valuePerHit = Mathf.Max(0, valuePerHit);
            procMultiplier = Mathf.Max(1, procMultiplier);
            toeBonusMultiplier = Mathf.Max(1, toeBonusMultiplier);
            footBonusMultiplier = Mathf.Max(1, footBonusMultiplier);

            if (bucketBias == null || bucketBias.Length != 10)
            {
                bucketBias = new float[10];
            }

            for (int i = 0; i < bucketBias.Length; i++)
            {
                if (bucketBias[i] <= 0f)
                {
                    bucketBias[i] = 1f;
                }
            }
        }
    }
}
