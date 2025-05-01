using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class AutoTile : Node
{
    /// <summary>
    /// 地形的瓦片数据，键为经过计算的瓦片 id，值为对应的 TileData
    /// </summary>
    public Dictionary<int, TileData> TerrainTilesData = [];

    /// <summary>、
    /// 记录每个类型对应的瓦片，键为类型，值为对应的瓦片数据
    /// </summary>
    public Dictionary<TerrainType, TileData> TilesDataByType = [];

    /// <summary>
    /// 构造数据集合
    /// </summary>
    public void Build()
    {
        foreach (int i in Enumerable.Range(0, MapUtils.TileSet.GetSourceCount()))
        {
            int sourceId = MapUtils.TileSet.GetSourceId(i);
            TileSetSource source = MapUtils.TileSet.GetSource(sourceId);

            if (source is TileSetAtlasSource tileSetAtlasSource)
            {
                foreach (int j in Enumerable.Range(0, tileSetAtlasSource.GetTilesCount()))
                {
                    var atlasCoords = tileSetAtlasSource.GetTileId(j);
                    foreach (int k in Enumerable.Range(0, tileSetAtlasSource.GetAlternativeTilesCount(atlasCoords)))
                    {
                        int alternative = tileSetAtlasSource.GetAlternativeTileId(atlasCoords, k);
                        TileData tileData = tileSetAtlasSource.GetTileData(atlasCoords, alternative);
                        int id = GetTileDataId(tileData);
                        if (id >= 0)
                        {
                            tileData.SetMeta("sourceId", sourceId);
                            tileData.SetMeta("atlasCoords", atlasCoords);
                            tileData.SetMeta("alternative", alternative);
                            TerrainTilesData.Add(id, tileData);
                        }

                        if (tileData.HasCustomData("type"))
                        {
                            TerrainType type = (TerrainType)(int)tileData.GetCustomData("type");
                            TilesDataByType.Add(type, tileData);
                        }
                    }
                }
            }

            // 暂时不考虑场景合集
        }
    }

    /// <summary>
    /// 获取瓦片数据的 id
    /// </summary>
    public static int GetTileDataId(TileData tileData)
    {
        if (tileData.TerrainSet == -1 || tileData.Terrain == -1)
        {
            return -1;
        }

        int terrainSet = tileData.TerrainSet;
        int terrain = tileData.Terrain;

        // 如果是匹配边和角，需要考虑八个方向的连接情况
        if (MapUtils.TileSet.GetTerrainSetMode(tileData.TerrainSet) == TileSet.TerrainMode.CornersAndSides)
        {
            return GetTileId(terrainSet, terrain,
                tileData.GetTerrainPeeringBit(TileSet.CellNeighbor.TopSide) == terrain ? 1 : 0,
                tileData.GetTerrainPeeringBit(TileSet.CellNeighbor.TopRightSide) == terrain ? 1 : 0,
                tileData.GetTerrainPeeringBit(TileSet.CellNeighbor.RightSide) == terrain ? 1 : 0,
                tileData.GetTerrainPeeringBit(TileSet.CellNeighbor.BottomRightSide) == terrain ? 1 : 0,
                tileData.GetTerrainPeeringBit(TileSet.CellNeighbor.BottomSide) == terrain ? 1 : 0,
                tileData.GetTerrainPeeringBit(TileSet.CellNeighbor.BottomLeftSide) == terrain ? 1 : 0,
                tileData.GetTerrainPeeringBit(TileSet.CellNeighbor.LeftSide) == terrain ? 1 : 0,
                tileData.GetTerrainPeeringBit(TileSet.CellNeighbor.TopLeftSide) == terrain ? 1 : 0);
        }

        // 如果是匹配边，需要考虑四个方向的连接情况
        if (MapUtils.TileSet.GetTerrainSetMode(tileData.TerrainSet) == TileSet.TerrainMode.Sides)
        {
            return GetTileId(terrainSet, terrain,
                tileData.GetTerrainPeeringBit(TileSet.CellNeighbor.TopSide) == terrain ? 1 : 0,
                0,
                tileData.GetTerrainPeeringBit(TileSet.CellNeighbor.RightSide) == terrain ? 1 : 0,
                0,
                tileData.GetTerrainPeeringBit(TileSet.CellNeighbor.BottomSide) == terrain ? 1 : 0,
                0,
                tileData.GetTerrainPeeringBit(TileSet.CellNeighbor.LeftSide) == terrain ? 1 : 0,
                0);
        }

        return -1;
    }

    /// <summary>
    /// 根据瓦片所属的地形集和地形以及周围的连接情况计算瓦片 id
    /// </summary>
    public static int GetTileId(int terrainSet, int terrain, int top, int topRight, int right, int bottomRight, int bottom, int bottomLeft, int left, int topLeft)
    {
        // 计算 id
        int id = terrainSet;
        id <<= 40;
        id |= terrain;

        // 计算连接情况
        id <<= 8;
        id |= (top << 7) | (topRight << 6) | (right << 5) | (bottomRight << 4) | (bottom << 3) | (bottomLeft << 2) | (left << 1) | topLeft;

        return id;
    }

    /// <summary>
    /// 标准化位，当角位为连接时，所对应的两条边也应该连接，否则这个角的连接是无效的
    /// </summary>
    public static void NormalizeBit(TilePeering tilePeering)
    {
        if (tilePeering.topLeft == 1)
        {
            tilePeering.topLeft = (tilePeering.top == 1 && tilePeering.left == 1) ? 1 : 0;
        }
        if (tilePeering.topRight == 1)
        {
            tilePeering.topRight = (tilePeering.top == 1 && tilePeering.right == 1) ? 1 : 0;
        }
        if (tilePeering.bottomLeft == 1)
        {
            tilePeering.bottomLeft = (tilePeering.bottom == 1 && tilePeering.left == 1) ? 1 : 0;
        }
        if (tilePeering.bottomRight == 1)
        {
            tilePeering.bottomRight = (tilePeering.bottom == 1 && tilePeering.right == 1) ? 1 : 0;
        }
    }

    /// <summary>
    /// 绘制地形
    /// </summary>
    public void DrawTerrain(Dictionary<Vector2I, ChunkData> ChunkDatas, int layer, Vector2I globalTileCoords, int terrainSet, int terrain, bool modified)
    {
        // 清除地形
        if (terrainSet == -1 || terrain == -1)
        {
            // 区块坐标和区块内的局部坐标
            Vector2I chunkCoords = MapUtils.GlobalTileToChunk(globalTileCoords);
            Vector2I tileCoords = MapUtils.GlobalTileToLocal(globalTileCoords);
            ChunkData chunkData = ChunkDatas[chunkCoords];
            // 使用 -1 来清除地形
            chunkData.SetTileData(tileCoords, layer, -1, new Vector2I(-1, -1), -1, modified);

            // 更新周围的瓦片
            foreach (Vector2I dir in MapUtils.DIRECTIONS)
            {
                Vector2I neighborCoords = globalTileCoords + dir;
                Vector2I neighborChunkCoords = MapUtils.GlobalTileToChunk(neighborCoords);
                if (!ChunkDatas.ContainsKey(neighborChunkCoords))
                {
                    continue;
                }

                ChunkData neighborChunkData = ChunkDatas[neighborChunkCoords];
                Vector2I neighborTileCoords = MapUtils.GlobalTileToLocal(neighborCoords);
                TileTerrain neighborTerrain = neighborChunkData.GetTerrainData(neighborTileCoords, layer);
                if (neighborTerrain.terrainSet != -1 && neighborTerrain.terrain != -1)
                {
                    // 更新周围的瓦片
                    UpdateTile(ChunkDatas, layer, neighborCoords, modified);
                }
            }
            return;
        }

        TilePeering bits = new();
        // 需要更新的邻居坐标
        Vector2I[] updateCoords = [];

        foreach (Vector2I dir in MapUtils.DIRECTIONS)
        {
            // 如果这是一个四方向地形，那么忽略四个角的检测
            if (MapUtils.TileSet.GetTerrainSetMode(terrainSet) == TileSet.TerrainMode.Sides && dir.X != 0 && dir.Y != 0)
            {
                continue;
            }

            Vector2I neighborCoords = globalTileCoords + dir;
            Vector2I neighborChunkCoords = MapUtils.GlobalTileToChunk(neighborCoords);
            if (!ChunkDatas.ContainsKey(neighborChunkCoords))
            {
                continue;
            }

            ChunkData neighborChunkData = ChunkDatas[neighborChunkCoords];
            Vector2I neighborTileCoords = MapUtils.GlobalTileToLocal(neighborCoords);
            TileTerrain neighborTerrain = neighborChunkData.GetTerrainData(neighborTileCoords, layer);
            // 如果周围瓷砖的地形集和地形和当前一样，那就将这个方向连接情况设置为连接
            if (terrainSet == neighborTerrain.terrainSet && terrain == neighborTerrain.terrain)
            {
                bits.Set(dir, 1);
                updateCoords.Append(neighborCoords);
            }
        }

        NormalizeBit(bits);
        int id = GetTileId(terrainSet, terrain,
            bits.top, bits.topRight, bits.right, bits.bottomRight, bits.bottom, bits.bottomLeft, bits.left, bits.topLeft);
        if (TerrainTilesData.TryGetValue(id, out TileData tileData))
        {
            // 区块坐标和区块内的局部坐标
            Vector2I chunkCoords = MapUtils.GlobalTileToChunk(globalTileCoords);
            Vector2I tileCoords = MapUtils.GlobalTileToLocal(globalTileCoords);

            int sourceId = (int)tileData.GetMeta("sourceId");
            Vector2I atlasCoords = (Vector2I)tileData.GetMeta("atlasCoords");
            int alternative = (int)tileData.GetMeta("alternative");

            ChunkData chunkData = ChunkDatas[chunkCoords];
            chunkData.SetTileData(tileCoords, layer, sourceId, atlasCoords, alternative, modified);

            // 更新周围的瓦片
            foreach (Vector2I neighborCoords in updateCoords)
            {
                UpdateTile(ChunkDatas, layer, neighborCoords, modified);
            }
        }
        else
        {
            GD.PushWarning($"DrawTerrain: tile {id} not found");
        }
    }

    /// <summary>
    /// 更新瓦片
    /// </summary>
    public void UpdateTile(Dictionary<Vector2I, ChunkData> ChunkDatas, int layer, Vector2I globalTileCoords, bool modified)
    {
        // 所在的区块坐标和区块内的局部坐标
        Vector2I chunkCoords = MapUtils.GlobalTileToChunk(globalTileCoords);
        Vector2I tileCoords = MapUtils.GlobalTileToLocal(globalTileCoords);

        if (!ChunkDatas.ContainsKey(chunkCoords))
        {
            GD.PushWarning($"UpdateTile: chunk {chunkCoords} not found");
            return;
        }

        ChunkData chunkData = ChunkDatas[chunkCoords];
        TileTerrain tileTerrain = chunkData.GetTerrainData(tileCoords, layer);
        int terrainSet = tileTerrain.terrainSet;
        int terrain = tileTerrain.terrain;

        // 不是地形
        if (terrainSet == -1 || terrain == -1)
        {
            return;
        }

        TilePeering bits = new();
        foreach (Vector2I dir in MapUtils.DIRECTIONS)
        {
            // 如果这是一个四方向地形，那么忽略四个角的检测
            if (MapUtils.TileSet.GetTerrainSetMode(terrainSet) == TileSet.TerrainMode.Sides && dir.X != 0 && dir.Y != 0)
            {
                continue;
            }

            Vector2I neighborCoords = globalTileCoords + dir;
            Vector2I neighborChunkCoords = MapUtils.GlobalTileToChunk(neighborCoords);
            if (!ChunkDatas.ContainsKey(neighborChunkCoords))
            {
                continue;
            }

            ChunkData neighborChunkData = ChunkDatas[neighborChunkCoords];
            Vector2I neighborTileCoords = MapUtils.GlobalTileToLocal(neighborCoords);
            TileTerrain neighborTerrain = neighborChunkData.GetTerrainData(neighborTileCoords, layer);

            // 如果周围瓷砖的地形集和地形和当前一样，那就将这个方向连接情况设置为连接
            if (terrainSet == neighborTerrain.terrainSet && terrain == neighborTerrain.terrain)
            {
                bits.Set(dir, 1);
            }
        }

        NormalizeBit(bits);

        int id = GetTileId(terrainSet, terrain,
            bits.top, bits.topRight, bits.right, bits.bottomRight, bits.bottom, bits.bottomLeft, bits.left, bits.topLeft);
        if (TerrainTilesData.TryGetValue(id, out TileData tileData))
        {
            int sourceId = (int)tileData.GetMeta("sourceId");
            Vector2I atlasCoords = (Vector2I)tileData.GetMeta("atlasCoords");
            int alternative = (int)tileData.GetMeta("alternative");
            chunkData.SetTileData(tileCoords, layer, sourceId, atlasCoords, alternative, modified);
        }
        else
        {
            GD.PushWarning($"UpdateTile: tile {id} not found");
        }
    }
}