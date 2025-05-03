using Godot;

/// <summary>
/// 地形类型
/// </summary>
public enum TerrainType
{
    /// <summary>
    /// 水
    /// </summary>
    Water,
    /// <summary>
    /// 地面
    /// </summary>
    Ground,
    /// <summary>
    /// 沙子
    /// </summary>
    Sand,
    /// <summary>
    /// 岩石
    /// </summary>
    Rock
}

/// <summary>
/// 瓦片更新数据
/// </summary>
public struct TileUpdateData
{
    public Vector2I coords;
    public int layer;
    public int sourceId;
    public Vector2I atlasCoords;
    public int alternativeTile;
}

/// <summary>
/// 瓦片的地形数据
/// </summary>
public struct TileTerrain
{
    public int terrainSet;
    public int terrain;
}

/// <summary>
/// 瓦片与周围的连接情况
/// </summary>
public struct TilePeering
{
    public int top;
    public int topRight;
    public int right;
    public int bottomRight;
    public int bottom;
    public int bottomLeft;
    public int left;
    public int topLeft;

    public TilePeering Set(Vector2I direction, int value)
    {
        if (direction.X < 0)
        {
            if (direction.Y < 0)
            {
                topLeft = value;
            }
            else if (direction.Y > 0)
            {
                bottomLeft = value;
            }
            else
            {
                left = value;
            }
        }
        else if (direction.X > 0)
        {
            if (direction.Y < 0)
            {
                topRight = value;
            }
            else if (direction.Y > 0)
            {
                bottomRight = value;
            }
            else
            {
                right = value;
            }
        }
        else
        {
            if (direction.Y < 0)
            {
                top = value;
            }
            else if (direction.Y > 0)
            {
                bottom = value;
            }
        }

        return this;
    }
}