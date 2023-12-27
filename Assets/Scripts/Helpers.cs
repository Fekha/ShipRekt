public enum HexDirection
{
    BottomRight,
    BottomLeft,
    Left,
    TopLeft,
    TopRight,
    Right
}

public enum Orders
{
    Evade,
    Move,
    TurnLeft,
    TurnRight,
    ShootLeft,
    ShootRight,
    None,
}
public enum Treasure
{
    Earring,
    Ring,
    Goblet,
    Crown,
    Necklace,
    Coins,
    GreenDie,
    PinkDie,
    BlueDie,
    YellowDie,
    RedDie,
    OrangeDie,
    None
}

public static class Settings
{
    public static int NumPlayers = 2;
    public static int NumNPCs = 1;
}
