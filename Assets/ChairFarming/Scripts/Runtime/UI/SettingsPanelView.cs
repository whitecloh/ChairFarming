using System;
using UnityEngine;
using UnityEngine.UI;

namespace ChairFarming.Runtime.UI
{
    public sealed class SettingsPanelView : MonoBehaviour
    {
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Toggle muteToggle;

        public void Bind(
            float volume,
            bool isMuted,
            Action<float> onVolumeChanged,
            Action<bool> onMuteChanged)
        {
            if (volumeSlider != null)
            {
                volumeSlider.SetValueWithoutNotify(volume);
                volumeSlider.onValueChanged.RemoveAllListeners();
                volumeSlider.onValueChanged.AddListener(value => onVolumeChanged?.Invoke(value));
            }

            if (muteToggle != null)
            {
                muteToggle.SetIsOnWithoutNotify(isMuted);
                muteToggle.onValueChanged.RemoveAllListeners();
                muteToggle.onValueChanged.AddListener(value => onMuteChanged?.Invoke(value));
            }
        }
    }
}
