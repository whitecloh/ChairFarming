using ChairFarming.Runtime.App;
using ChairFarming.Runtime.Board;
using ChairFarming.Runtime.Core;
using ChairFarming.Runtime.Save;
using ChairFarming.Runtime.UI;
using UnityEngine;

namespace ChairFarming.Runtime.Battle
{
    public sealed class BattleSceneController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private BoardView boardView;
        [SerializeField] private BoardViewportInput boardViewportInput;
        [SerializeField] private ImpactPresenter impactPresenter;
        [SerializeField] private EnemyView enemyView;
        [SerializeField] private BattleHudView hudView;
        [SerializeField] private OfferPanelView offerPanelView;
        [SerializeField] private BallTooltipView tooltipView;
        [SerializeField] private BattleLogView battleLogView;
        [SerializeField] private LocationProgressBarView progressBarView;
        [SerializeField] private ScreenBlockerView screenBlockerView;
        [SerializeField] private EnemyDeathWindowView enemyDeathWindowView;
        [SerializeField] private LocationEndWindowView locationEndWindowView;
        [SerializeField] private LostWindowView lostWindowView;

        [Header("Additional")]
        [SerializeField] private FlightCounterView flightCounterView;
        [SerializeField] private ResultFlyoutPresenter resultFlyoutPresenter;
        [SerializeField] private AudioCueLibrary audioCueLibrary;

        private BattleFlowController _battleFlowController;

        private void Awake()
        {
            if (boardView != null)
            {
                boardView.InitializeRuntimeLayout();
            }

            GameSession session = GameSession.Instance;
            if (session == null || session.ProjectDatabase == null || !session.HasSelectedLocation)
            {
                Debug.LogError("BattleSceneController: GameSession or selected location is missing.");
                enabled = false;
                return;
            }

            BattleContext context = new BattleContext
            {
                Database = session.ProjectDatabase,
                BalanceConfig = session.ProjectDatabase.GameBalanceConfig,
                Location = session.GetSelectedLocation(),
                RunProgressService = new RunProgressService(),
                BoardView = boardView,
                BoardInput = boardViewportInput,
                ImpactPresenter = impactPresenter,
                EnemyView = enemyView,
                HudView = hudView,
                OfferPanelView = offerPanelView,
                TooltipView = tooltipView,
                BattleLogView = battleLogView,
                ProgressBarView = progressBarView,
                ScreenBlockerView = screenBlockerView,
                EnemyDeathWindowView = enemyDeathWindowView,
                LocationEndWindowView = locationEndWindowView,
                LostWindowView = lostWindowView,
                FlightCounterView = flightCounterView,
                ResultFlyoutPresenter = resultFlyoutPresenter,
                AudioCueLibrary = audioCueLibrary,
            };

            if (!ValidateContext(context))
            {
                enabled = false;
                return;
            }

            if (AudioService.Instance != null && context.Location.Theme != null && context.AudioCueLibrary.MenuMusic != null)
            {
                AudioService.Instance.PlayMusic(context.AudioCueLibrary.MenuMusic);
            }

            _battleFlowController = gameObject.AddComponent<BattleFlowController>();
            _battleFlowController.Initialize(context);
        }

        private static bool ValidateContext(BattleContext context)
        {
            bool valid =
                context.BoardView != null &&
                context.BoardInput != null &&
                context.ImpactPresenter != null &&
                context.EnemyView != null &&
                context.HudView != null &&
                context.OfferPanelView != null &&
                context.BattleLogView != null &&
                context.ProgressBarView != null &&
                context.ScreenBlockerView != null &&
                context.EnemyDeathWindowView != null &&
                context.LocationEndWindowView != null &&
                context.LostWindowView != null &&
                context.Database != null &&
                context.BalanceConfig != null &&
                context.Location != null;

            if (!valid)
            {
                Debug.LogError("BattleSceneController: missing required scene references.");
            }

            return valid;
        }
    }
}