using Godot;
using System;

public partial class TestWaterShader : Node2D
{
    public override void _Ready()
    {
        var tileMapLayer = GetNode<TileMapLayer>("TileMapLayer2");
        var data = tileMapLayer.TileMapData;
        GD.Print($"TileMapData: {data.Length} {data[0]} {data[1]} {data[2]} {data[3]} {data[4]} {data[5]} {data[6]} {data[7]} {data[8]} {data[9]} {data[10]} {data[11]} {data[12]} {data[13]}");
    }
}
