using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// 区块节点
/// 区块是地图渲染的基本单位，包含了一个区块内的所有瓦片
/// </summary>
public partial class ChunkNode : Node2D
{
    /// <summary>
    /// 存 tileMapLayer 节点，方便后续修改瓦片
    /// </summary>
    protected TileMapLayer[] _layers = [];

    /// <summary>
    /// 区块数据
    /// </summary>
    public ChunkData ChunkData { get; set; }

    /// <summary>
    /// 需要进行 y 排序的层
    /// </summary>
    public int[] YSortLayers { get; set; } = [];

    public override void _Ready()
    {
        // 设置 y 排序，使得子节点 y 排序可以生效
        YSortEnabled = true;
        // 设置位置
        GlobalPosition = MapUtils.ChunkToGlobalPixel(ChunkData.ChunkCoord);
        // 使用区块数据来生产 tileMapLayer 节点
        _layers = new TileMapLayer[ChunkData.TileData.Count];
        for (int i = 0; i < ChunkData.TileData.Count; i++)
        {
            TileMapLayer layer = new()
            {
                TileSet = MapUtils.TileSet,
                TextureFilter = TextureFilterEnum.Nearest,
                TileMapData = ChunkData._tileData[i]
            };

            if (ChunkData.ChunkCoord == Vector2I.Zero)
            {
                GD.Print($"ChunkData.TileData: {ChunkData.TileData[i].Length} {ChunkData.TileData[i][2]} {ChunkData.TileData[i][3]}");
            }

            if (YSortLayers.Contains(i))
            {
                layer.YSortEnabled = true;
            }
            else
            {
                // 不需要 y 排序的层放在下面
                layer.ZIndex = -1;
            }

            _layers[i] = layer;
            AddChild(layer);
        }
    }

    public override void _Process(double delta)
    {
        List<TileUpdateData> needUpdateData = ChunkData.NeedUpdateData;
        ChunkData.NeedUpdateData.Clear();

        foreach (var data in needUpdateData)
        {
            TileMapLayer layer = _layers[data.layer];
            layer.SetCell(data.coords, data.sourceId, data.atlasCoords, data.alternativeTile);
            Vector2I globalTileCoord = MapUtils.ChunkToGlobalTile(ChunkData.ChunkCoord) + data.coords;
            MapUtils.MapLoader.EmitSignal(MapLoader.SignalName.TileChanged, globalTileCoord);
        }
    }
}