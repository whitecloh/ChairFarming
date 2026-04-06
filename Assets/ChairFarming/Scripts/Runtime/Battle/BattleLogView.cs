using System.Collections.Generic;
using ChairFarming.Runtime.Battle;
using UnityEngine;

namespace ChairFarming.Runtime.UI
{
    public sealed class BattleLogView : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject root;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private BattleLogEntryView entryPrefab;

        [Header("Settings")]
        [SerializeField] private int maxTurnEntries = 3;

        [Header("Style")]
        [SerializeField] private Color oddRowColor = new Color(1f, 1f, 1f, 0.06f);
        [SerializeField] private Color evenRowColor = new Color(1f, 1f, 1f, 0.12f);

        private readonly Queue<BattleLogEntryView> _entries = new Queue<BattleLogEntryView>();
        private int _entryCounter;

        private const string TitleColor = "#F6F1D1";
        private const string NeutralColor = "#D8D8D8";
        private const string PositiveColor = "#78E08F";
        private const string NegativeColor = "#FF7675";
        private const string NotesColor = "#AFAFAF";

        public void Show()
        {
            if (root != null)
            {
                root.SetActive(true);
            }
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        public void Clear()
        {
            while (_entries.Count > 0)
            {
                BattleLogEntryView entry = _entries.Dequeue();
                if (entry != null)
                {
                    Destroy(entry.gameObject);
                }
            }

            _entryCounter = 0;
        }

        public void AddTurnResult(int turnIndex, string ballName, BallResolutionData data)
        {
            if (data == null || entryPrefab == null || contentRoot == null)
            {
                return;
            }

            BattleLogEntryView entry = Instantiate(entryPrefab, contentRoot);
            entry.SetText(BuildTurnText(turnIndex, ballName, data));
            entry.SetBackgroundColor((_entryCounter % 2) == 0 ? oddRowColor : evenRowColor);

            _entries.Enqueue(entry);
            _entryCounter++;

            while (_entries.Count > Mathf.Max(1, maxTurnEntries))
            {
                BattleLogEntryView oldest = _entries.Dequeue();
                if (oldest != null)
                {
                    Destroy(oldest.gameObject);
                }
            }

            Show();
        }

        private static string BuildTurnText(int turnIndex, string ballName, BallResolutionData data)
        {
            string safeBallName = string.IsNullOrWhiteSpace(ballName) ? "Unknown Ball" : ballName;
            string notes = string.IsNullOrWhiteSpace(data.Notes)
                ? string.Empty
                : "  <color=" + NotesColor + ">[" + data.Notes + "]</color>";

            return
                "<color=" + TitleColor + ">Turn " + turnIndex + "</color>  " +
                "<color=" + NeutralColor + ">" + safeBallName + "</color>\n" +
                "<color=" + NeutralColor + ">Hits:</color> " + data.Collisions +
                "   <color=" + NeutralColor + ">Finger:</color> #" + (data.FingerIndex + 1) + " x" + data.FingerMultiplier +
                "   <color=" + PositiveColor + ">Money " + FormatSigned(data.MoneyDelta) + "</color>" +
                "   <color=" + NegativeColor + ">Damage " + FormatSigned(data.DamageDelta) + "</color>" +
                notes;
        }

        private static string FormatSigned(int value)
        {
            return value >= 0 ? "+" + value : value.ToString();
        }
    }
}