using System;
using System.Collections.Generic;

namespace ChairFarming.Runtime.Save
{
    [Serializable]
    public sealed class RunSaveData
    {
        public int HighestUnlockedLocation = 0;
        public int LastSelectedLocation = 0;
        public List<string> CompletedLocationIds = new List<string>();
    }
}
