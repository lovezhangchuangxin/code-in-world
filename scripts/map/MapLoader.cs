using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
    public Vector2I ChunkSize { get; set; } = new(16, 16);

    /// <summary>
    /// 区块的父节点，所有区块节点放在这个节点下
    /// </summary>
    [Export]
    public Node2D ChunkParent { get; set; } = new();

    /// <summary>
    /// 世界节点，世界的所有物体都放在这个节点下
    /// </summary>
    [Export]
    public Node2D World { get; set; } = new();

    /// <summary>
    /// 地图的瓦片集
    /// </summary>
    [Export]
    public TileSet TileSet { get; set; }

    /// <summary>
    /// 地图生成器
    /// </summary>
    [Export]
    public MapGenerator MapGenerator { get; set; }

    /// <summary>
    /// 区块额外加载的范围，即在可视区之外会刻意多加载几圈区块
    /// </summary>
    [Export]
    public int ExtraLoadRange { get; set; } = 2;

    /// <summary>
    /// 瓦片层的数量
    /// </summary>
    [Export]
    public int LayerCount { get; set; } = 1;

    /// <summary>
    /// 需要进行 y 排序的层
    /// </summary>
    [Export]
    public int[] YSortLayers { get; set; } = [];

    /// <summary>
    /// 自动瓦片
    /// </summary>
    public AutoTile AutoTile { get; set; } = new();

    /// <summary>
    /// 当前区块坐标
    /// </summary>
    public Vector2I CurrentChunkCoord { get; set; } = new(0, 0);

    /// <summary>
    /// 是否是初次加载地图
    /// </summary>
    private bool _Initialize = true;

    /// <summary>
    /// 初始要加载的区块坐标
    /// </summary>
    private List<Vector2I> _InitialChunkCoords = [];

    /// <summary>
    /// 已经加载的区块坐标
    /// </summary>
    public HashSet<Vector2I> LoadedChunkCoords { get; set; } = [];

    /// <summary>
    /// 已经加载的区块节点，键是区块坐标，值是区块节点
    /// </summary>
    private Dictionary<Vector2I, ChunkNode> LoadedChunkNodes { get; set; } = [];

    /// <summary>
    /// 更新队列，每个元素的 0 号位置表示需要加载的区块坐标，1 号位置表示需要卸载的区块坐标
    /// </summary>
    private List<List<Vector2I>[]> UpdateQueue { get; set; } = [];

    /// <summary>
    /// 目标位置，以该位置为中心加载区块
    /// </summary>
    public Vector2 TargetPosition { get; set; } = new(0, 0);

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

    public override void _EnterTree()
    {
        // 将自己传递到全局
        MapUtils.MapLoader = this;
    }

    public override void _Ready()
    {
        MapUtils.TileSet = TileSet;
        MapUtils.ChunkPixelSize = ChunkSize * TileSet.TileSize;
        AutoTile.Build();
    }

    public override void _Process(double delta)
    {
        Vector2I chunkCoord = MapUtils.GlobalPixelToChunk(TargetPosition);
        if (chunkCoord != CurrentChunkCoord || _Initialize)
        {
            // 目标区块坐标发生变化，重新加载区块
            CurrentChunkCoord = chunkCoord;
            UpdateChunk();

            // 暂时在本线程处理加载和卸载区块，后续使用多线程
            ProcessUpdateQueue();
        }
    }

    /// <summary>
    /// 更新区块，计算需要加载和卸载的区块
    /// </summary>
    public void UpdateChunk()
    {
        Vector2I chunkCoord = CurrentChunkCoord;
        Vector2I extraLoadRange = new(ExtraLoadRange, ExtraLoadRange);
        Vector2I minChunkCoord = chunkCoord - extraLoadRange;
        Vector2I maxChunkCoord = chunkCoord + extraLoadRange;
        // 需要加载和卸载的区块坐标
        List<Vector2I> loadCoors = [];
        List<Vector2I> unloadCoors = [];

        for (int x = minChunkCoord.X; x <= maxChunkCoord.X; x++)
        {
            for (int y = minChunkCoord.Y; y <= maxChunkCoord.Y; y++)
            {
                Vector2I coord = new(x, y);
                if (!LoadedChunkCoords.Contains(coord))
                {
                    loadCoors.Add(coord);
                }
            }
        }

        // 卸载不在范围内的区块
        foreach (var chunkNode in LoadedChunkNodes)
        {
            Vector2I coord = chunkNode.Key;
            if (coord.X < minChunkCoord.X || coord.X > maxChunkCoord.X || coord.Y < minChunkCoord.Y || coord.Y > maxChunkCoord.Y)
            {
                unloadCoors.Add(coord);
            }
        }

        // 将需要加载的坐标添加到已加载的坐标中‘
        foreach (var coord in loadCoors)
        {
            LoadedChunkCoords.Add(coord);
        }

        // 将需要卸载的坐标从已加载的坐标中删除
        foreach (var coord in unloadCoors)
        {
            LoadedChunkCoords.Remove(coord);
        }

        if (_Initialize)
        {
            _Initialize = false;
            _InitialChunkCoords = [.. loadCoors];
        }

        UpdateQueue.Add([loadCoors, unloadCoors]);
    }

    /// <summary>
    /// 处理加载和卸载区块的逻辑
    /// </summary>
    public void ProcessUpdateQueue()
    {
        if (UpdateQueue.Count == 0)
        {
            return;
        }

        var updateData = UpdateQueue[0];
        UpdateQueue.RemoveAt(0);
        List<Vector2I> loadCoors = updateData[0];
        List<Vector2I> unloadCoors = updateData[1];

        // 加载区块
        foreach (var coord in loadCoors)
        {
            ChunkData chunkData = new()
            {
                ChunkCoord = coord,
                ChunkSize = ChunkSize,
            };
            chunkData.Init(LayerCount);

            MapGenerator.GenerateChunkData(chunkData);
            CallDeferred(nameof(AddChunkNode), chunkData);
        }

        // 卸载区块
        foreach (var coord in unloadCoors)
        {
            CallDeferred(nameof(RemoveChunkNode), coord);
        }
    }

    /// <summary>
    /// 添加区块节点
    /// </summary>
    public void AddChunkNode(ChunkData chunkData)
    {
        // 如果初始区块不为空，则从中移除当前区块坐标
        if (_InitialChunkCoords.Count > 0)
        {
            _InitialChunkCoords.Remove(chunkData.ChunkCoord);
            if (_InitialChunkCoords.Count == 0)
            {
                // 通知地图初始化完成
                EmitSignal(SignalName.MapInit);
            }
        }

        // 使用区块数据创建区块节点
        ChunkNode chunkNode = new()
        {
            ChunkData = chunkData,
            YSortLayers = YSortLayers,
        };

        ChunkParent.AddChild(chunkNode);
        LoadedChunkNodes.Add(chunkData.ChunkCoord, chunkNode);

        // 通知区块加载完成
        EmitSignal(SignalName.ChunkLoaded, chunkData.ChunkCoord);
    }

    /// <summary>
    /// 卸载区块节点
    /// </summary>
    public void RemoveChunkNode(Vector2I chunkCoord)
    {
        if (LoadedChunkNodes.TryGetValue(chunkCoord, out ChunkNode chunkNode))
        {
            LoadedChunkNodes.Remove(chunkCoord);
            chunkNode.QueueFree();

            // 通知区块卸载完成
            EmitSignal(SignalName.ChunkUnloaded, chunkCoord);
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
    }
}