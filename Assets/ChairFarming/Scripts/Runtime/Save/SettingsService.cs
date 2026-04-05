using System.IO;
using UnityEngine;

namespace ChairFarming.Runtime.Save
{
    public sealed class SettingsService
    {
        private const string FileName = "chair_farming_settings.json";

        private readonly string _savePath;
        private AudioSettingsData _data;

        public SettingsService()
        {
            _savePath = Path.Combine(Application.persistentDataPath, FileName);
            _data = LoadInternal();
        }

        public AudioSettingsData Data => _data;

        public void SetMasterVolume(float volume)
        {
            _data.MasterVolume = Mathf.Clamp01(volume);
            Save();
        }

        public void SetMuted(bool isMuted)
        {
            _data.IsMuted = isMuted;
            Save();
        }

        public void ResetToDefaults()
        {
            _data = new AudioSettingsData();
            Save();
        }

        public void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(_data, true);
                File.WriteAllText(_savePath, json);
            }
            catch (IOException ioException)
            {
                Debug.LogError("SettingsService save failed: " + ioException.Message);
            }
        }

        private AudioSettingsData LoadInternal()
        {
            if (!File.Exists(_savePath))
            {
                return new AudioSettingsData();
            }

            try
            {
                string json = File.ReadAllText(_savePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new AudioSettingsData();
                }

                return JsonUtility.FromJson<AudioSettingsData>(json) ?? new AudioSettingsData();
            }
            catch
            {
                return new AudioSettingsData();
            }
        }
    }
}
