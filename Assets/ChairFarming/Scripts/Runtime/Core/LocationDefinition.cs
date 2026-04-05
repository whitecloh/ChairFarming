using UnityEngine;

namespace ChairFarming.Runtime.Core
{
    [CreateAssetMenu(menuName = "Chair Farming/Location Definition", fileName = "LocationDefinition")]
    public sealed class LocationDefinition : ScriptableObject
    {
        [SerializeField] private string id = "LOCATION";
        [SerializeField] private string displayName = "Location";
        [SerializeField] private VisualThemeDefinition theme;
        [SerializeField] private int startMoney = 10;
        [SerializeField] private EnemyDefinition[] enemies;

        public string Id => id;
        public string DisplayName => displayName;
        public VisualThemeDefinition Theme => theme;
        public int StartMoney => startMoney;
        public EnemyDefinition[] Enemies => enemies;

        private void OnValidate()
        {
            startMoney = Mathf.Max(0, startMoney);
        }
    }
}
