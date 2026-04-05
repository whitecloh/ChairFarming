using System.Collections;
using ChairFarming.Runtime.App;
using ChairFarming.Runtime.Board;
using ChairFarming.Runtime.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChairFarming.Runtime.Battle
{
    public sealed class BattleFlowController : MonoBehaviour
    {
        private BattleContext _context;
        private BattleRuntimeState _state;

        private EconomyService _economyService;
        private OfferGenerator _offerGenerator;
        private ResolutionService _resolutionService;
        private DropPlannerService _dropPlannerService;

        private System.Random _random;
        private Coroutine _advanceRoutine;
        private int _flightGatePassCount;

        public void Initialize(BattleContext context)
        {
            _context = context;
            _random = new System.Random();

            _state = new BattleRuntimeState
            {
                CurrentLocationIndex = GameSession.Instance != null ? GameSession.Instance.SelectedLocationIndex : 0,
                CurrentEnemyIndex = 0,
                Phase = BattlePhase.Preparing,
            };

            _economyService = new EconomyService(_context.BalanceConfig);
            _offerGenerator = new OfferGenerator();
            _resolutionService = new ResolutionService(_context.BalanceConfig);
            _dropPlannerService = new DropPlannerService(_context.BalanceConfig);

            BindViews();
            ResetTransientUi();
            StartLocation();
        }

        private void BindViews()
        {
            _context.OfferPanelView.BindCallbacks(
                HandleBallPurchaseRequested,
                HandleRerollRequested,
                HandleOfferHovered,
                HandleOfferHoverExited);

            _context.BoardInput.BindCallbacks(
                HandleAimChanged,
                HandleLaunchRequested);
        }

        private void StartLocation()
        {
            _state.CurrentEnemyIndex = 0;

            _state.CurrentMoney = Mathf.Clamp(
                _context.Location != null ? _context.Location.StartMoney : 0,
                0,
                _context.BalanceConfig.MaxMoney);

            StartEnemyBattle();
        }

        private void StartEnemyBattle()
        {
            EnemyDefinition enemy = GetCurrentEnemy();
            if (enemy == null)
            {
                Debug.LogError("BattleFlowController: enemy is missing.");
                return;
            }

            _state.CurrentEnemyHp = enemy.MaxHp;
            _state.RerollCount = 0;
            _state.PendingNextBallMultiplier = 1;
            _state.Phase = BattlePhase.AwaitOfferChoice;
            _state.ClearTurnSelection();

            RegenerateOffers();
            ApplyLocationAndEnemyVisuals();
            RefreshAllViews();
            ResetTransientUi();
        }

        private void ApplyLocationAndEnemyVisuals()
        {
            _context.BoardView.ApplyLocation(_context.Location);
            _context.BoardView.ApplyEnemyFingerSet(GetCurrentEnemy().FingerSet);
            _context.BoardView.ResetFingerPresentation();
            _context.EnemyView.ApplyEnemy(GetCurrentEnemy());
            _context.ProgressBarView.Refresh(
                _context.Location.Enemies.Length,
                _state.CurrentEnemyIndex);
            _context.OfferPanelView.ApplyLocation(_context.Location);
        }

        private void HandleBallPurchaseRequested(BallDefinition ball)
        {
            if (_state.Phase != BattlePhase.AwaitOfferChoice || ball == null)
            {
                return;
            }

            if (!_economyService.TrySpendBall(_state, ball))
            {
                RefreshAllViews();
                return;
            }

            PlaySfx(_context.AudioCueLibrary != null ? _context.AudioCueLibrary.Purchase : null);
            ShowSpendPopup(ball.Cost);

            _state.SelectedBall = ball;
            _state.CurrentAimNormalizedX = 0.5f;
            _state.Phase = BattlePhase.AwaitLaunchPosition;

            _context.BoardView.ShowAimingBall(ball, _state.CurrentAimNormalizedX, _context.BalanceConfig);
            _context.BoardInput.SetInteractable(true);
            _context.ScreenBlockerView.Show("Кликни по полю, чтобы сбросить шар");
            RefreshAllViews();
        }

        private void HandleAimChanged(float normalizedX)
        {
            if (_state.Phase != BattlePhase.AwaitLaunchPosition)
            {
                return;
            }

            _state.CurrentAimNormalizedX = Mathf.Clamp01(normalizedX);
            _context.BoardView.UpdateAimPosition(_state.CurrentAimNormalizedX);
        }

        private void HandleLaunchRequested(float normalizedX)
        {
            if (_state.Phase != BattlePhase.AwaitLaunchPosition || _state.SelectedBall == null)
            {
                return;
            }

            _flightGatePassCount = 0;
            _context.FlightCounterView.Show();

            _state.CurrentAimNormalizedX = Mathf.Clamp01(normalizedX);
            _state.Phase = BattlePhase.BallInFlight;
            _state.IsDropInProgress = true;

            _context.BoardInput.SetInteractable(false);
            _context.ScreenBlockerView.Show("Шар в полёте");

            _state.PlannedDrop = _dropPlannerService.BuildPlan(
                _context.BoardView,
                _state.SelectedBall,
                _state.CurrentAimNormalizedX,
                _random);

            _context.BoardView.HighlightTargetFinger(_state.PlannedDrop.TargetFingerIndex);
            RefreshAllViews();

            _context.BoardView.PlayDrop(
                _state.PlannedDrop,
                _context.BalanceConfig.SegmentBaseDuration,
                HandleGatePassed,
                HandleFingerLand,
                HandleDropCompleted);
        }

        private void HandleGatePassed(int gateId)
        {
            _flightGatePassCount++;
            _context.FlightCounterView.Increment();
            PlaySfx(_context.AudioCueLibrary != null ? _context.AudioCueLibrary.GatePass : null, 0.8f);
        }

        private void HandleFingerLand(int fingerIndex)
        {
            FingerSlotView finger = _context.BoardView.GetFingerSlot(fingerIndex);
            if (finger == null)
            {
                return;
            }

            FingerRuntimeInfo fingerInfo = finger.GetRuntimeInfo();
            _context.FlightCounterView.ShowResolve(fingerInfo.Multiplier);
            _context.ImpactPresenter.PresentFingerLanding(finger);

            PlaySfx(_context.AudioCueLibrary != null ? _context.AudioCueLibrary.FingerLand : null);
        }

        private void HandleDropCompleted(int playedImpacts, int landedFingerIndex)
        {
            _state.IsDropInProgress = false;
            _state.Phase = BattlePhase.Resolving;

            FingerRuntimeInfo fingerInfo = _context.BoardView.GetFingerRuntimeInfo(landedFingerIndex);

            BallResolutionData resolution = _resolutionService.Resolve(
                _state.SelectedBall,
                fingerInfo,
                _state,
                _state.PlannedDrop,
                playedImpacts,
                _random);

            _state.LastResolution = resolution;
            _economyService.ApplyResolution(_state, resolution);

            _context.ResultPopupView.Show(resolution, _context.BalanceConfig.ResultPopupDuration);
            _context.EnemyView.RefreshHp(GetCurrentEnemy(), _state.CurrentEnemyHp);
            _context.EnemyView.PlayHitOrDeath(_state.CurrentEnemyHp <= 0);

            PlayResultFlyouts(landedFingerIndex, resolution);
            PlayResolutionSfx(resolution);

            if (_advanceRoutine != null)
            {
                StopCoroutine(_advanceRoutine);
            }

            _advanceRoutine = StartCoroutine(AdvanceAfterResolve());
        }

        private IEnumerator AdvanceAfterResolve()
        {
            RefreshAllViews();
            yield return new WaitForSeconds(_context.BalanceConfig.ResolvePause);

            if (_state.CurrentEnemyHp <= 0)
            {
                ShowEnemyOrLocationCompleteWindow();
                yield break;
            }

            _state.ClearTurnSelection();
            _state.Phase = BattlePhase.AwaitOfferChoice;
            _context.BoardView.HideAimBall();
            _context.BoardView.ResetFingerPresentation();
            RegenerateOffers();
            RefreshAllViews();

            if (_economyService.IsDefeated(_state))
            {
                ShowDefeatWindow();
                yield break;
            }

            ResetTransientUi();
        }

        private void ShowEnemyOrLocationCompleteWindow()
        {
            EnemyDefinition enemy = GetCurrentEnemy();
            bool isLocationFinished = _state.CurrentEnemyIndex >= _context.Location.Enemies.Length - 1;

            if (!isLocationFinished)
            {
                _state.Phase = BattlePhase.EnemyDeathWindow;
                RefreshAllViews();
                _context.EnemyDeathWindowView.Show(enemy, HandleEnemyNextPressed);
                return;
            }

            _context.RunProgressService.MarkLocationCompleted(_context.Location.Id);

            int nextLocationIndex = _state.CurrentLocationIndex + 1;
            if (_context.Database.Locations != null && nextLocationIndex < _context.Database.Locations.Length)
            {
                _context.RunProgressService.UnlockLocation(nextLocationIndex);
            }

            _state.Phase = BattlePhase.LocationCompleteWindow;
            RefreshAllViews();
            _context.LocationEndWindowView.Show(_context.Location, enemy, ReturnToMenu);
        }

        private void HandleEnemyNextPressed()
        {
            _context.EnemyDeathWindowView.HideImmediate();
            _state.CurrentEnemyIndex++;
            StartEnemyBattle();
        }

        private void HandleRerollRequested()
        {
            if (_state.Phase != BattlePhase.AwaitOfferChoice)
            {
                return;
            }

            int rerollCost = _economyService.GetRerollCost(_state.RerollCount);

            if (!_economyService.TrySpendReroll(_state))
            {
                RefreshAllViews();
                return;
            }

            PlaySfx(_context.AudioCueLibrary != null ? _context.AudioCueLibrary.Reroll : null);
            ShowSpendPopup(rerollCost);

            RegenerateOffers();
            RefreshAllViews();

            if (_economyService.IsDefeated(_state))
            {
                ShowDefeatWindow();
            }
        }

        private void HandleOfferHovered(BallDefinition ball)
        {
            if (_context.TooltipView != null)
            {
                _context.TooltipView.Show(ball);
            }
        }

        private void HandleOfferHoverExited()
        {
            if (_context.TooltipView != null)
            {
                _context.TooltipView.Hide();
            }
        }

        private void RegenerateOffers()
        {
            _state.CurrentOffers.Clear();
            _state.CurrentOffers.AddRange(
                _offerGenerator.GenerateOffers(
                    _context.Database,
                    _context.BalanceConfig.OfferCount,
                    _random));
        }

        private void RefreshAllViews()
        {
            EnemyDefinition enemy = GetCurrentEnemy();
            int rerollCost = _economyService.GetRerollCost(_state.RerollCount);

            _context.HudView.Refresh(
                _state.CurrentMoney,
                _context.Location.DisplayName,
                _state.CurrentEnemyIndex + 1,
                _context.Location.Enemies.Length,
                _state.PendingNextBallMultiplier,
                _state.Phase.ToString(),
                GetDebugText());

            _context.ProgressBarView.Refresh(_context.Location.Enemies.Length, _state.CurrentEnemyIndex);

            if (enemy != null)
            {
                _context.EnemyView.RefreshHp(enemy, _state.CurrentEnemyHp);
            }

            _context.OfferPanelView.Refresh(
                _state.CurrentOffers,
                _state.CurrentMoney,
                rerollCost,
                _state.Phase == BattlePhase.AwaitOfferChoice);

            _context.BoardInput.SetInteractable(_state.Phase == BattlePhase.AwaitLaunchPosition);
        }

        private string GetDebugText()
        {
            if (!_context.BalanceConfig.ShowDebugOverlay || _state.PlannedDrop == null)
            {
                return string.Empty;
            }

            string played = _state.LastResolution != null ? _state.LastResolution.Collisions.ToString() : "-";
            return "TargetFinger: " + (_state.PlannedDrop.TargetFingerIndex + 1) +
                   " | TargetHits: " + _state.PlannedDrop.TargetHitCount +
                   " | Seed: " + _state.PlannedDrop.RouteSeed +
                   " | Played: " + played;
        }

        private void ShowDefeatWindow()
        {
            _state.Phase = BattlePhase.DefeatWindow;
            RefreshAllViews();

            PlaySfx(_context.AudioCueLibrary != null ? _context.AudioCueLibrary.Defeat : null);

            _context.LostWindowView.Show(
                GetCurrentEnemy(),
                RetryLocation,
                ReturnToMenu);
        }

        private void ResetTransientUi()
        {
            _context.BoardInput.SetInteractable(false);
            _context.BoardView.HideAimBall();
            _context.BoardView.ResetFingerPresentation();
            _context.ScreenBlockerView.Hide();
            _context.ResultPopupView.HideImmediate();
            _context.EnemyDeathWindowView.HideImmediate();
            _context.LocationEndWindowView.HideImmediate();
            _context.LostWindowView.HideImmediate();
            _context.TooltipView.Hide();
            _context.FlightCounterView.ResetImmediate();
        }

        private void RetryLocation()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void ReturnToMenu()
        {
            SceneManager.LoadScene("MenuScene");
        }

        private EnemyDefinition GetCurrentEnemy()
        {
            if (_context.Location == null || _context.Location.Enemies == null || _context.Location.Enemies.Length == 0)
            {
                return null;
            }

            int index = Mathf.Clamp(_state.CurrentEnemyIndex, 0, _context.Location.Enemies.Length - 1);
            return _context.Location.Enemies[index];
        }

        private void ShowSpendPopup(int amount)
        {
            if (_context.ResultFlyoutPresenter == null || _context.HudView == null || amount <= 0)
            {
                return;
            }

            RectTransform anchor = _context.HudView.SpendPopupAnchor;
            RectTransform target = _context.HudView.MoneyTargetAnchor;

            if (anchor != null && target != null)
            {
                _context.ResultFlyoutPresenter.PlayFromUiToUi(anchor, target, "-" + amount);
            }
        }

        private void PlayResultFlyouts(int landedFingerIndex, BallResolutionData resolution)
        {
            if (_context.ResultFlyoutPresenter == null || resolution == null)
            {
                return;
            }

            FingerSlotView finger = _context.BoardView.GetFingerSlot(landedFingerIndex);
            if (finger == null || finger.LandingPoint == null)
            {
                return;
            }

            Vector3 worldSource = finger.LandingPoint.position;

            if (resolution.MoneyDelta != 0 && _context.HudView != null && _context.HudView.MoneyTargetAnchor != null)
            {
                _context.ResultFlyoutPresenter.PlayFromWorldToUi(
                    worldSource,
                    _context.HudView.MoneyTargetAnchor,
                    FormatSigned(resolution.MoneyDelta));
            }

            if (resolution.DamageDelta != 0 && _context.EnemyView != null && _context.EnemyView.DamageTargetAnchor != null)
            {
                _context.ResultFlyoutPresenter.PlayFromWorldToUi(
                    worldSource,
                    _context.EnemyView.DamageTargetAnchor,
                    FormatSigned(-resolution.DamageDelta));
            }
        }

        private void PlayResolutionSfx(BallResolutionData resolution)
        {
            if (resolution == null || _context.AudioCueLibrary == null)
            {
                return;
            }

            if (resolution.MoneyDelta > 0)
            {
                PlaySfx(_context.AudioCueLibrary.MoneyGain);
            }

            if (resolution.DamageDelta > 0)
            {
                PlaySfx(_context.AudioCueLibrary.EnemyHit);
            }

            if (_state.CurrentEnemyHp <= 0)
            {
                PlaySfx(_context.AudioCueLibrary.EnemyDeath);
            }
        }

        private void PlaySfx(AudioClip clip, float volumeScale = 1f)
        {
            if (AudioService.Instance != null && clip != null)
            {
                AudioService.Instance.PlaySfx(clip, volumeScale);
            }
        }

        private static string FormatSigned(int value)
        {
            return value > 0 ? "+" + value : value.ToString();
        }
    }
}