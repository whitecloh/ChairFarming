using System;
using UnityEngine;

namespace ChairFarming.Runtime.Core
{
    [Serializable]
    public sealed class FrameAnimationClip
    {
        [SerializeField] private string id = "idle";
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float fps = 8f;
        [SerializeField] private bool loop = true;

        public string Id => id;
        public Sprite[] Frames => frames;
        public float Fps => fps;
        public bool Loop => loop;
        public float FrameDuration => fps > 0f ? 1f / fps : 0.125f;
    }
}
