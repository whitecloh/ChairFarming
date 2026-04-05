using TMPro;
using UnityEngine;

namespace ChairFarming.Runtime.UI
{
    public sealed class ScreenBlockerView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI hintText;

        public void Show(string hint)
        {
            if (root != null)
            {
                root.SetActive(true);
            }

            if (hintText != null)
            {
                hintText.text = hint;
            }
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }
    }
}
