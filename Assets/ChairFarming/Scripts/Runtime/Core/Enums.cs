namespace ChairFarming.Runtime.Core
{
    public enum BallCategory
    {
        Money = 0,
        Damage = 1,
        Utility = 2,
    }

    public enum ToeType
    {
        Normal = 0,
        BigToe = 1,
        Pinky = 2,
    }

    public enum FootSide
    {
        None = 0,
        Left = 1,
        Right = 2,
    }

    public enum BallSpecialMode
    {
        None = 0,
        MoneyToDamageX5 = 1,
        ComboThreshold = 2,
        NextBallMultiplier = 3,
    }

    public enum BattlePhase
    {
        None = 0,
        Preparing = 1,
        AwaitOfferChoice = 2,
        AwaitLaunchPosition = 3,
        BallInFlight = 4,
        Resolving = 5,
        EnemyDeathWindow = 6,
        LocationCompleteWindow = 7,
        DefeatWindow = 8,
    }

    public enum RoutePointType
    {
        Move = 0,
        PinImpact = 1,
        FingerEntry = 2,
        FingerLand = 3,
        GatePass = 4,
    }
}