using System.Collections.Generic;
using ChairFarming.Runtime.App;
using ChairFarming.Runtime.Core;
using ChairFarming.Runtime.Save;
using ChairFarming.Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ChairFarming.Runtime.Menu
{
    public sealed class MenuSceneController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private ProjectDatabase projectDatabase;
        [SerializeField] private string battleSceneName = "Battle";

        [Header("UI")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform locationContentRoot;
        [SerializeField] private LocationCardView locationCardPrefab;
        [SerializeField] private Button startButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button resetProgressButton;
        [SerializeField] private SettingsPanelView settingsPanelView;

        private RunProgressService _runProgressService;
        private SettingsService _settingsService;
        private readonly List<LocationCardView> _spawnedCards = new List<LocationCardView>();
        private int _selectedLocationIndex;

        private void Awake()
        {
            _runProgressService = new RunProgressService();
            _settingsService = new SettingsService();

            EnsureSessionExists();
            EnsureAudioServiceExists();

            BuildMenu();
            BindButtons();
            BindSettings();
            RefreshSelectedLocationVisuals();
        }

        private void BuildMenu()
        {
            if (projectDatabase == null || projectDatabase.Locations == null || projectDatabase.Locations.Length == 0)
            {
                Debug.LogError("MenuSceneController: ProjectDatabase is missing or has no locations.");
                return;
            }

            _selectedLocationIndex = GetDefaultSelectedLocationIndex();

            if (titleText != null)
            {
                titleText.text = "Chair Farming: Toe Siege";
            }

            RebuildLocationList();
        }

        private void BindButtons()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveAllListeners();
                startButton.onClick.AddListener(StartSelectedLocation);
            }

            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(ExitGame);
            }

            if (resetProgressButton != null)
            {
                resetProgressButton.onClick.RemoveAllListeners();
                resetProgressButton.onClick.AddListener(ResetProgress);
            }
        }

        private void BindSettings()
        {
            if (settingsPanelView == null)
            {
                return;
            }

            settingsPanelView.Bind(
                _settingsService.Data.MasterVolume,
                _settingsService.Data.IsMuted,
                HandleVolumeChanged,
                HandleMuteChanged);

            if (AudioService.Instance != null)
            {
                AudioService.Instance.Initialize(_settingsService);
            }
        }

        private void HandleVolumeChanged(float volume)
        {
            _settingsService.SetMasterVolume(volume);

            if (AudioService.Instance != null)
            {
                AudioService.Instance.ApplySettings(_settingsService.Data.MasterVolume, _settingsService.Data.IsMuted);
            }
        }

        private void HandleMuteChanged(bool isMuted)
        {
            _settingsService.SetMuted(isMuted);

            if (AudioService.Instance != null)
            {
                AudioService.Instance.ApplySettings(_settingsService.Data.MasterVolume, _settingsService.Data.IsMuted);
            }
        }

        private void RebuildLocationList()
        {
            for (int i = 0; i < _spawnedCards.Count; i++)
            {
                if (_spawnedCards[i] != null)
                {
                    Destroy(_spawnedCards[i].gameObject);
                }
            }

            _spawnedCards.Clear();

            if (locationCardPrefab == null || locationContentRoot == null)
            {
                return;
            }

            for (int i = 0; i < projectDatabase.Locations.Length; i++)
            {
                LocationDefinition location = projectDatabase.Locations[i];
                bool unlocked = _runProgressService.IsLocationUnlocked(i);
                bool selected = i == _selectedLocationIndex;

                LocationCardView card = Instantiate(locationCardPrefab, locationContentRoot);
                card.Bind(i, location, unlocked, selected, HandleLocationSelected);
                _spawnedCards.Add(card);
            }

            if (startButton != null)
            {
                startButton.interactable = _runProgressService.IsLocationUnlocked(_selectedLocationIndex);
            }
        }

        private void HandleLocationSelected(int locationIndex)
        {
            if (!_runProgressService.IsLocationUnlocked(locationIndex))
            {
                return;
            }

            _selectedLocationIndex = locationIndex;
            _runProgressService.SetLastSelectedLocation(locationIndex);
            RebuildLocationList();
            RefreshSelectedLocationVisuals();
        }

        private void RefreshSelectedLocationVisuals()
        {
            if (projectDatabase == null || projectDatabase.Locations == null || projectDatabase.Locations.Length == 0)
            {
                return;
            }

            LocationDefinition selectedLocation = projectDatabase.Locations[Mathf.Clamp(_selectedLocationIndex, 0, projectDatabase.Locations.Length - 1)];
            VisualThemeDefinition theme = selectedLocation.Theme;

            if (backgroundImage != null)
            {
                backgroundImage.sprite = theme != null ? theme.MenuBackgroundSprite : null;
            }

            AudioService.Instance.PlayMusic(projectDatabase.AudioCueLibrary.MenuMusic);
        }

        private int GetDefaultSelectedLocationIndex()
        {
            int candidate = Mathf.Clamp(_runProgressService.Data.LastSelectedLocation, 0, projectDatabase.Locations.Length - 1);

            if (_runProgressService.IsLocationUnlocked(candidate) &&
                !_runProgressService.IsLocationCompleted(projectDatabase.Locations[candidate].Id))
            {
                return candidate;
            }

            for (int i = 0; i <= _runProgressService.Data.HighestUnlockedLocation && i < projectDatabase.Locations.Length; i++)
            {
                if (!_runProgressService.IsLocationCompleted(projectDatabase.Locations[i].Id))
                {
                    return i;
                }
            }

            return Mathf.Clamp(_runProgressService.Data.HighestUnlockedLocation, 0, projectDatabase.Locations.Length - 1);
        }

        private void StartSelectedLocation()
        {
            GameSession session = GameSession.Instance;
            if (session == null)
            {
                Debug.LogError("MenuSceneController: GameSession is missing.");
                return;
            }

            session.SetProjectDatabase(projectDatabase);
            session.SelectLocation(_selectedLocationIndex);
            _runProgressService.SetLastSelectedLocation(_selectedLocationIndex);

            SceneManager.LoadScene(battleSceneName);
        }

        private void ResetProgress()
        {
            _runProgressService.ResetProgress();
            _selectedLocationIndex = 0;
            RebuildLocationList();
            RefreshSelectedLocationVisuals();
        }

        private static void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void EnsureSessionExists()
        {
            if (GameSession.Instance != null)
            {
                GameSession.Instance.SetProjectDatabase(projectDatabase);
                return;
            }

            GameObject sessionObject = new GameObject("GameSession");
            GameSession session = sessionObject.AddComponent<GameSession>();
            session.SetProjectDatabase(projectDatabase);
        }

        private void EnsureAudioServiceExists()
        {
            if (AudioService.Instance != null)
            {
                return;
            }

            GameObject audioObject = new GameObject("AudioService");
            audioObject.AddComponent<AudioSource>();
            AudioService audioService = audioObject.AddComponent<AudioService>();
            audioService.Initialize(_settingsService);
        }
    }
}
