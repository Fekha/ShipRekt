using System.Collections.Generic;
using static GameManager;

public enum HexDirection
{
    BottomRight,
    BottomLeft,
    Left,
    TopLeft,
    TopRight,
    Right
}

public class HexCoords
{
    public HexCoords(int i, int j, int type = 0, int rotation = 0)
    {
        x = i;
        y = j;
        this.type = type;
        this.rotation = rotation;
    }
    public int x;
    public int y;
    public int type;
    public int rotation;
}

public class HexDirections
{
    public static Dictionary<HexDirection, HexCoords> directionMap = new Dictionary<HexDirection, HexCoords>()
    {
        { HexDirection.BottomRight, new HexCoords(-1, 1) },
        { HexDirection.BottomLeft, new HexCoords(-1, 0) },
        { HexDirection.Left, new HexCoords(0, -1) },
        { HexDirection.TopLeft, new HexCoords(1, -1) },
        { HexDirection.TopRight, new HexCoords(1, 0) },
        { HexDirection.Right, new HexCoords(0, 1) }
    };
}
