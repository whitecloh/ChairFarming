using System.IO;
using ChairFarming.Runtime.Core;
using UnityEngine;

namespace ChairFarming.Runtime.Save
{
    public sealed class RunProgressService
    {
        private const string FileName = "chair_farming_progress.json";

        private readonly string _savePath;
        private RunSaveData _data;

        public RunProgressService()
        {
            _savePath = Path.Combine(Application.persistentDataPath, FileName);
            _data = LoadInternal();
        }

        public RunSaveData Data => _data;

        public bool IsLocationUnlocked(int locationIndex)
        {
            return locationIndex <= _data.HighestUnlockedLocation;
        }

        public void UnlockLocation(int locationIndex)
        {
            if (locationIndex > _data.HighestUnlockedLocation)
            {
                _data.HighestUnlockedLocation = locationIndex;
                Save();
            }
        }

        public void SetLastSelectedLocation(int locationIndex)
        {
            _data.LastSelectedLocation = Mathf.Max(0, locationIndex);
            Save();
        }

        public void MarkLocationCompleted(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                return;
            }

            if (!_data.CompletedLocationIds.Contains(locationId))
            {
                _data.CompletedLocationIds.Add(locationId);
                Save();
            }
        }

        public bool IsLocationCompleted(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                return false;
            }

            return _data.CompletedLocationIds.Contains(locationId);
        }

        public void ResetProgress()
        {
            _data = new RunSaveData();
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
                Debug.LogError("RunProgressService save failed: " + ioException.Message);
            }
        }

        private RunSaveData LoadInternal()
        {
            if (!File.Exists(_savePath))
            {
                return new RunSaveData();
            }

            try
            {
                string json = File.ReadAllText(_savePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new RunSaveData();
                }

                return JsonUtility.FromJson<RunSaveData>(json) ?? new RunSaveData();
            }
            catch
            {
                return new RunSaveData();
            }
        }
    }
}
