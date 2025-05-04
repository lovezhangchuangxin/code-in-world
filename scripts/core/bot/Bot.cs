using Godot;

public partial class Bot : Sprite2D
{
    /// <summary>
    /// 机器人场景路径
    /// </summary>
    public static string BotScenePath { get; set; } = "res://scenes/objects/bots/Bot.tscn";

    /// <summary>
    /// 机器人 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 机器人名称
    /// </summary>
    public string BotName { get; set; }

    /// <summary>
    /// 补间动画
    /// </summary>
    private Tween _tween = null;

    /// <summary>
    /// 补间动画
    /// </summary>
    public Tween Tween
    {
        get => _tween ??= CreateTween();
        set => _tween = value;
    }

    public override void _Ready()
    {
        Game.Bots[Id] = this;
    }

    public override void _ExitTree()
    {
        Game.Bots.Remove(Id);
    }

    /// <summary>
    /// 移动
    /// </summary>
    public void Move(Vector2I pace)
    {
        Vector2I pos = GetPos();
        Vector2I newPos = pos + pace;
        Vector2I targetPos = MapUtils.GlobalTileToPixel(newPos);
        UseTween("global_position", (Vector2)targetPos, Game.TickTime);
    }

    /// <summary>
    /// 返回当前坐标
    /// </summary>
    public Vector2I GetPos()
    {
        return MapUtils.GlobalPixelToTile((Vector2I)GlobalPosition);
    }

    /// <summary>
    /// 使用补间动画
    /// </summary>
    public void UseTween(NodePath property, Variant finalValue, double duration)
    {
        Tween.TweenProperty(this, property, finalValue, duration);
        Tween.TweenCallback(Callable.From(() => Tween = null)).SetDelay(Game.TickTime);
    }

    /// <summary>
    /// 创建一个新的机器人
    /// </summary>
    public static Bot CreateBot(string botName, string color)
    {
        Bot bot = GD.Load<PackedScene>(BotScenePath).Instantiate<Bot>();
        bot.BotName = botName;
        bot.Id = new RandomNumberGenerator().RandiRange(0, 1000000);
        bot.SetPosition(new Vector2(0, 0));
        bot.Modulate = new Color(color);

        return bot;
    }
}

public class BotApi
{
    public int Id { get; set; }

    public string BotName { get; set; }

    public BotApi(int id, string botName)
    {
        Id = id;
        BotName = botName;
    }

    /// <summary>
    /// 机器人移动
    /// </summary>
    public void Move(Direction direction)
    {
        Bot bot = Game.Bots[Id];
        Vector2I pace = direction switch
        {
            Direction.Top => new Vector2I(0, -1),
            Direction.TopRight => new Vector2I(1, -1),
            Direction.Right => new Vector2I(1, 0),
            Direction.BottomRight => new Vector2I(1, 1),
            Direction.Bottom => new Vector2I(0, 1),
            Direction.BottomLeft => new Vector2I(-1, 1),
            Direction.Left => new Vector2I(-1, 0),
            Direction.TopLeft => new Vector2I(-1, -1),
            _ => new Vector2I(0, 0),
        };
        bot.Move(pace);
    }
}