using Godot;

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
    /// 瓦片集的大小
    /// </summary>
    public static Vector2 TileSize { get => TileSet.TileSize; }

    /// <summary>
    /// 区块大小
    /// </summary>
    public static Vector2 ChunkSize { get => MapLoader.ChunkSize; }
}
