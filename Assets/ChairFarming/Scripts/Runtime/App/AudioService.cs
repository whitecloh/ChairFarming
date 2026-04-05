using ChairFarming.Runtime.Save;
using UnityEngine;

namespace ChairFarming.Runtime.App
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioService : MonoBehaviour
    {
        public static AudioService Instance { get; private set; }

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

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

            if (sfxSource == null)
            {
                GameObject sfxObject = new GameObject("SfxSource");
                sfxObject.transform.SetParent(transform, false);
                sfxSource = sfxObject.AddComponent<AudioSource>();
            }

            musicSource.playOnAwake = false;
            musicSource.loop = true;
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
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
            float clamped = Mathf.Clamp01(volume);

            if (musicSource != null)
            {
                musicSource.mute = isMuted;
                musicSource.volume = clamped;
            }

            if (sfxSource != null)
            {
                sfxSource.mute = isMuted;
                sfxSource.volume = clamped;
            }
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

        public void PlaySfx(AudioClip clip, float volumeScale = 1f)
        {
            if (sfxSource == null || clip == null)
            {
                return;
            }

            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
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