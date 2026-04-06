using ChairFarming.Runtime.Board;
using ChairFarming.Runtime.Core;
using ChairFarming.Runtime.Save;
using ChairFarming.Runtime.UI;

namespace ChairFarming.Runtime.Battle
{
    public sealed class BattleContext
    {
        public ProjectDatabase Database;
        public GameBalanceConfig BalanceConfig;
        public LocationDefinition Location;
        public RunProgressService RunProgressService;

        public BoardView BoardView;
        public BoardViewportInput BoardInput;
        public ImpactPresenter ImpactPresenter;
        public EnemyView EnemyView;
        public BattleHudView HudView;
        public OfferPanelView OfferPanelView;
        public BallTooltipView TooltipView;
        public BattleLogView BattleLogView;
        public LocationProgressBarView ProgressBarView;
        public ScreenBlockerView ScreenBlockerView;
        public EnemyDeathWindowView EnemyDeathWindowView;
        public LocationEndWindowView LocationEndWindowView;
        public LostWindowView LostWindowView;

        public FlightCounterView FlightCounterView;
        public ResultFlyoutPresenter ResultFlyoutPresenter;
        public AudioCueLibrary AudioCueLibrary;
    }
}