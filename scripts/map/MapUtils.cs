using Godot;

/// <summary>
/// 地图工具类
/// 提供地图加载器和瓦片集的引用，以及一些常用的地图相关的计算方法
/// </summary>
public static class MapUtils
{
    /// <summary>
    /// 地图加载器
    /// </summary>
    public static MapLoader MapLoader { get; set; }

    /// <summary>
    /// 瓦片集
    /// </summary>
    public static TileSet TileSet { get; set; }

    /// <summary>
    /// 瓦片的大小，即一个瓦片的长和宽是多少像素
    /// </summary>
    public static Vector2I TileSize { get => TileSet.TileSize; }

    /// <summary>
    /// 区块大小，即一个区块长和宽包含的瓦片数量
    /// </summary>
    public static Vector2I ChunkSize { get => MapLoader.ChunkSize; }

    /// <summary>
    /// 区块的像素大小，即一个区块的长和宽是多少像素
    /// </summary>
    public static Vector2I ChunkPixelSize { get; set; }

    /// <summary>
    /// 八个方向
    /// </summary>
    public static readonly Vector2I[] DIRECTIONS =
    {
        new(0, -1), // 上
        new(1, -1), // 右上
        new(1, 0), // 右
        new(1, 1), // 右下
        new(0, 1), // 下
        new(-1, 1), // 左下
        new(-1, 0), // 左
        new(-1, -1) // 左上
    };

    /// <summary>
    /// 全局的瓦片坐标转为区块坐标
    /// </summary>
    public static Vector2I GlobalTileToChunk(Vector2I globalTileCoords)
    {
        return globalTileCoords / ChunkSize;
    }

    /// <summary>
    /// 全局的瓦片坐标转为区块内的瓦片坐标
    /// </summary>
    public static Vector2I GlobalTileToLocal(Vector2I globalTileCoords)
    {
        return globalTileCoords % ChunkSize;
    }

    /// <summary>
    /// 全局像素坐标转为区块坐标
    /// </summary>
    public static Vector2I GlobalPixelToChunk(Vector2I globalPixelCoords)
    {
        return globalPixelCoords / ChunkPixelSize;
    }

    /// <summary>
    /// 全局像素坐标转瓦片坐标
    /// </summary>
    public static Vector2I GlobalPixelToTile(Vector2I globalPixelCoords)
    {
        return globalPixelCoords / TileSize;
    }

    /// <summary>
    /// 区块坐标转全局像素坐标
    /// </summary>
    public static Vector2I ChunkToGlobalPixel(Vector2I chunkCoords)
    {
        return chunkCoords * ChunkPixelSize;
    }

    /// <summary>
    /// 区块坐标转全局瓦片坐标
    /// </summary>
    public static Vector2I ChunkToGlobalTile(Vector2I chunkCoords)
    {
        return chunkCoords * ChunkSize;
    }
}
