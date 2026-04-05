using ChairFarming.Runtime.Save;
using UnityEngine;

namespace ChairFarming.Runtime.App
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioService : MonoBehaviour
    {
        public static AudioService Instance { get; private set; }

        [SerializeField] private AudioSource musicSource;

        private SettingsService _settingsService;
        private AudioClip _currentMusic;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null)
            {
                musicSource = GetComponent<AudioSource>();
            }

            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }

        public void Initialize(SettingsService settingsService)
        {
            _settingsService = settingsService;
            ApplySettings(
                _settingsService != null ? _settingsService.Data.MasterVolume : 0.5f,
                _settingsService != null && _settingsService.Data.IsMuted);
        }

        public void ApplySettings(float volume, bool isMuted)
        {
            if (musicSource == null)
            {
                return;
            }

            musicSource.mute = isMuted;
            musicSource.volume = Mathf.Clamp01(volume);
        }

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null)
            {
                return;
            }

            if (_currentMusic == clip && musicSource.isPlaying)
            {
                return;
            }

            _currentMusic = clip;
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource == null)
            {
                return;
            }

            musicSource.Stop();
            _currentMusic = null;
        }
    }
}
