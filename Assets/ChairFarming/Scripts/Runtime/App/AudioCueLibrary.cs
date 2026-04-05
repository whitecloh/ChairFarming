using UnityEngine;

namespace ChairFarming.Runtime.Core
{
    [CreateAssetMenu(menuName = "Chair Farming/Audio Cue Library", fileName = "AudioCueLibrary")]
    public sealed class AudioCueLibrary : ScriptableObject
    {
        [Header("UI")]
        [SerializeField] private AudioClip buttonClick;
        [SerializeField] private AudioClip purchase;
        [SerializeField] private AudioClip reroll;

        [Header("Gameplay")]
        [SerializeField] private AudioClip gatePass;
        [SerializeField] private AudioClip fingerLand;
        [SerializeField] private AudioClip moneyGain;
        [SerializeField] private AudioClip enemyHit;
        [SerializeField] private AudioClip enemyDeath;
        [SerializeField] private AudioClip defeat;
        
        [Header("Main")]
        [SerializeField] private AudioClip menuMusic;

        public AudioClip ButtonClick => buttonClick;
        public AudioClip Purchase => purchase;
        public AudioClip Reroll => reroll;
        public AudioClip GatePass => gatePass;
        public AudioClip FingerLand => fingerLand;
        public AudioClip MoneyGain => moneyGain;
        public AudioClip EnemyHit => enemyHit;
        public AudioClip EnemyDeath => enemyDeath;
        public AudioClip Defeat => defeat;
        
        public AudioClip MenuMusic => menuMusic;
    }
}