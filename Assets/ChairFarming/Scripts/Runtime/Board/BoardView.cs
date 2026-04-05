using System.Collections.Generic;
using System.Linq;
using ChairFarming.Runtime.Core;
using UnityEngine;

namespace ChairFarming.Runtime.Board
{
    public sealed class BoardView : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer backgroundRenderer;

        [Header("Roots")]
        [SerializeField] private Transform pinsRoot;
        [SerializeField] private Transform fingersRoot;
        [SerializeField] private Transform ballRoot;

        [Header("Launch Anchors")]
        [SerializeField] private Transform previewSpawnAnchor;
        [SerializeField] private Transform launchLeftAnchor;
        [SerializeField] private Transform launchRightAnchor;

        [Header("Prefabs")]
        [SerializeField] private BallActor ballPrefab;

        [Header("Layout Cache")]
        [SerializeField] private BoardPinPoint[] pins;
        [SerializeField] private FingerSlotView[] fingerSlots;
        [SerializeField] private float rowGroupingTolerance = 0.18f;

        private readonly List<List<BoardPinPoint>> _rows = new List<List<BoardPinPoint>>();
        private readonly Dictionary<int, BoardPinPoint> _pinById = new Dictionary<int, BoardPinPoint>();

        private BallActor _aimBall;

        public Transform PreviewSpawnAnchor => previewSpawnAnchor;
        public IReadOnlyList<List<BoardPinPoint>> Rows => _rows;
        public IReadOnlyList<FingerSlotView> FingerSlots => fingerSlots;

        private void Awake()
        {
            InitializeRuntimeLayout();
        }

        public void InitializeRuntimeLayout()
        {
            CollectPins();
            CollectFingers();
            BuildRowsAndAssignIds();
            BuildPinCache();
        }

        public void ApplyLocation(LocationDefinition location)
        {
            if (location == null)
            {
                return;
            }

            if (backgroundRenderer != null && location.Theme != null && location.Theme.BoardBackgroundSprite != null)
            {
                backgroundRenderer.sprite = location.Theme.BoardBackgroundSprite;
            }

            Sprite pinSprite = location.Theme != null ? location.Theme.PinSprite : null;
            if (pinSprite != null && pins != null)
            {
                for (int i = 0; i < pins.Length; i++)
                {
                    if (pins[i] != null)
                    {
                        pins[i].ApplySprite(pinSprite);
                    }
                }
            }
        }

        public void ApplyEnemyFingerSet(FingerSetDefinition fingerSet)
        {
            if (fingerSlots == null)
            {
                return;
            }

            for (int i = 0; i < fingerSlots.Length; i++)
            {
                FingerDefinition definition = fingerSet != null ? fingerSet.GetFinger(i) : null;
                fingerSlots[i].ApplyDefinition(i, definition);
            }
        }

        public FingerRuntimeInfo GetFingerRuntimeInfo(int fingerIndex)
        {
            if (fingerSlots == null || fingerSlots.Length == 0)
            {
                return new FingerRuntimeInfo
                {
                    Index = 0,
                    ToeType = ToeType.Normal,
                    FootSide = FootSide.None,
                    Multiplier = 1,
                    LandingWeight = 1f,
                };
            }

            fingerIndex = Mathf.Clamp(fingerIndex, 0, fingerSlots.Length - 1);
            return fingerSlots[fingerIndex].GetRuntimeInfo();
        }

        public FingerSlotView GetFingerSlot(int fingerIndex)
        {
            if (fingerSlots == null || fingerSlots.Length == 0)
            {
                return null;
            }

            fingerIndex = Mathf.Clamp(fingerIndex, 0, fingerSlots.Length - 1);
            return fingerSlots[fingerIndex];
        }

        public BoardPinPoint GetPinById(int pinId)
        {
            BoardPinPoint pin;
            return _pinById.TryGetValue(pinId, out pin) ? pin : null;
        }

        public void ResetFingerPresentation()
        {
            if (fingerSlots == null)
            {
                return;
            }

            for (int i = 0; i < fingerSlots.Length; i++)
            {
                if (fingerSlots[i] != null)
                {
                    fingerSlots[i].PlayIdle();
                }
            }
        }

        public void HighlightTargetFinger(int fingerIndex)
        {
            if (fingerSlots == null)
            {
                return;
            }

            for (int i = 0; i < fingerSlots.Length; i++)
            {
                if (fingerSlots[i] == null)
                {
                    continue;
                }

                if (i == fingerIndex)
                {
                    fingerSlots[i].PlayHighlight();
                }
                else
                {
                    fingerSlots[i].PlayIdle();
                }
            }
        }

        public void ShowAimingBall(BallDefinition ball, float normalizedX, GameBalanceConfig balanceConfig)
        {
            EnsureAimBall();

            if (_aimBall == null)
            {
                return;
            }

            _aimBall.Initialize(ball, balanceConfig);
            _aimBall.gameObject.SetActive(true);
            UpdateAimPosition(normalizedX);
        }

        public void UpdateAimPosition(float normalizedX)
        {
            if (_aimBall == null)
            {
                return;
            }

            _aimBall.SetPosition(GetLaunchWorldPosition(normalizedX));
        }

        public void HideAimBall()
        {
            if (_aimBall != null)
            {
                Destroy(_aimBall.gameObject);
                _aimBall = null;
            }
        }

        public void PlayDrop(
            DropPlanData plan,
            float segmentBaseDuration,
            System.Action<int> onPinImpact,
            System.Action<int> onFingerLand,
            System.Action<int, int> onCompleted)
        {
            if (_aimBall == null)
            {
                Debug.LogError("BoardView: no aiming ball was created before PlayDrop.");
                return;
            }

            _aimBall.PlayDrop(
                plan,
                segmentBaseDuration,
                onPinImpact,
                onFingerLand,
                onCompleted);
        }

        public Vector3 GetLaunchWorldPosition(float normalizedX)
        {
            normalizedX = Mathf.Clamp01(normalizedX);

            if (launchLeftAnchor == null || launchRightAnchor == null)
            {
                return previewSpawnAnchor != null ? previewSpawnAnchor.position : transform.position;
            }

            return Vector3.Lerp(launchLeftAnchor.position, launchRightAnchor.position, normalizedX);
        }

        private void CollectPins()
        {
            if (pinsRoot != null)
            {
                pins = pinsRoot.GetComponentsInChildren<BoardPinPoint>(true);
            }
        }

        private void CollectFingers()
        {
            if (fingersRoot != null)
            {
                fingerSlots = fingersRoot
                    .GetComponentsInChildren<FingerSlotView>(true)
                    .OrderBy(slot => slot.transform.position.x)
                    .ToArray();
            }
        }

        private void BuildRowsAndAssignIds()
        {
            _rows.Clear();

            if (pins == null || pins.Length == 0)
            {
                return;
            }

            List<BoardPinPoint> orderedPins = pins
                .Where(pin => pin != null)
                .OrderByDescending(pin => pin.transform.localPosition.y)
                .ThenBy(pin => pin.transform.localPosition.x)
                .ToList();

            float currentRowY = float.NaN;
            int currentRowIndex = -1;
            int nextPinId = 0;

            for (int i = 0; i < orderedPins.Count; i++)
            {
                BoardPinPoint pin = orderedPins[i];
                float localY = pin.transform.localPosition.y;

                if (float.IsNaN(currentRowY) || Mathf.Abs(currentRowY - localY) > rowGroupingTolerance)
                {
                    currentRowY = localY;
                    currentRowIndex++;
                    _rows.Add(new List<BoardPinPoint>());
                }

                _rows[currentRowIndex].Add(pin);
            }

            for (int rowIndex = 0; rowIndex < _rows.Count; rowIndex++)
            {
                _rows[rowIndex] = _rows[rowIndex]
                    .OrderBy(pin => pin.transform.localPosition.x)
                    .ToList();

                for (int columnIndex = 0; columnIndex < _rows[rowIndex].Count; columnIndex++)
                {
                    _rows[rowIndex][columnIndex].SetIds(nextPinId, rowIndex, columnIndex);
                    nextPinId++;
                }
            }

            pins = _rows.SelectMany(row => row).ToArray();
        }

        private void BuildPinCache()
        {
            _pinById.Clear();

            if (pins == null)
            {
                return;
            }

            for (int i = 0; i < pins.Length; i++)
            {
                if (pins[i] == null)
                {
                    continue;
                }

                int id = pins[i].PinId;
                if (_pinById.ContainsKey(id))
                {
                    Debug.LogError("BoardView: duplicate pin id detected: " + id + " on " + pins[i].name);
                    continue;
                }

                _pinById.Add(id, pins[i]);
            }
        }

        private void EnsureAimBall()
        {
            if (_aimBall != null)
            {
                return;
            }

            if (ballPrefab == null)
            {
                Debug.LogError("BoardView: Ball prefab is missing.");
                return;
            }

            Transform parent = ballRoot != null ? ballRoot : transform;
            _aimBall = Instantiate(ballPrefab, parent);
            _aimBall.gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                CollectPins();
                CollectFingers();
            }
        }
#endif
    }
}