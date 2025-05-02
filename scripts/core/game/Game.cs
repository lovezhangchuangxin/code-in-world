using System.Collections.Generic;

/// <summary>
/// 游戏全局对象
/// </summary>
public static class Game
{
    /// <summary>
    /// 游戏 tick 数
    /// </summary>
    public static int Tick { get; set; } = 0;

    /// <summary>
    /// 玩家的所有机器人
    /// </summary>
    public static Dictionary<int, Bot> Bots { get; set; } = [];

    /// <summary>
    /// 往引擎上设置 api
    /// </summary>
    public static void SetEngineApi(Jint.Engine engine)
    {
        engine.SetValue("Game", new GameApi());
    }
}

public class GameApi
{
    /// <summary>
    /// 获取游戏 tick 数
    /// </summary>
    public int GetTick()
    {
        return Game.Tick;
    }

    /// <summary>
    /// 获取玩家的所有机器人
    /// </summary>
    public Dictionary<int, BotApi> GetBots()
    {
        Dictionary<int, BotApi> bots = new();
        foreach (var bot in Game.Bots)
        {
            bots[bot.Key] = new BotApi(bot.Key, bot.Value.BotName);
        }
        return bots;
    }

    /// <summary>
    /// 创建机器人
    /// </summary>
    public bool CreateBot(string botName)
    {
        Bot bot = Bot.CreateBot(botName);
        MapUtils.MapLoader.World.AddChild(bot);
        return true;
    }

    /// <summary>
    /// 删除机器人
    /// </summary>
    public bool DeleteBot(int id)
    {
        if (Game.Bots.ContainsKey(id))
        {
            Game.Bots[id].QueueFree();
            Game.Bots.Remove(id);
            return true;
        }
        return false;
    }
}