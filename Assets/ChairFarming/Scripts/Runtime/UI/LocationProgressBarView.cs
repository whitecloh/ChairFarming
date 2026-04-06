using System.Collections.Generic;
using UnityEngine;

namespace ChairFarming.Runtime.UI
{
    public sealed class LocationProgressBarView : MonoBehaviour
    {
        [SerializeField] private Transform dotRoot;
        [SerializeField] private LocationProgressBarViewItem dotPrefab;
        [SerializeField] private Sprite completedColor;
        [SerializeField] private Sprite currentColor;
        [SerializeField] private Sprite pendingColor;

        private readonly List<LocationProgressBarViewItem> _dots = new List<LocationProgressBarViewItem>();

        public void Refresh(int totalLevels, int currentIndex)
        {
            EnsureDotCount(totalLevels);

            for (int i = 0; i < _dots.Count; i++)
            {
                bool active = i < totalLevels;
                _dots[i].gameObject.SetActive(active);

                Sprite icon;

                if (!active)
                {
                    continue;
                }

                if (i < currentIndex)
                {
                    icon = completedColor;
                }
                else if (i == currentIndex)
                {
                    icon = currentColor;
                }
                else
                {
                    icon = pendingColor;
                }
                
                _dots[i].Init(icon, i + 1);
            }
        }

        private void EnsureDotCount(int totalLevels)
        {
            if (dotPrefab == null || dotRoot == null)
            {
                return;
            }

            while (_dots.Count < totalLevels)
            {
                LocationProgressBarViewItem dot = Instantiate(dotPrefab, dotRoot);
                _dots.Add(dot);
            }

            for (int i = _dots.Count - 1; i >= totalLevels; i--)
            {
                if (_dots[i] != null)
                {
                    Destroy(_dots[i].gameObject);
                }

                _dots.RemoveAt(i);
            }
        }
    }
}
