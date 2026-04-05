using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChairFarming.Runtime.UI
{
    public sealed class LocationProgressBarView : MonoBehaviour
    {
        [SerializeField] private Transform dotRoot;
        [SerializeField] private Image dotPrefab;
        [SerializeField] private Color completedColor = Color.white;
        [SerializeField] private Color currentColor = Color.black;
        [SerializeField] private Color pendingColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private readonly List<Image> _dots = new List<Image>();

        public void Refresh(int totalLevels, int currentIndex)
        {
            EnsureDotCount(totalLevels);

            for (int i = 0; i < _dots.Count; i++)
            {
                bool active = i < totalLevels;
                _dots[i].gameObject.SetActive(active);

                if (!active)
                {
                    continue;
                }

                if (i < currentIndex)
                {
                    _dots[i].color = completedColor;
                }
                else if (i == currentIndex)
                {
                    _dots[i].color = currentColor;
                }
                else
                {
                    _dots[i].color = pendingColor;
                }
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
                Image dot = Instantiate(dotPrefab, dotRoot);
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
