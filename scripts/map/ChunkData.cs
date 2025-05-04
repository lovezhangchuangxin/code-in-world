using System.Collections.Generic;
using Godot;
using Godot.Collections;

/// <summary>
/// 区块数据，可以被序列化保存到文件中，用于在区块节点中提供数据
/// </summary>
public partial class ChunkData : Resource
{
    /// <summary>
    /// tiledata 的原始数据，避免使用 Godot 的 Array 从而导致频繁的拷贝和封装
    /// </summary>
    public List<byte[]> _tileData = [];

    /// <summary>
    /// 每个元素代表一个 TileMapLayer 使用的数据
    /// </summary>
    [Export]
    public Array<byte[]> TileData { get; set; } = [];

    /// <summary>
    /// 区块大小
    /// </summary>
    [Export]
    public Vector2I ChunkSize { get; set; } = Vector2I.Zero;

    /// <summary>
    /// 区块坐标
    /// </summary>
    [Export]
    public Vector2I ChunkCoord { get; set; } = Vector2I.Zero;

    /// <summary>
    /// 区块数据是否可以更新，一开始初始化不需要更新，初始化完成之后再修改需要更新
    /// </summary>
    [Export]
    public bool CanUpdate { get; set; } = false;

    /// <summary>
    /// 区块数据是否被修改过，只有被修改过的区块才会被保存
    /// </summary>
    [Export]
    public bool IsModified { get; set; } = false;

    /// <summary>
    /// 需要更新的数据
    /// </summary>
    public List<TileUpdateData> NeedUpdateData = [];

    /// <summary>
    /// 根据提供的层初始化区块数据，每层表示一个 TileMapLayer
    /// </summary>
    public void Init(int layerCount)
    {
        TileData.Resize(layerCount);
        for (int i = 0; i < layerCount; i++)
        {
            // 每个瓦片会使用 12 个字节存储，另外还需要 2 个字节存储版本相关数据
            var data = new byte[ChunkSize.X * ChunkSize.Y * 12 + 2];
            // 全局初始化为 -1
            System.Array.Fill(data, (byte)0xff);
            // 版本相关的数据存储再开头两个字节
            data[0] = 0;
            data[1] = 0;
            _tileData.Add(data);
            TileData[i] = data;
        }
    }

    /// <summary>
    /// 根据局部的瓦片坐标设置瓦片数据
    /// </summary>
    public void SetTileData(Vector2I coords, int layer, int sourceId, Vector2I atlasCoord, int alternativeTile, bool modified = false)
    {
        if (!IsCoordInChunk(coords))
        {
            GD.PushWarning($"SetTile: coord {coords} is out of chunk size {ChunkSize}");
            return;
        }

        // 取出对应层的瓦片数据
        var data = _tileData[layer];
        // 偏移用来存储版本的 2 个字节
        var index = 2;
        // 计算对应瓦片在瓦片数据层的开始位置
        index += (coords.Y * ChunkSize.X + coords.X) * 12;
        // 设置瓦片数据，TODO：不确定是小端还是大端，是否有跨平台问题
        data[index] = (byte)(coords.X & 0xff);
        data[index + 1] = (byte)((coords.X >> 8) & 0xff);
        data[index + 2] = (byte)(coords.Y & 0xff);
        data[index + 3] = (byte)((coords.Y >> 8) & 0xff);
        data[index + 4] = (byte)(sourceId & 0xff);
        data[index + 5] = (byte)((sourceId >> 8) & 0xff);
        data[index + 6] = (byte)(atlasCoord.X & 0xff);
        data[index + 7] = (byte)((atlasCoord.X >> 8) & 0xff);
        data[index + 8] = (byte)(atlasCoord.Y & 0xff);
        data[index + 9] = (byte)((atlasCoord.Y >> 8) & 0xff);
        data[index + 10] = (byte)(alternativeTile & 0xff);
        data[index + 11] = (byte)((alternativeTile >> 8) & 0xff);
        TileData[layer] = data;

        if (modified && !IsModified)
        {
            IsModified = true;
        }

        if (CanUpdate)
        {
            NeedUpdateData.Add(new TileUpdateData()
            {
                coords = coords,
                layer = layer,
                sourceId = sourceId,
                atlasCoords = atlasCoord,
                alternativeTile = alternativeTile
            });
        }
    }

    public void SetTileData(Vector2I coords, int layer, TerrainType type, bool modified = false)
    {
        if (!MapUtils.MapLoader.AutoTile.TilesDataByType.TryGetValue(type, out TileData tileData))
        {
            GD.PushWarning($"SetTile: type {type} is not in AutoTile data");
            return;
        }

        int sourceId = tileData.GetMeta("sourceId").AsInt32();
        Vector2I atlasCoord = tileData.GetMeta("atlasCoords").AsVector2I();
        int alternative = tileData.GetMeta("alternative").AsInt32();
        SetTileData(coords, layer, sourceId, atlasCoord, alternative, modified);
    }

    /// <summary>
    /// 根据局部的瓦片坐标获取瓦片信息
    /// </summary>
    public TileUpdateData GetTileData(Vector2I coords, int layer)
    {
        if (!IsCoordInChunk(coords))
        {
            GD.PushWarning($"GetTile: coord {coords} is out of chunk size {ChunkSize}");
            return new TileUpdateData();
        }

        // 取出对应层的瓦片数据
        var data = TileData[layer];
        // 偏移用来存储版本的 2 个字节
        var index = 2;
        // 计算对应瓦片在瓦片数据层的开始位置
        index += (coords.Y * ChunkSize.X + coords.X) * 12;
        return new TileUpdateData()
        {
            sourceId = data[index] | (data[index + 1] << 8),
            atlasCoords = new Vector2I(data[index + 6] | (data[index + 7] << 8), data[index + 8] | (data[index + 9] << 8)),
            alternativeTile = data[index + 10] | (data[index + 11] << 8)
        };
    }

    /// <summary>
    /// 使用局部瓦片坐标设置场景
    /// </summary>
    public void SetSceneData(Vector2I coords, int layer, int sceneSet, int scene, bool modified = false)
    {
        SetTileData(coords, layer, sceneSet, new Vector2I(0, 0), scene, modified);
    }

    /// <summary>
    /// 使用局部瓦片坐标获取地形信息
    /// </summary>
    public TileTerrain GetTerrainData(Vector2I coords, int layer)
    {
        if (!IsCoordInChunk(coords))
        {
            GD.PushWarning($"GetTile: coord {coords} is out of chunk size {ChunkSize}");
            return new TileTerrain();
        }

        TileUpdateData data = GetTileData(coords, layer);
        if (MapUtils.TileSet.HasSource(data.sourceId) && MapUtils.TileSet.GetSource(data.sourceId) is TileSetAtlasSource atlasSource)
        {
            var tileData = atlasSource.GetTileData(data.atlasCoords, data.alternativeTile);
            return new TileTerrain()
            {
                terrainSet = tileData.TerrainSet,
                terrain = tileData.Terrain
            };
        }
        else
        {
            return new TileTerrain()
            {
                terrainSet = -1,
                terrain = -1
            };
        }
    }

    /// <summary>
    /// 根据局部瓦片坐标获取瓦片类型
    /// </summary>
    public TerrainType GetTileType(Vector2I coords, int layer)
    {
        if (!IsCoordInChunk(coords))
        {
            GD.PushWarning($"GetTile: coord {coords} is out of chunk size {ChunkSize}");
            return TerrainType.None;
        }

        TileUpdateData data = GetTileData(coords, layer);
        if (MapUtils.TileSet.HasSource(data.sourceId) && MapUtils.TileSet.GetSource(data.sourceId) is TileSetAtlasSource atlasSource)
        {
            var tileData = atlasSource.GetTileData(data.atlasCoords, data.alternativeTile);
            return (TerrainType)(int)tileData.GetCustomData("type");
        }
        else
        {
            return TerrainType.None;
        }
    }

    /// <summary>
    /// 检查坐标是否在区块内
    /// </summary>
    public bool IsCoordInChunk(Vector2I coord)
    {
        return coord.X >= 0 && coord.X < ChunkSize.X && coord.Y >= 0 && coord.Y < ChunkSize.Y;
    }
}