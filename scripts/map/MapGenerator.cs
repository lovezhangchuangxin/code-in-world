using Godot;

public partial class MapGenerator : Node
{
    /// <summary>
    /// 种子
    /// </summary>
    [Export]
    public int Seed { get; set; } = 0;

    /// <summary>
    /// 地形噪声
    /// </summary>
    [Export]
    public FastNoiseLite TerrainNoise { get; set; } = new();

    /// <summary>
    /// 资源噪声
    /// </summary>
    [Export]
    public FastNoiseLite ResourceNoise { get; set; } = new();

    /// <summary>
    /// 资源深度噪声
    /// </summary>
    [Export]
    public FastNoiseLite ResourceDepthNoise { get; set; } = new();

    public override void _Ready()
    {
        int seed1 = GenerateRandomSeed(Seed);
        int seed2 = GenerateRandomSeed(seed1);
        int seed3 = GenerateRandomSeed(seed2);

        // 设置噪声种子
        TerrainNoise.SetSeed(seed1);
        ResourceNoise.SetSeed(seed2);
        ResourceDepthNoise.SetSeed(seed3);

        // 设置噪声频率
        TerrainNoise.SetFrequency(0.02f);
        ResourceNoise.SetFrequency(0.1f);
        ResourceDepthNoise.SetFrequency(0.1f);

        // 设置噪声类型
        TerrainNoise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.SimplexSmooth);
        ResourceNoise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.Simplex);
        ResourceDepthNoise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.Simplex);
    }

    /// <summary>
    /// 生成区块数据
    /// </summary>
    public void GenerateChunkData(ChunkData chunkData)
    {
        for (int x = 0; x < chunkData.ChunkSize.X; x++)
        {
            for (int y = 0; y < chunkData.ChunkSize.Y; y++)
            {
                Vector2I tileCoord = new Vector2I(x, y);
                Vector2I globalTileCoord = MapUtils.ChunkToGlobalTile(chunkData.ChunkCoord) + tileCoord;

                // 计算基础地形噪声
                float terrainNoise = TerrainNoise.GetNoise2Dv(globalTileCoord);
                float resourceNoise = ResourceNoise.GetNoise2Dv(globalTileCoord);
                float resourceDepthNoise = ResourceDepthNoise.GetNoise2Dv(globalTileCoord);

                // 生成基础地形
                TerrainType terrain;

                // 根据噪声值确定基础地形类型
                if (terrainNoise < -0.4f)
                {
                    terrain = TerrainType.Water;
                }
                else if (terrainNoise < -0.3f)
                {
                    terrain = TerrainType.Sand;
                }
                else if (terrainNoise < 0.4f)
                {
                    terrain = TerrainType.Ground;
                }
                else if (terrainNoise < 0.5f)
                {
                    terrain = TerrainType.Swamp;
                }
                else
                {
                    terrain = TerrainType.Wall;
                }

                // 只在可通行的地形上生成资源 
                if (terrain == TerrainType.Ground)
                {
                    // 确定是否生成资源及资源类型
                    if (resourceDepthNoise < -0.5f)
                    {
                        if (resourceNoise > 0.5f)
                        {
                            // 铜矿分布
                            terrain = TerrainType.Copper;
                        }
                    }
                    else if (resourceDepthNoise < 0f)
                    {
                        if (resourceNoise > 0.7f)
                        {
                            // 铁矿分布
                            terrain = TerrainType.Iron;
                        }
                    }
                    else if (resourceDepthNoise < 0.5f)
                    {
                        if (resourceNoise > 0.5f)
                        {
                            // 煤矿分布
                            terrain = TerrainType.Coal;
                        }
                    }
                }

                chunkData.SetTileData(tileCoord, 0, terrain, false);
            }
        }
    }

    /// <summary>
    /// 生成随机种子
    /// </summary>
    private int GenerateRandomSeed(int seed)
    {
        return (1103515245 * seed + 12345) & 0x7FFFFFFF;
    }
}
