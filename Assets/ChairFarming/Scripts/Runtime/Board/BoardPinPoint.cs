using UnityEngine;

namespace ChairFarming.Runtime.Board
{
    public sealed class BoardPinPoint : MonoBehaviour
    {
        [SerializeField] private int pinId;
        [SerializeField] private int rowIndex;
        [SerializeField] private int columnIndex;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public int PinId => pinId;
        public int RowIndex => rowIndex;
        public int ColumnIndex => columnIndex;
        public Vector2 WorldPosition => transform.position;
        public SpriteRenderer SpriteRenderer => spriteRenderer;

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
        }
#endif
    }
}
