using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ChairFarming.Runtime.Board
{
    public sealed class BoardViewportInput : MonoBehaviour, IPointerMoveHandler, IDragHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform inputRect;

        private Action<float> _onAimChanged;
        private Action<float> _onLaunchRequested;
        private bool _interactable;

        public void BindCallbacks(Action<float> onAimChanged, Action<float> onLaunchRequested)
        {
            _onAimChanged = onAimChanged;
            _onLaunchRequested = onLaunchRequested;
        }

        public void SetInteractable(bool interactable)
        {
            _interactable = interactable;
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!_interactable)
            {
                return;
            }

            float normalizedX;
            if (TryGetNormalizedX(eventData, out normalizedX))
            {
                _onAimChanged?.Invoke(normalizedX);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_interactable)
            {
                return;
            }

            float normalizedX;
            if (TryGetNormalizedX(eventData, out normalizedX))
            {
                _onAimChanged?.Invoke(normalizedX);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_interactable)
            {
                return;
            }

            float normalizedX;
            if (TryGetNormalizedX(eventData, out normalizedX))
            {
                _onAimChanged?.Invoke(normalizedX);
                _onLaunchRequested?.Invoke(normalizedX);
            }
        }

        private bool TryGetNormalizedX(PointerEventData eventData, out float normalizedX)
        {
            normalizedX = 0.5f;

            if (inputRect == null)
            {
                inputRect = transform as RectTransform;
            }

            if (inputRect == null)
            {
                return false;
            }

            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    inputRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out localPoint))
            {
                return false;
            }

            Rect rect = inputRect.rect;
            if (rect.width <= 0.001f)
            {
                return false;
            }

            normalizedX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
            normalizedX = Mathf.Clamp01(normalizedX);
            return true;
        }
    }
}
