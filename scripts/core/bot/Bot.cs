using Godot;

public partial class Bot : CharacterBody2D
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

    public override void _Ready()
    {
        Game.Bots[Id] = this;
    }

    public override void _ExitTree()
    {
        Game.Bots.Remove(Id);
    }

    public static Bot CreateBot(string botName)
    {
        Bot bot = GD.Load<PackedScene>(BotScenePath).Instantiate<Bot>();
        bot.BotName = botName;
        bot.Id = new RandomNumberGenerator().RandiRange(0, 1000000);
        bot.SetPosition(new Vector2(0, 0));
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
    public void Move(int direction, float speed = 100)
    {
        Bot bot = Game.Bots[Id];
        Vector2 velocity = bot.Velocity;
        switch (direction)
        {
            case 0:
                velocity = new Vector2(-speed, 0);
                break;
            case 1:
                velocity = new Vector2(speed, 0);
                break;
            case 2:
                velocity = new Vector2(0, -speed);
                break;
            case 3:
                velocity = new Vector2(0, speed);
                break;
            default:
                velocity = new Vector2(0, 0);
                break;
        }
        bot.Velocity = velocity;
        bot.MoveAndSlide();
    }
}