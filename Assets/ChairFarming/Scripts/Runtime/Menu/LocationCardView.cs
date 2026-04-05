using System;
using ChairFarming.Runtime.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChairFarming.Runtime.Menu
{
    public sealed class LocationCardView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image artImage;
        [SerializeField] private GameObject selectedState;
        [SerializeField] private GameObject lockedState;
        [SerializeField] private TextMeshProUGUI titleText;

        private int _locationIndex;

        public void Bind(int locationIndex, LocationDefinition location, bool unlocked, bool selected, Action<int> onClicked)
        {
            _locationIndex = locationIndex;

            if (artImage != null)
            {
                artImage.sprite = location != null && location.Theme != null
                    ? location.Theme.MenuCardSprite
                    : null;
            }

            if (titleText != null)
            {
                titleText.text = location != null ? location.DisplayName : "Location";
            }

            if (button != null)
            {
                button.interactable = unlocked;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClicked?.Invoke(_locationIndex));
            }

            if (selectedState != null)
            {
                selectedState.SetActive(selected);
            }

            if (lockedState != null)
            {
                lockedState.SetActive(!unlocked);
            }
        }
    }
}
