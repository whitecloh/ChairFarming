using UnityEngine;

namespace ChairFarming.Runtime.Core
{
    public sealed class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }

        [SerializeField] private ProjectDatabase projectDatabase;
        [SerializeField] private int selectedLocationIndex = -1;

        public ProjectDatabase ProjectDatabase => projectDatabase;
        public int SelectedLocationIndex => selectedLocationIndex;

        public bool HasSelectedLocation =>
            projectDatabase != null &&
            projectDatabase.Locations != null &&
            selectedLocationIndex >= 0 &&
            selectedLocationIndex < projectDatabase.Locations.Length;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetProjectDatabase(ProjectDatabase database)
        {
            projectDatabase = database;
        }

        public void SelectLocation(int locationIndex)
        {
            selectedLocationIndex = locationIndex;
        }

        public LocationDefinition GetSelectedLocation()
        {
            if (!HasSelectedLocation)
            {
                return null;
            }

            return projectDatabase.Locations[selectedLocationIndex];
        }
    }
}
