using System;

namespace ChairFarming.Runtime.Save
{
    [Serializable]
    public sealed class AudioSettingsData
    {
        public float MasterVolume = 0.5f;
        public bool IsMuted = false;
    }
}
