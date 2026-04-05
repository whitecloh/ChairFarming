using UnityEngine;

namespace ChairFarming.Runtime.Core
{
    [CreateAssetMenu(menuName = "Chair Farming/Finger Set Definition", fileName = "FingerSetDefinition")]
    public sealed class FingerSetDefinition : ScriptableObject
    {
        [SerializeField] private FingerDefinition[] fingers = new FingerDefinition[10];

        public FingerDefinition[] Fingers => fingers;

        private void OnValidate()
        {
            if (fingers == null || fingers.Length != 10)
            {
                System.Array.Resize(ref fingers, 10);
            }
        }

        public FingerDefinition GetFinger(int index)
        {
            if (fingers == null || fingers.Length == 0)
            {
                return null;
            }

            index = Mathf.Clamp(index, 0, fingers.Length - 1);
            return fingers[index];
        }
    }
}
