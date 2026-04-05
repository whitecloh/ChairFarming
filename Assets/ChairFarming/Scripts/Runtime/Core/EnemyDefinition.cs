using UnityEngine;

namespace ChairFarming.Runtime.Core
{
    [CreateAssetMenu(menuName = "Chair Farming/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string id = "ENEMY";
        [SerializeField] private string displayName = "Enemy";
        [SerializeField] private int maxHp = 100;
        [SerializeField] private Sprite icon;
        [SerializeField] private Sprite bodySprite;
        [SerializeField] private EnemyAnimationDefinition animationDefinition;
        [SerializeField] private FingerSetDefinition fingerSet;

        public string Id => id;
        public string DisplayName => displayName;
        public int MaxHp => maxHp;
        public Sprite Icon => icon;
        public Sprite BodySprite => bodySprite;
        public EnemyAnimationDefinition AnimationDefinition => animationDefinition;
        public FingerSetDefinition FingerSet => fingerSet;

        private void OnValidate()
        {
            maxHp = Mathf.Max(1, maxHp);
        }
    }
}
