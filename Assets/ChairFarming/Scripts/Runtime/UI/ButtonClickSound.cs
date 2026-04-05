using ChairFarming.Runtime.App;
using ChairFarming.Runtime.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ChairFarming.Runtime.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class ButtonClickSound : MonoBehaviour
    {
        [SerializeField] private AudioCueLibrary audioCueLibrary;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleClicked);
        }

        private void HandleClicked()
        {
            AudioService.Instance.PlaySfx(audioCueLibrary.ButtonClick);
        }
    }
}