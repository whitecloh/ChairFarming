using ChairFarming.Runtime.Animation;
using ChairFarming.Runtime.Core;
using UnityEngine;

namespace ChairFarming.Runtime.Board
{
    public sealed class FingerSlotView : MonoBehaviour
    {
        [SerializeField] private int fingerIndex;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private Transform entryPoint;
        [SerializeField] private Transform landingPoint;
        [SerializeField] private FingerAnimator fingerAnimator;

        private FingerDefinition _definition;

        public int FingerIndex => fingerIndex;
        public Transform EntryPoint => entryPoint;
        public Transform LandingPoint => landingPoint;
        public FingerDefinition Definition => _definition;

        public void ApplyDefinition(int index, FingerDefinition definition)
        {
            fingerIndex = index;
            _definition = definition;

            if (bodyRenderer != null)
            {
                bodyRenderer.sprite = definition != null ? definition.Sprite : null;
            }

            if (fingerAnimator != null)
            {
                fingerAnimator.ApplyDefinition(definition != null ? definition.AnimationDefinition : null, bodyRenderer);
                fingerAnimator.PlayIdle();
            }
        }

        public FingerRuntimeInfo GetRuntimeInfo()
        {
            if (_definition == null)
            {
                return new FingerRuntimeInfo
                {
                    Index = fingerIndex,
                    ToeType = ToeType.Normal,
                    FootSide = FootSide.None,
                    Multiplier = 1,
                    LandingWeight = 1f,
                };
            }

            return new FingerRuntimeInfo
            {
                Index = fingerIndex,
                ToeType = _definition.ToeType,
                FootSide = _definition.FootSide,
                Multiplier = _definition.Multiplier,
                LandingWeight = _definition.LandingWeight,
            };
        }

        public void PlayIdle()
        {
            if (fingerAnimator != null)
            {
                fingerAnimator.PlayIdle();
            }
        }

        public void PlayHit()
        {
            if (fingerAnimator != null)
            {
                fingerAnimator.PlayHit();
            }
        }

        public void PlayHighlight()
        {
            if (fingerAnimator != null)
            {
                fingerAnimator.PlayHighlight();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (bodyRenderer == null)
            {
                bodyRenderer = GetComponent<SpriteRenderer>();
            }
        }
#endif
    }
}
