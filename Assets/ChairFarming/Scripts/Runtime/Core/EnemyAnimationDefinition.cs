using UnityEngine;

namespace ChairFarming.Runtime.Core
{
    [CreateAssetMenu(menuName = "Chair Farming/Animations/Enemy Animation Definition", fileName = "EnemyAnimationDefinition")]
    public sealed class EnemyAnimationDefinition : ScriptableObject
    {
        [SerializeField] private FrameAnimationClip idle;
        [SerializeField] private FrameAnimationClip hit;
        [SerializeField] private FrameAnimationClip death;

        public FrameAnimationClip Idle => idle;
        public FrameAnimationClip Hit => hit;
        public FrameAnimationClip Death => death;
    }
}
