using Godot;

public partial class MapGenerator : Node
{
    /// <summary>
    /// 温度噪声
    /// </summary>
    [Export]
    public FastNoiseLite TemperatureNoise { get; set; } = new();

    /// <summary>
    /// 湿度噪声
    /// </summary>
    [Export]
    public FastNoiseLite HumidityNoise { get; set; } = new();

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
                // 计算温度和湿度
                float temperature = TemperatureNoise.GetNoise2Dv(globalTileCoord);
                float humidity = HumidityNoise.GetNoise2Dv(globalTileCoord);
                if (humidity < 0)
                {
                    chunkData.SetTileData(tileCoord, 0, TerrainType.Sand);
                }
                else if (temperature < 0.5)
                {
                    chunkData.SetTileData(tileCoord, 0, TerrainType.Ground);
                }
                else if (temperature < 0.55)
                {
                    chunkData.SetTileData(tileCoord, 0, TerrainType.Swamp);
                }
                else if (temperature < 0.6)
                {
                    chunkData.SetTileData(tileCoord, 0, TerrainType.Ground);
                }
                else if (temperature < 0.7)
                {
                    chunkData.SetTileData(tileCoord, 0, TerrainType.Wall);
                }
                else
                {
                    chunkData.SetTileData(tileCoord, 0, TerrainType.Water);
                }
            }
        }
    }
}
