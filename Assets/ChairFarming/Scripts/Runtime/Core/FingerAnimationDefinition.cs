using UnityEngine;

namespace ChairFarming.Runtime.Core
{
    [CreateAssetMenu(menuName = "Chair Farming/Animations/Finger Animation Definition", fileName = "FingerAnimationDefinition")]
    public sealed class FingerAnimationDefinition : ScriptableObject
    {
        [SerializeField] private FrameAnimationClip idle;
        [SerializeField] private FrameAnimationClip hit;
        [SerializeField] private FrameAnimationClip highlight;

        public FrameAnimationClip Idle => idle;
        public FrameAnimationClip Hit => hit;
        public FrameAnimationClip Highlight => highlight;
    }
}
