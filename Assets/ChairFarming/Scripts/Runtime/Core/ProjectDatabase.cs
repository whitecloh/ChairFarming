using UnityEngine;

namespace ChairFarming.Runtime.Core
{
    [CreateAssetMenu(menuName = "Chair Farming/Project Database", fileName = "ProjectDatabase")]
    public sealed class ProjectDatabase : ScriptableObject
    {
        [SerializeField] private GameBalanceConfig gameBalanceConfig;
        [SerializeField] private BallDefinition[] balls;
        [SerializeField] private LocationDefinition[] locations;
        [SerializeField] private AudioCueLibrary audioCueLibrary;

        public GameBalanceConfig GameBalanceConfig => gameBalanceConfig;
        public BallDefinition[] Balls => balls;
        public LocationDefinition[] Locations => locations;
        
        public AudioCueLibrary AudioCueLibrary => audioCueLibrary;
    }
}
