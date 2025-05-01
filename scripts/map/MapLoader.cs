using Godot;

/// <summary>
/// 地图加载器
/// </summary>
public partial class MapLoader : Node2D
{
    /// <summary>
    /// 区块大小
    /// </summary>
    [Export]
    public Vector2I ChunkSize { get; set; } = new Vector2I(16, 16);

    /// <summary>
    /// 瓦片发生改变时发出，提供全局的瓦片坐标
    /// </summary>
    [Signal]
    public delegate void TileChangedEventHandler(Vector2I globalTileCoord);

    /// <summary>
    /// 地图初始区块加载完成时发出，可用于关闭加载页面
    /// </summary>
    [Signal]
    public delegate void MapInitEventHandler();

    /// <summary>
    /// 区块加载完成时发出，提供加载完成的区块的坐标
    /// </summary>
    [Signal]
    public delegate void ChunkLoadedEventHandler(Vector2I chunkCoord);

    /// <summary>
    /// 区块卸载完成时发出，提供卸载完成的区块的坐标
    /// </summary>
    [Signal]
    public delegate void ChunkUnloadedEventHandler(Vector2I chunkCoord);

    public override void _Ready()
    {
    }
}