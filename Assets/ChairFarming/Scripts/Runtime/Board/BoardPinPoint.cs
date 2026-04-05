using UnityEngine;

namespace ChairFarming.Runtime.Board
{
    public sealed class BoardPinPoint : MonoBehaviour
    {
        [SerializeField] private int pinId;
        [SerializeField] private int rowIndex;
        [SerializeField] private int columnIndex;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private CircleCollider2D circleCollider2D;
        [SerializeField] private float radiusOverride = -1f;

        public int PinId => pinId;
        public int RowIndex => rowIndex;
        public int ColumnIndex => columnIndex;
        public Vector2 WorldPosition => transform.position;
        public SpriteRenderer SpriteRenderer => spriteRenderer;

        public float WorldRadius
        {
            get
            {
                if (radiusOverride > 0f)
                {
                    return radiusOverride;
                }

                if (circleCollider2D != null)
                {
                    Vector3 lossyScale = transform.lossyScale;
                    float maxScale = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y));
                    return circleCollider2D.radius * maxScale;
                }

                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    Bounds bounds = spriteRenderer.bounds;
                    return Mathf.Min(bounds.extents.x, bounds.extents.y);
                }

                return 0.12f;
            }
        }

        public void SetIds(int newPinId, int newRowIndex, int newColumnIndex)
        {
            pinId = newPinId;
            rowIndex = newRowIndex;
            columnIndex = newColumnIndex;
        }

        public void ApplySprite(Sprite sprite)
        {
            if (spriteRenderer != null && sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (circleCollider2D == null)
            {
                circleCollider2D = GetComponent<CircleCollider2D>();
            }
        }
#endif
    }
}